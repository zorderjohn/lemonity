using UnityEngine;

namespace Leanity
{
	public interface IMotionStyle
	{
		Vector3 Position { get; set; }
		Quaternion Rotation { get; set; }
		float Scale { get; set; }
		GestureController LeftGesture { get; set; }
		GestureController RightGesture { get; set; }
		bool RequiresTwoHands { get; }
		bool InvertAxis { get; set; }

		void Start();
		void Update();
		void Stop();
		bool InertialUpdate();
		void LateFrameUpdate();
		void StopInertia();
		bool HasInertia { get; }

		void OptionsChange();
		void DebugDraw();
	}

	public abstract class MotionStyleBase : IMotionStyle
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public float Scale { get; set; }
		public GestureController LeftGesture { get; set; }
		public GestureController RightGesture { get; set; }
		public virtual bool RequiresTwoHands { get { return false; } }
		public bool InvertAxis { get; set; }
		public virtual void DebugDraw() {; }
		public bool HasInertia
		{
			get
			{
				return _inertialData.IsMoving;
			}
		}

		protected InertialObject _inertialData;
		protected float _lastFrameTime = 0f;


		public MotionStyleBase()
		{
			_inertialData = new InertialObject(Options.VelocityFrames);
		}

		public void Start()
		{
			_inertialData.Clear();
			StartMotion();
		}


		public void Update()
		{
			UpdateMotion();
			UpdateInertialData();
		}

		public void Stop()
		{
			_inertialData.DiscardFrames(Options.DiscardFrames);
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}

		public virtual bool InertialUpdate()
		{
			float t = GetTime();
			_inertialData.DragAngularVelocity(Options.AngularDrag, t);
			_inertialData.DragLinearVelocity(Options.LinearDrag, t);

			_inertialData.Update(t);

			Position = _inertialData.Position;
			Rotation = _inertialData.Rotation;

			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);

			return _inertialData.IsMoving;
		}

		public void StopInertia()
		{
			_inertialData.Clear();
		}

		public virtual void OptionsChange()
		{
			_inertialData.BufferLength = Options.VelocityFrames;
			InvertAxis = Options.InvertAxis;
		}

		protected abstract void StartMotion();

		protected abstract void UpdateMotion();

		protected virtual void UpdateInertialData()
		{
			float t = GetTime();
			_inertialData.SetPosition(Position, t);
			_inertialData.SetRotation(Rotation, t);

			// Debugging
			_inertialData.CalculateAngularVelocity();

			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);
		}

		protected GestureController GetDominantGrabController(bool latestHold = true)
		{
			if (LeftGesture.IsHolding && RightGesture.IsHolding)
			{
				if (LeftGesture.StartTime > RightGesture.StartTime && latestHold)
				{
					return latestHold ? LeftGesture : RightGesture;
				}
				else
				{
					return latestHold ? RightGesture : LeftGesture;
				}
			}
			else if (LeftGesture.IsHolding)
			{
				return LeftGesture;
			}
			else if (RightGesture.IsHolding)
			{
				return RightGesture;
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
