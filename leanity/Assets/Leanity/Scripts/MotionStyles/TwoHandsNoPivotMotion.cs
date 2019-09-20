using UnityEngine;

namespace Leanity
{
	public class TwoHandsNoPivotMotion : MotionStyleBase
	{
		protected override void StartMotion() {}

		protected override void UpdateMotion()
		{
			bool absoluteMovement = true;

			var grabInfo = GetDominantGrabController();

			Vector3 leftInitialPos = LeftGesture.HandInitialPosition;
			Vector3 rightInitialPos = RightGesture.HandInitialPosition;
			Vector3 centerInitialPos = Vector3.Lerp(leftInitialPos, rightInitialPos, 0.5f);

			Vector3 centerFinalPos = Vector3.Lerp(LeftGesture.HandCurrentPosition, RightGesture.HandCurrentPosition, 0.5f);

			Vector3 deltaMovement = (centerInitialPos - centerFinalPos) * Options.PosScale;
			if (InvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = Rotation * deltaMovement;

			Position = absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : Position + deltaMovement;


			// Initial Rotation
			Vector3 initialRotation = leftInitialPos - rightInitialPos;
			Vector3 currentRotation = LeftGesture.HandCurrentPosition - RightGesture.HandCurrentPosition;
			var deltaRot = Quaternion.FromToRotation(currentRotation, initialRotation);

			if (InvertAxis)
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
		}
	}
}
