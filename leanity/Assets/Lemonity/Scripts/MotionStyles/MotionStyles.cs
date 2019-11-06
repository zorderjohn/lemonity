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
		public virtual void DebugDraw() {; }
		public bool HasInertia
		{
			get
			{
				return _inertialData.IsMoving;
			}
		}

		protected InertialObject _inertialData;
		protected const float _hoverMaxStep = 100f;
		protected const float _hoverSmooth = 0.95f;

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
			StopMotion();
			_inertialData.DiscardFrames(Options.DiscardFrames);
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}

		public virtual bool InertialMovement()
		{
			float t = GetTime();
			_inertialData.DragAngularVelocity(Options.AngularDrag, t);
			_inertialData.DragLinearVelocity(Options.LinearDrag, t);

			if (_inertialData.Update(t))
			{
				Position = _inertialData.Position;
				Rotation = _inertialData.Rotation;
			}

			return _inertialData.IsMoving;
		}

		public void StopInertia()
		{
			_inertialData.Clear();
		}

		public virtual void OptionsChange()
		{
			_inertialData.BufferLength = Options.VelocityFrames;
		}

		protected abstract void StartMotion();

		protected abstract void UpdateMotion();

		protected virtual void StopMotion()	{}

		protected virtual void UpdateInertialData()
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

		protected Vector3 Hover()
		{
			Vector3 pos = Position;

			if (Physics.Raycast(Position + Vector3.up, Vector3.down, out var hit))
			{
				pos = hit.point + Vector3.up * Options.FlyHoverDistance;
			}
			else if (Physics.Raycast(Position + _hoverMaxStep * Vector3.up, Vector3.down, out var hit2))
			{
				pos = hit2.point + Vector3.up * Options.FlyHoverDistance;
			}

			return Position * _hoverSmooth + pos * (1f - _hoverSmooth);
		}
	}
}
