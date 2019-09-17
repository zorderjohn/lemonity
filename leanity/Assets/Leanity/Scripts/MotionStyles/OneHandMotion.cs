using UnityEngine;

namespace Leanity
{
	public class OneHandMotion : MotionStyleBase
	{
		protected override void StartMotion() {}

		protected override void UpdateMotion()
		{
			bool absoluteMovement = true;
			var grabInfo = GetDominantGrabController();
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

			Rotation = MathHelper.ClampRotationXZ(targetRotation, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);

			// Not camera
			//transform.rotation = deltaRot * initialObjectRot;
		}
	}
}
