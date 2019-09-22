using System;
using UnityEngine;

namespace Leanity
{
	public class MotionController
	{
		private IMotionStyle _scaleStyle;
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

		public Vector3 Position
		{
			get
			{
				return IsDualPinching ? _scaleStyle.Position : _motionStyle.Position;
			}
			set
			{
				_motionStyle.Position = value;
				_scaleStyle.Position = value;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return IsDualPinching ? _scaleStyle.Rotation : _motionStyle.Rotation;
			}
			set
			{
				_motionStyle.Rotation = value;
				_scaleStyle.Rotation = value;
			}
		}

		public float Scale
		{
			get { return _scaleStyle.Scale; }
			set {
				_scaleStyle.Scale = value;
				_motionStyle.Scale = value;
			}
		}

		public event Action OnHandsVisible;
		public event Action OnHandsInVisible;
		public event Action OnStartPinch;
		public event Action OnEndPinch;
		public event Action OnStartGrab;
		public event Action OnEndGrab;
		public event Action OnStateChange;

		public GrabController LeftGrab { get; private set; }
		public GrabController RightGrab { get; private set; }

		public PinchController LeftPinch { get; private set; }
		public PinchController RightPinch { get; private set; }

		public bool IsGrabbing { get; private set; } = false;
		public bool IsDualPinching { get; private set; } = false;

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

			// Always instantiate after Left and Right grabs
			_currentGesture = Options.Gesture;
			_scaleStyle = new ScaleMotion();
			LoadMotionStyle();
			Scale = 0f;
		}

		private void LoadMotionStyle()
		{
			switch(_currentGesture)
			{
				case WorkingGesture.OneHand:
					MotionStyle = new OneHandMotion();
					break;

				case WorkingGesture.TwoHands:
					MotionStyle = new TwoHandsMotion();
					break;

				case WorkingGesture.Hybrid:
				default:
					MotionStyle = new HybridMotion();
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

			if (HandTracking.Update())
			{
				LeftGrab.Update(HandTracking.LeftHandData, Position, Rotation);
				RightGrab.Update(HandTracking.RightHandData, Position, Rotation);

				LeftPinch.Update(HandTracking.LeftHandData, Position, Rotation);
				RightPinch.Update(HandTracking.RightHandData, Position, Rotation);

				HandDetectedEvent();

				bool retValue = EventController();

				if (retValue)
				{
					HandTracking.TransformPosition = Position + Rotation * HandTracking.CamToHandOffset();
					HandTracking.TransformRotation = Rotation;
					HandTracking.TransformScale = Options.PosScale;
				}

				MotionStyle.LateFrameUpdate();
				return retValue;
			}

			return false;
		}

		public bool ScaleUpdate(float scale)
		{
			if (IsDualPinching)
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
			_motionStyle.LeftGesture = LeftGrab;
			_motionStyle.RightGesture = RightGrab;
			_motionStyle.OptionsChange();

			_scaleStyle.LeftGesture = LeftPinch;
			_scaleStyle.RightGesture = RightPinch;
		}

		private void HandDetectedEvent()
		{
			bool handsDetected = HandTracking.LeftHandData.Detected || HandTracking.RightHandData.Detected;
			if (!_handsVisible && handsDetected)
			{
				OnHandsVisible?.Invoke();
				OnStateChange?.Invoke();
			}
			else if (_handsVisible && !handsDetected)
			{
				OnHandsInVisible?.Invoke();
				OnStateChange?.Invoke();
			}
			_handsVisible = handsDetected;
		}

		private bool EventController()
		{
			bool grabbingUpdate = false;

			if (_motionStyle != null && _motionStyle.RequiresTwoHands)
			{
				grabbingUpdate = LeftGrab.IsHolding && RightGrab.IsHolding;
			}
			else
			{
				grabbingUpdate = LeftGrab.IsHolding || RightGrab.IsHolding;
			}

			bool dualPinchingUpdate = LeftPinch.IsHolding && RightPinch.IsHolding;

			if (grabbingUpdate)
			{
				if (!IsGrabbing)
				{
					IsGrabbing = true;
					StartMoving();
				}

				MotionStyle.Update();
				return true;
			}
			else
			{
				if (IsGrabbing)
				{
					IsGrabbing = false;
					StopMoving();
				}

				if (dualPinchingUpdate)
				{
					if (!IsDualPinching)
					{
						IsDualPinching = true;
						StartPinching();
					} else
					{
						_scaleStyle.Update();
						return true;
					}
				}
				else
				{
					if (IsDualPinching)
					{
						IsDualPinching = false;
						StopPinching();
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
				if (MotionStyle.RequiresTwoHands)
				{
					LeftGrab.Reset();
					RightGrab.Reset();
				}
				MotionStyle.Start();
			}
			OnStartGrab?.Invoke();
			OnStateChange?.Invoke();
		}

		private void StopMoving()
		{
			MotionStyle?.Stop();
			OnEndGrab?.Invoke();
			OnStateChange?.Invoke();
		}

		private void StartPinching()
		{
			StopInertia();
			_scaleStyle.Start();
			OnStartPinch?.Invoke();
			OnStateChange?.Invoke();
		}

		private void StopPinching()
		{
			OnEndPinch?.Invoke();
			OnStateChange?.Invoke();
		}

	}
}