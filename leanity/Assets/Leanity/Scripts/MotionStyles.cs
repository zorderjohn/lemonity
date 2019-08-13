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
		bool InvertAxis { get; set; }
	}

	public abstract class MotionStyleBase : IMotionStyle
	{
		public Vector3 ObjectPosition { get; set; }
		public Quaternion ObjectRotation { get; set; }
		public GrabController LeftGrab { get; set; }
		public GrabController RightGrab { get; set; }
		public InertialObject InertialData { get; set; }
		public bool InvertAxis { get; set; }

		public abstract void Update();

		protected GrabController GetDominantGrabController()
		{
			if (LeftGrab.IsHolding && RightGrab.IsHolding)
			{
				if (LeftGrab.StartTime > RightGrab.StartTime)
				{
					return LeftGrab;
				}
				else
				{
					return RightGrab;
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

	public class AbsoluteMotion : MotionStyleBase
	{
		public override void Update()
		{
			bool absoluteMovement = true;
			GrabController grabInfo = GetDominantGrabController();
			Vector3 deltaMovement = grabInfo.DeltaPosition * Options.PosScale;
			if (InvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = ObjectRotation * deltaMovement;

			ObjectPosition = absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : ObjectPosition + deltaMovement;


			Quaternion deltaRot = grabInfo.DeltaRotation;
			if (InvertAxis)
			{
				deltaRot = Quaternion.Inverse(deltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(deltaRot);
			eulerDeltaRot.Scale(Options.AxisRotScale);
			eulerDeltaRot *= Options.RotScale;
			deltaRot = Quaternion.Euler(eulerDeltaRot);

			Quaternion targetRotation = absoluteMovement ? grabInfo.ObjectInitialRotation * deltaRot : ObjectRotation * deltaRot;

			Vector3 clampedEulerRotation = MathHelper.ClampEulerRotationXZ(targetRotation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
			ObjectRotation = Quaternion.Euler(clampedEulerRotation);

			// Not camera
			//transform.rotation = deltaRot * initialObjectRot;

		}
	}
}
