using UnityEngine;

namespace Lemonity
{
	public interface IMotionStyle
	{
		Vector3 Position { get; set; }
		Quaternion Rotation { get; set; }
		float Scale { get; set; }
		IGestureController LeftGesture { get; set; }
		IGestureController RightGesture { get; set; }
		bool RequiresTwoHands { get; }
		bool InvertAxis { get; set; }

		void Start();
		void Update();
		void Stop();
		bool InertialMovement();
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
		public IGestureController LeftGesture { get; set; }
		public IGestureController RightGesture { get; set; }
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

			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);
		}

		public void Stop()
		{
			_inertialData.DiscardFrames(Options.DiscardFrames);
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}

		public abstract bool InertialMovement();

		public bool InertialMovementSimple()
		{
			float t = GetTime();
			_inertialData.DragAngularVelocity(Options.AngularDrag, t);
			_inertialData.DragLinearVelocity(Options.LinearDrag, t);

			if (_inertialData.Update(t))
			{
				Position = _inertialData.Position;
				Rotation = _inertialData.Rotation;
			}

			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);
			GraphDbg.Log("moving", _inertialData.IsMoving ? 1f : 0f);

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

		protected abstract void UpdateInertialData();

		protected void UpdateInertialDataSimple()
		{
			float t = GetTime();
			_inertialData.SetPosition(Position, t);
			_inertialData.SetRotation(Rotation, t);

			// Debugging
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}

		protected IGestureController GetDominantGrabController(bool latestHold = true)
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
	}
}
