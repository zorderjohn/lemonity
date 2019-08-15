using UnityEngine;

namespace Leanity
{


	public class MotionController
	{
		private IMotionStyle _motionStyle;
		public IMotionStyle MotionStyle
		{
			get { return _motionStyle; }
			set {
				_motionStyle = value;
				InitMotionStyle();
			}
		}
		public bool IsCamera { get; set; }

		public Vector3 ObjectPosition
		{
			get { return _motionStyle.ObjectPosition; }
			set { _motionStyle.ObjectPosition = value; }
		}
		public Quaternion ObjectRotation
		{
			get { return _motionStyle.ObjectRotation; }
			set { _motionStyle.ObjectRotation = value; }
		}

		public static MotionController LatestInstance { get; private set; }

		public GrabController LeftGrab { get; private set; }
		public GrabController RightGrab { get; private set; }

		private bool _isHolding = false;
		private InertialObject _inertialData;

		public MotionController()
		{
			Options.Load();
			Options.OnOptionsChange += OnOptionsChanged;

			LeftGrab = new GrabController();
			RightGrab = new GrabController();

			// Always instantiate after Left and Right grabs
			//MotionStyle = new AbsoluteMotion();
			MotionStyle = new HandlebarMotion();

			_inertialData = new InertialObject(Options.VelocityFrames);
			LatestInstance = this;
		}

		public void OnOptionsChanged()
		{
			if (_motionStyle != null)
			{
				_motionStyle.InvertAxis = Options.InvertAxis;
			}
		}

		public bool Update(Vector3 position, Quaternion rotation)
		{
			ObjectPosition = position;
			ObjectRotation = rotation;

			HandTracking.Update();

			LeftGrab.Update(HandTracking.LeftHandData, ObjectPosition, ObjectRotation);
			RightGrab.Update(HandTracking.RightHandData, ObjectPosition, ObjectRotation);

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
			bool anyHandHolding = LeftGrab.IsHolding && RightGrab.IsHolding;

			if (anyHandHolding)
			{
				if (!_isHolding)
				{
					_isHolding = true;
					StartMoving();
				}
				float t = Time.time;
				_inertialData.SetPosition(ObjectPosition, t);
				_inertialData.SetRotation(ObjectRotation, t);

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


			ObjectPosition += linearVelocity * deltaTime;
			_inertialData.SetPosition(ObjectPosition, Time.time);

			Vector3 eulerVelocity = _inertialData.AngularVelocityEuler * Options.AngularDrag;

			// Up vector always pointing up
			eulerVelocity.z = 0;
			_inertialData.AngularVelocityEuler = eulerVelocity;

			Quaternion deltaRotation = Quaternion.Euler(eulerVelocity * deltaTime);

			Quaternion orientation = deltaRotation * ObjectRotation;

			if (IsCamera)
			{
				orientation.eulerAngles = MathHelper.ClampEulerRotationXZ(orientation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
			}

			ObjectRotation = orientation;

			return linearVelocity.sqrMagnitude >= Vector3.kEpsilonNormalSqrt ||
			       eulerVelocity.sqrMagnitude >= Vector3.kEpsilonNormalSqrt;
		}

		private void StartMoving()
		{
			_inertialData.Clear();
		}

		private void StopMoving()
		{
			_inertialData.DiscardFrames(Options.DiscardFrames);
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}
	}
}