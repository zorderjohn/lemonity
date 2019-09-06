using System;
using UnityEngine;

namespace Leanity
{
	public class MotionController
	{
		private IMotionStyle _motionStyle;

		public IMotionStyle MotionStyle
		{
			get { return _motionStyle; }
			set
			{
				_motionStyle = value;
				InitMotionStyle();
			}
		}
		public bool IsCamera { get; set; }

		public Vector3 Position
		{
			get { return _motionStyle.Position; }
			set { _motionStyle.Position = value; }
		}
		public Quaternion Rotation
		{
			get { return _motionStyle.Rotation; }
			set { _motionStyle.Rotation = value; }
		}

		public event Action OnHandsVisible;

		public GrabController LeftGrab { get; private set; }
		public GrabController RightGrab { get; private set; }

		public bool IsHolding { get; private set; } = false;

		private bool _handsVisible = false;
		private WorkingGesture _currentGesture;


		public MotionController()
		{
			Options.Load();
			Options.OnOptionsChange += OnOptionsChanged;

			LeftGrab = new GrabController();
			RightGrab = new GrabController();

			// Always instantiate after Left and Right grabs
			_currentGesture = Options.Gesture;
			LoadMotionStyle();
		}

		private void LoadMotionStyle()
		{
			switch(_currentGesture)
			{
				case WorkingGesture.OneHand:
					MotionStyle = new OneHandMotion();
					break;

				case WorkingGesture.TwoHands:
				default:
					MotionStyle = new TwoHandsMotion();
					break;
			}
		}

		public void OnOptionsChanged()
		{
			if (_currentGesture != Options.Gesture)
			{
				_currentGesture = Options.Gesture;
				LoadMotionStyle();
			}

			_motionStyle?.OptionsChange();
		}

		public bool Update(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;

			HandTracking.TransformPosition = position + rotation * HandTracking.CamToHandOffset();
			HandTracking.TransformRotation = rotation;
			HandTracking.TransformScale = Options.PosScale;
			if (HandTracking.Update())
			{
				LeftGrab.Update(HandTracking.LeftHandData, Position, Rotation);
				RightGrab.Update(HandTracking.RightHandData, Position, Rotation);

				HandDetectedEvent();

				bool retValue = EventController();
				MotionStyle.LateFrameUpdate();
				return retValue;
			}
			return false;
		}

		private void InitMotionStyle()
		{
			_motionStyle.LeftGrab = LeftGrab;
			_motionStyle.RightGrab = RightGrab;
			_motionStyle.OptionsChange();
		}

		private void HandDetectedEvent()
		{
			bool handsDetected = HandTracking.LeftHandData.Detected || HandTracking.RightHandData.Detected;
			if (!_handsVisible && handsDetected && OnHandsVisible != null)
			{
				OnHandsVisible.Invoke();
			}
			_handsVisible = handsDetected;
		}

		private bool EventController()
		{
			bool holding = false;

			if (_motionStyle != null && _motionStyle.RequiresTwoHands)
			{
				holding = LeftGrab.IsHolding && RightGrab.IsHolding;
			}
			else
			{
				holding = LeftGrab.IsHolding || RightGrab.IsHolding;
			}

			if (holding)
			{
				if (!IsHolding)
				{
					IsHolding = true;
					StartMoving();
				}

				MotionStyle.Update();
				return true;
			}
			else
			{
				if (IsHolding)
				{
					IsHolding = false;
					StopMoving();
				}
				if (Options.EnableInertia)
				{
					return MotionStyle.InertialUpdate();
				}
			}

			return false;
		}

		private void StartMoving()
		{
			if (MotionStyle != null)
			{
				MotionStyle.Start();

				if (MotionStyle.RequiresTwoHands)
				{
					LeftGrab.Reset();
					RightGrab.Reset();
				}
			}
		}

		private void StopMoving()
		{
			MotionStyle?.Stop();
		}
	}
}