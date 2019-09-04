using UnityEngine;

namespace Leanity
{
	public class OneHandMotion : MotionStyleBase
	{
		public override void Update()
		{
			bool absoluteMovement = true;
			GrabController grabInfo = GetDominantGrabController();
			Vector3 deltaMovement = grabInfo.DeltaPosition * Options.PosScale;
			if (!InvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = ObjectRotation * deltaMovement;

			ObjectPosition = absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : ObjectPosition + deltaMovement;


			Quaternion deltaRot = grabInfo.DeltaRotation;
			if (!InvertAxis)
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
