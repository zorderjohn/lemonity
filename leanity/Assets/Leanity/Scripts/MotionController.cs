using System;
using UnityEngine;

namespace Lemonity
{
	public class MotionController
	{
		public enum State { Hided = 0, Idle = 1, Grabbing = 2, Pinching = 3 }

		#region Properties
		public IMotionStyle _pinchMotion;
		public IMotionStyle PinchMotion
		{
			get { return _pinchMotion; }
			set
			{
				_pinchMotion = value;
				InitScaleStyle();
			}
		}

		private IMotionStyle _grabMotion;
		public IMotionStyle GrabMotion
		{
			get { return _grabMotion; }
			set
			{
				_grabMotion = value;
				InitGrabStyle();
			}
		}

		public Vector3 Position
		{
			get
			{
				return IsDualPinching ? PinchMotion.Position : GrabMotion.Position;
			}
			set
			{
				GrabMotion.Position = value;
				PinchMotion.Position = value;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return IsDualPinching ? PinchMotion.Rotation : GrabMotion.Rotation;
			}
			set
			{
				GrabMotion.Rotation = value;
				PinchMotion.Rotation = value;
			}
		}

		public float Scale
		{
			get { return PinchMotion.Scale; }
			set {
				PinchMotion.Scale = value;
				GrabMotion.Scale = value;
			}
		}
		public GrabController LeftGrab { get; private set; }
		public GrabController RightGrab { get; private set; }

		public PinchController LeftPinch { get; private set; }
		public PinchController RightPinch { get; private set; }

		public State MotionState { get; private set; }
		public bool IsGrabbing => MotionState == State.Grabbing;
		public bool IsDualPinching => MotionState == State.Pinching;
		#endregion

		#region Actions
		public event Action OnHandsVisible;
		public event Action OnHandsInVisible;
		public event Action OnStartPinch;
		public event Action OnEndPinch;
		public event Action OnStartGrab;
		public event Action OnEndGrab;
		public event Action OnStateChange;
		#endregion

		#region Private Fields
		private WorkingGesture _currentGesture;
		#endregion

		public MotionController()
		{
			Options.Load();
			Options.OnOptionsChange += OnOptionsChange;

			LeftGrab = new GrabController();
			RightGrab = new GrabController();

			LeftPinch = new PinchController();
			RightPinch = new PinchController();

			// Always instantiate after Left and Right grabs
			_currentGesture = Options.Gesture;
			LoadMotionStyle();
			Scale = 0f;
		}

		public void OnOptionsChange()
		{
			if (_currentGesture != Options.Gesture)
			{
				_currentGesture = Options.Gesture;
				LoadMotionStyle();
			}

			GrabMotion?.OptionsChange();
			PinchMotion?.OptionsChange();
		}

		public bool Update(Vector3 position, Quaternion rotation, float scale)
		{
			Position = position;
			Rotation = rotation;
			Scale = scale;

			if (HandTracking.Update())
			{
				LeftGrab.Update(HandTracking.LeftHandData, Position, Rotation);
				RightGrab.Update(HandTracking.RightHandData, Position, Rotation);

				LeftPinch.Update(HandTracking.LeftHandData, Position, Rotation);
				RightPinch.Update(HandTracking.RightHandData, Position, Rotation);

				bool retValue = EventController();

				HandTracking.TransformPosition = Position + Rotation * HandTracking.CamToHandOffset();
				HandTracking.TransformRotation = Rotation;
				//HandTracking.TransformScale = Scale;
				HandTracking.TransformScale = Options.PosScale;
				return retValue;
			}

			return false;
		}

		public void StopInertia()
		{
			GrabMotion?.StopInertia();
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

		private void InitGrabStyle()
		{
			GrabMotion.LeftGesture = LeftGrab;
			GrabMotion.RightGesture = RightGrab;
			GrabMotion.OptionsChange();
		}

		private void InitScaleStyle()
		{
			PinchMotion.LeftGesture = LeftPinch;
			PinchMotion.RightGesture = RightPinch;
		}

		private void LoadMotionStyle()
		{
			switch (_currentGesture)
			{
				case WorkingGesture.OneHand:
					GrabMotion = new OneHandMotion();
					PinchMotion = new ScaleMotion();
					break;

				case WorkingGesture.TwoHands:
					GrabMotion = new TwoHandsMotion();
					PinchMotion = new ScaleMotion();
					break;

				case WorkingGesture.Hybrid:
				default:
					GrabMotion = new HybridMotion();
					PinchMotion = new ScaleMotion();
					break;

				case WorkingGesture.Orbit:
					GrabMotion = new OrbitMotion();
					PinchMotion = new NullMotion();
					break;
			}
		}

		private bool EventController()
		{
			bool grabbingUpdate = false;

			if (GrabMotion != null && GrabMotion.RequiresTwoHands)
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
				case State.Hided:
					if (!isHiding)
					{
						MotionState = State.Idle;
						OnHandsVisible?.Invoke();
						OnStateChange?.Invoke();
					}
					if (!Options.StopIfNotVisible && Options.EnableInertia)
					{
						bool inertialMoving = GrabMotion.InertialMovement();
						return inertialMoving;
					}
					break;

				case State.Idle:
					if (isHiding)
					{
						MotionState = State.Hided;
						OnHandsInVisible?.Invoke();
						OnStateChange?.Invoke();
					}
					if (grabbingUpdate)
					{
						MotionState = State.Grabbing;
						StartGrabbing();
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
							bool inertialMoving = GrabMotion.InertialMovement();
							return inertialMoving;
						}
					}
					break;

				case State.Grabbing:
					if (grabbingUpdate)
					{
						GrabMotion.Update();
						return true;
					}
					else
					{
						StopGrabbing();

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
						StartGrabbing();
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
						PinchMotion.Update();
						return true;
					}
					break;
			}

			return false;
		}

		private void StartGrabbing()
		{
			if (GrabMotion != null)
			{
				if (GrabMotion.RequiresTwoHands)
				{
					LeftGrab.Reset();
					RightGrab.Reset();
				}
				GrabMotion.Start();
			}
			OnStartGrab?.Invoke();
			OnStateChange?.Invoke();
		}

		private void StopGrabbing()
		{
			GrabMotion?.Stop();
			OnEndGrab?.Invoke();
			OnStateChange?.Invoke();
		}

		private void StartPinching()
		{
			StopInertia();
			PinchMotion.Start();
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