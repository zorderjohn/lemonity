using UnityEngine;

namespace Leanity
{
	public interface IMotionStyle
	{
		void Update();
		Vector3 ObjectPosition { get; set; }
		Quaternion ObjectRotation { get; set; }
		GrabController LeftGrab { get; set; }
		GrabController RightGrab { get; set; }
		InertialObject InertialData { get; set; }
		bool RequiresTwoHands { get; }
		bool InvertAxis { get; set; }
		void DebugDraw();
	}

	public abstract class MotionStyleBase : IMotionStyle
	{
		public Vector3 ObjectPosition { get; set; }
		public Quaternion ObjectRotation { get; set; }
		public GrabController LeftGrab { get; set; }
		public GrabController RightGrab { get; set; }
		public InertialObject InertialData { get; set; }
		public virtual bool RequiresTwoHands { get { return false; } }
		public bool InvertAxis { get; set; }
		public virtual void DebugDraw() {; }

		public abstract void Update();

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
	}
}
