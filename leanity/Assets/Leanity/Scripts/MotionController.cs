using System;
using UnityEngine;

namespace Leanity
{
	public class MotionController
	{
		public enum State { Hiding = 0, Idle = 1, Grabbing = 2, Pinching = 3 }

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

		public State MotionState { get; private set; }

		public bool IsGrabbing => MotionState == State.Grabbing;
		public bool IsDualPinching => MotionState == State.Pinching;

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

				case WorkingGesture.Orbit:
					MotionStyle = new OrbitMotion();
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

				bool retValue = EventController();

				HandTracking.TransformPosition = Position + Rotation * HandTracking.CamToHandOffset();
				HandTracking.TransformRotation = Rotation;
				HandTracking.TransformScale = Options.PosScale;

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

		public State GetHandState(bool isRightHand)
		{
			State handState = State.Idle;
			if (isRightHand)
			{
				if (RightGrab.IsHolding)
				{
					handState = State.Grabbing;
				}
				else if (RightPinch.IsHolding)
				{
					handState = State.Pinching;
				}
			}
			else
			{
				if (LeftGrab.IsHolding)
				{
					handState = State.Grabbing;
				}
				else if (LeftPinch.IsHolding)
				{
					handState = State.Pinching;
				}
			}

			return handState;
		}

		public IGestureController GetCurrentGestureController(bool isRightHand)
		{
			if (isRightHand)
			{
				if (RightGrab.IsHolding)
				{
					return RightGrab;
				}
				else
				{
					return RightPinch;
				}
			} else
			{
				if (LeftGrab.IsHolding)
				{
					return LeftGrab;
				}
				else
				{
					return LeftPinch;
				}
			}

		}

		private void InitMotionStyle()
		{
			_motionStyle.LeftGesture = LeftGrab;
			_motionStyle.RightGesture = RightGrab;
			_motionStyle.OptionsChange();

			_scaleStyle.LeftGesture = LeftPinch;
			_scaleStyle.RightGesture = RightPinch;
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

			bool dualPinchingUpdate = Options.PinchEnabled && LeftPinch.IsHolding && RightPinch.IsHolding;

			bool isHiding = !HandTracking.LeftHandData.Detected && !HandTracking.RightHandData.Detected;

			switch (MotionState)
			{
				case State.Hiding:
					if (!isHiding)
					{
						MotionState = State.Idle;
						OnHandsVisible?.Invoke();
						OnStateChange?.Invoke();
					}
					break;

				case State.Idle:
					if (isHiding)
					{
						MotionState = State.Hiding;
						OnHandsInVisible?.Invoke();
						OnStateChange?.Invoke();
					}
					if (grabbingUpdate)
					{
						MotionState = State.Grabbing;
						StartMoving();
						OnStateChange?.Invoke();
					}
					else if (dualPinchingUpdate)
					{
						MotionState = State.Pinching;
						StartPinching();
						OnStateChange?.Invoke();
					}
					else
					{
						if (Options.EnableInertia)
						{
							bool inertialMoving = MotionStyle.InertialMovement();
							return inertialMoving;
						}
					}
					break;

				case State.Grabbing:
					if (grabbingUpdate)
					{
						MotionStyle.Update();
						return true;
					}
					else
					{
						StopMoving();

						if (dualPinchingUpdate)
						{
							MotionState = State.Pinching;
							StartPinching();
							OnStateChange?.Invoke();
						}
						else
						{
							MotionState = State.Idle;
							OnStateChange?.Invoke();
						}
					}
					break;

				case State.Pinching:
					if (grabbingUpdate)
					{
						StopPinching();
						MotionState = State.Grabbing;
						StartMoving();
						OnStateChange?.Invoke();
					}
					else if (!dualPinchingUpdate)
					{
						StopPinching();
						MotionState = State.Idle;
						OnStateChange?.Invoke();
					}
					else
					{
						_scaleStyle.Update();
						return true;
					}
					break;
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