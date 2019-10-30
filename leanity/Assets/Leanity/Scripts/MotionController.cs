using System;
using UnityEngine;

namespace Lemonity
{
	public class MotionController
	{
		public enum State { Hided = 0, Idle = 1, Grabbing = 2, Pinching = 3 }

		#region Properties
		public IMotionStyle _scaleStyle;
		public IMotionStyle ScaleStyle
		{
			get { return _scaleStyle; }
			set
			{
				_scaleStyle = value;
				InitScaleStyle();
			}
		}

		private IMotionStyle _grabStyle;
		public IMotionStyle GrabStyle
		{
			get { return _grabStyle; }
			set
			{
				_grabStyle = value;
				InitGrabStyle();
			}
		}

		public Vector3 Position
		{
			get
			{
				return IsDualPinching ? ScaleStyle.Position : GrabStyle.Position;
			}
			set
			{
				GrabStyle.Position = value;
				ScaleStyle.Position = value;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return IsDualPinching ? ScaleStyle.Rotation : GrabStyle.Rotation;
			}
			set
			{
				GrabStyle.Rotation = value;
				ScaleStyle.Rotation = value;
			}
		}

		public float Scale
		{
			get { return ScaleStyle.Scale; }
			set {
				ScaleStyle.Scale = value;
				GrabStyle.Scale = value;
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

			GrabStyle?.OptionsChange();
			ScaleStyle?.OptionsChange();
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

				GrabStyle.LateFrameUpdate();
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
			GrabStyle?.StopInertia();
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
			GrabStyle.LeftGesture = LeftGrab;
			GrabStyle.RightGesture = RightGrab;
			GrabStyle.OptionsChange();
		}

		private void InitScaleStyle()
		{
			ScaleStyle.LeftGesture = LeftPinch;
			ScaleStyle.RightGesture = RightPinch;
		}

		private void LoadMotionStyle()
		{
			switch (_currentGesture)
			{
				case WorkingGesture.OneHand:
					GrabStyle = new OneHandMotion();
					ScaleStyle = new ScaleMotion();
					break;

				case WorkingGesture.TwoHands:
					GrabStyle = new TwoHandsMotion();
					ScaleStyle = new ScaleMotion();
					break;

				case WorkingGesture.Hybrid:
				default:
					GrabStyle = new HybridMotion();
					ScaleStyle = new ScaleMotion();
					break;

				case WorkingGesture.Orbit:
					GrabStyle = new OrbitMotion();
					ScaleStyle = new NullMotion();
					break;
			}
		}

		private bool EventController()
		{
			bool grabbingUpdate = false;

			if (GrabStyle != null && GrabStyle.RequiresTwoHands)
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
						bool inertialMoving = GrabStyle.InertialMovement();
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
							bool inertialMoving = GrabStyle.InertialMovement();
							return inertialMoving;
						}
					}
					break;

				case State.Grabbing:
					if (grabbingUpdate)
					{
						GrabStyle.Update();
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
						ScaleStyle.Update();
						return true;
					}
					break;
			}

			return false;
		}

		private void StartGrabbing()
		{
			if (GrabStyle != null)
			{
				if (GrabStyle.RequiresTwoHands)
				{
					LeftGrab.Reset();
					RightGrab.Reset();
				}
				GrabStyle.Start();
			}
			OnStartGrab?.Invoke();
			OnStateChange?.Invoke();
		}

		private void StopGrabbing()
		{
			GrabStyle?.Stop();
			OnEndGrab?.Invoke();
			OnStateChange?.Invoke();
		}

		private void StartPinching()
		{
			StopInertia();
			ScaleStyle.Start();
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