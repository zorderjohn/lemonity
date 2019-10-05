using UnityEngine;

namespace Leanity
{
	public class OneHandNoPivotMotion : MotionStyleBase
	{
		protected override void StartMotion() {}

		protected override void UpdateMotion()
		{
			bool absoluteMovement = true;
			var grabInfo = GetDominantGrabController(latestHold: true);
			Vector3 deltaMovement = grabInfo.HandDeltaPosition * Options.PosScale;
			if (!InvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = Rotation * deltaMovement;

			Position = absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : Position + deltaMovement;


			Quaternion deltaRot = grabInfo.HandDeltaRotation;
			if (!InvertAxis)
			{
				deltaRot = Quaternion.Inverse(deltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(deltaRot);
			eulerDeltaRot.Scale(Options.AxisRotScale);
			eulerDeltaRot *= Options.RotScale;
			deltaRot = Quaternion.Euler(eulerDeltaRot);

			Quaternion targetRotation = absoluteMovement ? grabInfo.ObjectInitialRotation * deltaRot : Rotation * deltaRot;

			Rotation = MathHelper.ClampRotationXZ(targetRotation, Options.PitchLimit, Options.PitchLimitAngle, Options.RollLimit);

			// Not camera
			//transform.rotation = deltaRot * initialObjectRot;
		}

		protected override void UpdateInertialData()
		{
			UpdateInertialDataSimple();
		}

		public override bool InertialMovement()
		{
			return InertialMovementSimple();
		}
	}
}
