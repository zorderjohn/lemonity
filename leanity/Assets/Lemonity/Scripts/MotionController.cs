using System;
using UnityEngine;

namespace Lemonity
{
	public class MotionController
	{
		public enum State { Hided = 0, Idle = 1, Grabbing = 2, Pinching = 3 }

		#region Properties
		public Runtime MotionRuntime { get; private set; }
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
				return IsPinching ? PinchMotion.Position : GrabMotion.Position;
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
				return IsPinching ? PinchMotion.Rotation : GrabMotion.Rotation;
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

		public float Size
		{
			get
			{
				switch (CurrentMode)
				{
					case WorkingMode.GrabHybrid:
					case WorkingMode.GrabOneHand:
					case WorkingMode.GrabTwoHands:
						return 1f + (Options.PosScale - 1f) * 0.1f;
					case WorkingMode.Orbit:
						return 0.1f * Vector3.Distance(MotionRuntime.SelectionCenter(), Position);
					case WorkingMode.FlyOneHand:
					case WorkingMode.FlyTwoHands:
						return Options.FlyHover ? 1f : 2f;
					default:
						return 1f;
				}
			}
		}

		private WorkingMode _currentMode;
		public WorkingMode CurrentMode
		{
			get { return _currentMode; }
			set
			{
				if (_currentMode != value)
				{
					_currentMode = value;
					LoadMotionStyle();
				}
			}
		}

		public GrabController LeftGrab { get; private set; }
		public GrabController RightGrab { get; private set; }

		public PinchController LeftPinch { get; private set; }
		public PinchController RightPinch { get; private set; }

		public State MotionState { get; private set; }
		public bool IsGrabbing { get { return MotionState == State.Grabbing; } }
		public bool IsPinching { get { return MotionState == State.Pinching; } }
		public static bool MultipleInstances { get { return _instances > 1; } }
		#endregion

		private static int _instances = 0;

		#region Actions
		public event Action OnHandsVisible;
		public event Action OnHandsInVisible;
		public event Action OnStartPinch;
		public event Action OnEndPinch;
		public event Action OnStartGrab;
		public event Action OnEndGrab;
		public event Action OnStateChange;
		#endregion

		public MotionController(WorkingMode mode)
		{
			MotionRuntime = new Runtime();

			Options.Load();
			Options.OnOptionsChange += OnOptionsChange;

			LeftGrab = new GrabController();
			RightGrab = new GrabController();

			LeftPinch = new PinchController();
			RightPinch = new PinchController();

			// Always instantiate after Left and Right grabs
			CurrentMode = mode;
			LoadMotionStyle();
			Scale = 0f;
			_instances++;
		}

		 ~MotionController()
		{
			_instances--;
		}

		public void OnOptionsChange()
		{
            if (GrabMotion != null) GrabMotion.OptionsChange();
			if (PinchMotion != null) PinchMotion.OptionsChange();
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
				HandTracking.TransformScale = Scale;
				return retValue;
			}

			return false;
		}

		public void StopInertia()
		{
			if (GrabMotion != null) GrabMotion.StopInertia();
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
			switch (_currentMode)
			{
				case WorkingMode.GrabOneHand:
					GrabMotion = new OneHandMotion(MotionRuntime);
					PinchMotion = new ScaleMotion(MotionRuntime);
					break;

				case WorkingMode.GrabTwoHands:
					GrabMotion = new TwoHandsMotion(MotionRuntime);
					PinchMotion = new ScaleMotion(MotionRuntime);
					break;

				case WorkingMode.GrabHybrid:
				default:
					GrabMotion = new HybridMotion(MotionRuntime);
					PinchMotion = new ScaleMotion(MotionRuntime);
					break;

				case WorkingMode.Orbit:
					GrabMotion = new OrbitMotion(MotionRuntime);
					PinchMotion = new AlignMotion(MotionRuntime);
					break;

				case WorkingMode.FlyOneHand:
					GrabMotion = new OneHandNoPivotMotion(MotionRuntime);
					PinchMotion = new NullMotion(MotionRuntime);
					break;

				case WorkingMode.FlyTwoHands:
					GrabMotion = new TwoHandsNoPivotMotion(MotionRuntime);
					PinchMotion = new NullMotion(MotionRuntime);
					break;
			}
		}

		private bool EventController()
		{
			bool grabbingUpdate = false;
			bool pinchingUpdate = false;

			if (GrabMotion != null && GrabMotion.RequiresTwoHands)
			{
				grabbingUpdate = LeftGrab.IsHolding && RightGrab.IsHolding;
			}
			else
			{
				grabbingUpdate = LeftGrab.IsHolding || RightGrab.IsHolding;
			}

			if (PinchMotion != null && PinchMotion.RequiresTwoHands)
			{
				pinchingUpdate = Options.PinchEnabled && LeftPinch.IsHolding && RightPinch.IsHolding;
			}
			else
			{
				pinchingUpdate = Options.PinchEnabled && (LeftPinch.IsHolding || RightPinch.IsHolding);
			}

			bool isHiding = !HandTracking.LeftHandData.Detected && !HandTracking.RightHandData.Detected;

			switch (MotionState)
			{
				case State.Hided:
					if (!isHiding)
					{
						MotionState = State.Idle;
						OnHandsVisible.SafeInvoke();
						OnStateChange.SafeInvoke();
					}
					if (!Options.StopIfNotVisible && Options.EnableInertia)
					{
						return GrabMotion.InertialMovement();
					}
					break;

				case State.Idle:
					if (isHiding)
					{
						MotionState = State.Hided;
						OnHandsInVisible.SafeInvoke();
						OnStateChange.SafeInvoke();
					}
					if (grabbingUpdate)
					{
						MotionState = State.Grabbing;
						StartGrabbing();
						OnStateChange.SafeInvoke();
					}
					else if (pinchingUpdate)
					{
						MotionState = State.Pinching;
						StartPinching();
						OnStateChange.SafeInvoke();
					}
					else
					{
						if (Options.EnableInertia)
						{
							return GrabMotion.InertialMovement();
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

						if (pinchingUpdate)
						{
							MotionState = State.Pinching;
							StartPinching();
							OnStateChange.SafeInvoke();
						}
						else
						{
							MotionState = State.Idle;
							OnStateChange.SafeInvoke();
						}
					}
					break;

				case State.Pinching:
					if (grabbingUpdate)
					{
						StopPinching();
						MotionState = State.Grabbing;
						StartGrabbing();
						OnStateChange.SafeInvoke();
					}
					else if (!pinchingUpdate)
					{
						StopPinching();
						MotionState = State.Idle;
						OnStateChange.SafeInvoke();
						return true;
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
			OnStartGrab.SafeInvoke();
			OnStateChange.SafeInvoke();
		}

		private void StopGrabbing()
		{
			GrabMotion.Stop();
			OnEndGrab.SafeInvoke();
			OnStateChange.SafeInvoke();
		}

		private void StartPinching()
		{
			StopInertia();
			PinchMotion.Start();
			OnStartPinch.SafeInvoke();
			OnStateChange.SafeInvoke();
		}

		private void StopPinching()
		{
			PinchMotion.Stop();
			Position = PinchMotion.Position;
			Rotation = PinchMotion.Rotation;
			OnEndPinch.SafeInvoke();
			OnStateChange.SafeInvoke();
		}

	}
}