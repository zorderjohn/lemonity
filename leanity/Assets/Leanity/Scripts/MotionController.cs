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

		public static MotionController LatestInstance { get; private set; }

		public GrabController LeftGrab { get; private set; }
		public GrabController RightGrab { get; private set; }

		public bool IsHolding { get; private set; } = false;

		private InertialObject _inertialData;
		private WorkingGesture _currentGesture;
		private float _lastFrameTime = 0f;

		public MotionController()
		{
			Options.Load();
			Options.OnOptionsChange += OnOptionsChanged;

			LeftGrab = new GrabController();
			RightGrab = new GrabController();

			// Always instantiate after Left and Right grabs
			_currentGesture = Options.Gesture;
			LoadMotionStyle();

			_inertialData = new InertialObject(Options.VelocityFrames);
			LatestInstance = this;
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
			}
		}

		public void OnOptionsChanged()
		{
			if (_currentGesture != Options.Gesture)
			{
				_currentGesture = Options.Gesture;
				LoadMotionStyle();
			}

			if (_motionStyle != null)
			{
				_motionStyle.InvertAxis = Options.InvertAxis;
			}

			_inertialData.BufferLength = Options.VelocityFrames;
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

				GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
				GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);

				bool returnValue = EventController();

				_lastFrameTime = GetTime();
				return returnValue;
			}
			return false;
		}

		private void InitMotionStyle()
		{
			_motionStyle.LeftGrab = LeftGrab;
			_motionStyle.RightGrab = RightGrab;
			_motionStyle.InvertAxis = Options.InvertAxis;
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
				float t = GetTime();
				_inertialData.SetPosition(Position, t);
				_inertialData.SetRotation(Rotation, t);

				MotionStyle.Update();

				// Only for debugging purposes
				_inertialData.CalculateAngularVelocity();
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
					return InertialMove();
				}
			}

			return false;
		}


		private bool InertialMove()
		{
			float deltaTime = GetDeltaTime();

			var linearVelocity = _inertialData.LinearVelocity;
			linearVelocity *= Options.LinearDrag;
			_inertialData.LinearVelocity = linearVelocity;


			Position += linearVelocity * deltaTime;
			_inertialData.SetPosition(Position, GetTime());

			Vector3 eulerVelocity = _inertialData.AngularVelocityEuler * Options.AngularDrag;

			// Up vector always pointing up
			eulerVelocity.z = 0;
			_inertialData.AngularVelocityEuler = eulerVelocity;

			Quaternion deltaRotation = Quaternion.Euler(eulerVelocity * deltaTime);

			Quaternion orientation = deltaRotation * Rotation;

			if (IsCamera)
			{
				orientation.eulerAngles = MathHelper.ClampEulerRotationXZ(orientation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
			}

			Rotation = orientation;

			return linearVelocity.sqrMagnitude >= Vector3.kEpsilonNormalSqrt ||
				   eulerVelocity.sqrMagnitude >= Vector3.kEpsilonNormalSqrt;
		}

		private void StartMoving()
		{
			_inertialData.Clear();

			if (MotionStyle != null && MotionStyle.RequiresTwoHands)
			{
				//TODO: Only on two hand gestures
				LeftGrab.Reset();
				RightGrab.Reset();
			}
		}

		private void StopMoving()
		{
			_inertialData.DiscardFrames(Options.DiscardFrames);
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}

		private float GetTime()
		{
			return Time.realtimeSinceStartup;
		}

		private float GetDeltaTime()
		{
			return Time.realtimeSinceStartup - _lastFrameTime;
		}
	}
}