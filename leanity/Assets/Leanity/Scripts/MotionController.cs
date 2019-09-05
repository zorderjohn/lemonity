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

		private bool _isHolding = false;
		private InertialObject _inertialData;
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
		}

		public bool Update(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;

			HandTracking.TransformPosition = position + rotation * HandTracking.CamToHandOffset();
			HandTracking.TransformRotation = rotation;
			HandTracking.TransformScale = Options.PosScale;
			HandTracking.Update();

			LeftGrab.Update(HandTracking.LeftHandData, Position, Rotation);
			RightGrab.Update(HandTracking.RightHandData, Position, Rotation);

			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1000);

			return EventController();
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
				if (!_isHolding)
				{
					_isHolding = true;
					StartMoving();
				}
				float t = Time.time;
				_inertialData.SetPosition(Position, t);
				_inertialData.SetRotation(Rotation, t);

				MotionStyle.Update();

				// Only for debugging purposes
				_inertialData.CalculateAngularVelocity();
				return true;
			}
			else
			{
				if (_isHolding)
				{
					_isHolding = false;
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
			float deltaTime = Time.deltaTime;

			var linearVelocity = _inertialData.LinearVelocity;
			linearVelocity *= Options.LinearDrag;
			_inertialData.LinearVelocity = linearVelocity;


			Position += linearVelocity * deltaTime;
			_inertialData.SetPosition(Position, Time.time);

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
	}
}