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

		public float Scale { get; private set; }

		public event Action OnHandsVisible;
		public event Action OnHandsInVisible;
		public event Action OnStartDualPinch;
		public event Action OnEndDualPinch;

		public GrabController LeftGrab { get; private set; }
		public GrabController RightGrab { get; private set; }

		public PinchController LeftPinch { get; private set; }
		public PinchController RightPinch { get; private set; }

		public bool IsHolding { get; private set; } = false;
		public bool IsPinching { get; private set; } = false;

		private bool _handsVisible = false;
		private WorkingGesture _currentGesture;

		private float _initialScale;


		public MotionController()
		{
			Options.Load();
			Options.OnOptionsChange += OnOptionsChanged;

			LeftGrab = new GrabController();
			RightGrab = new GrabController();

			LeftPinch = new PinchController();
			RightPinch = new PinchController();
			Scale = 0f;

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

				LeftPinch.Update(HandTracking.LeftHandData, Position, Rotation);
				RightPinch.Update(HandTracking.RightHandData, Position, Rotation);

				HandDetectedEvent();

				bool retValue = EventController();
				MotionStyle.LateFrameUpdate();
				return retValue;
			}
			return false;
		}

		public bool ScaleUpdate(float scale)
		{
			if (IsPinching)
			{
				return true;
			}
			else
			{
				Scale = scale;
			}
			return false;
		}

		public void StopInertia()
		{
			MotionStyle?.StopInertia();
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
			else if (_handsVisible && !handsDetected && OnHandsInVisible != null)
			{
				OnHandsInVisible.Invoke();
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

			bool pinching = LeftPinch.IsHolding && RightPinch.IsHolding;

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

				if (IsPinching)
				{
					if (!pinching)
					{
						IsPinching = false;
						StopPinching();
					} else
					{
						PinchingUpdate();
					}
				}
				else
				{
					if (pinching)
					{
						IsPinching = true;
						StartPinching();
					}
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

		private void StartPinching()
		{
			OnStartDualPinch?.Invoke();
			_initialScale = Scale;
		}

		private void PinchingUpdate()
		{
			var initialSep = Vector3.Distance(LeftPinch.HandInitialPosition, RightPinch.HandInitialPosition);
			var finalSep = Vector3.Distance(LeftPinch.HandCurrentPosition, RightPinch.HandCurrentPosition);

			float sep = finalSep - initialSep;
			float sepDelta = 5f * sep * Options.PinchScale;
			Scale = Mathf.Clamp(_initialScale + sepDelta, 0f, 10f);
		}

		private void StopPinching()
		{
			OnEndDualPinch?.Invoke();
		}

	}
}