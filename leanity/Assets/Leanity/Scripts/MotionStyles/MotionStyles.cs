using UnityEngine;

namespace Leanity
{
	public interface IMotionStyle
	{
		Vector3 Position { get; set; }
		Quaternion Rotation { get; set; }
		GrabController LeftGrab { get; set; }
		GrabController RightGrab { get; set; }
		InertialObject InertialData { get; set; }
		bool RequiresTwoHands { get; }
		bool InvertAxis { get; set; }

		void Start();
		void Update();
		void Stop();
		bool InertialUpdate();
		void LateFrameUpdate();

		void OptionsChange();
		void DebugDraw();
	}

	public abstract class MotionStyleBase : IMotionStyle
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public GrabController LeftGrab { get; set; }
		public GrabController RightGrab { get; set; }
		public InertialObject InertialData { get; set; }
		public virtual bool RequiresTwoHands { get { return false; } }
		public bool InvertAxis { get; set; }
		public virtual void DebugDraw() {; }

		protected InertialObject _inertialData;
		protected float _lastFrameTime = 0f;
		protected readonly float TERMINAL_SQR_VELOCITY = 0.001f;

		public MotionStyleBase()
		{
			_inertialData = new InertialObject(Options.VelocityFrames);
		}

		protected abstract void StartMotion();
		public void Start()
		{
			_inertialData.Clear();
			StartMotion();
		}

		protected abstract void UpdateMotion();
		public void Update()
		{
			UpdateMotion();

			float t = GetTime();
			_inertialData.SetPosition(Position, t);
			_inertialData.SetRotation(Rotation, t);

			// Debugging
			_inertialData.CalculateAngularVelocity();

			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);

		}

		public void Stop()
		{
			_inertialData.DiscardFrames(Options.DiscardFrames);
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}

		public virtual bool InertialUpdate()
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
			Quaternion orientation = Rotation * deltaRotation;
			orientation.eulerAngles = MathHelper.ClampEulerRotationXZ(orientation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);

			Rotation = orientation;

			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);

			return linearVelocity.sqrMagnitude + eulerVelocity.sqrMagnitude >= TERMINAL_SQR_VELOCITY;
		}

		public virtual void OptionsChange()
		{
			_inertialData.BufferLength = Options.VelocityFrames;
			InvertAxis = Options.InvertAxis;
		}

		protected GrabController GetDominantGrabController(bool latestHold = true)
		{
			if (LeftGrab.IsHolding && RightGrab.IsHolding)
			{
				if (LeftGrab.StartTime > RightGrab.StartTime && latestHold)
				{
					return latestHold ? LeftGrab : RightGrab;
				}
				else
				{
					return latestHold ? RightGrab : LeftGrab;
				}
			}
			else if (LeftGrab.IsHolding)
			{
				return LeftGrab;
			}
			else if (RightGrab.IsHolding)
			{
				return RightGrab;
			}
			else
			{
				return null;
			}
		}

		protected float GetTime()
		{
			return Time.realtimeSinceStartup;
		}

		protected float GetDeltaTime()
		{
			return Time.realtimeSinceStartup - _lastFrameTime;
		}

		public void LateFrameUpdate()
		{
			_lastFrameTime = GetTime();
		}
	}
}
