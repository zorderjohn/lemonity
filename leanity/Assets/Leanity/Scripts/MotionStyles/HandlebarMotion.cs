using UnityEngine;

namespace Leanity
{
	public class HandlebarMotion : MotionStyleBase
	{
		public override void Update()
		{
			bool absoluteMovement = true;

			GrabController grabInfo = GetDominantGrabController();

			Vector3 leftInitialPos = LeftGrab.HandInitialPosition;
			Vector3 rightInitialPos = RightGrab.HandInitialPosition;
			Vector3 centerInitialPos = Vector3.Lerp(leftInitialPos, rightInitialPos, 0.5f);

			Vector3 centerFinalPos = Vector3.Lerp(LeftGrab.HandCurrentPosition, RightGrab.HandCurrentPosition, 0.5f);

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
			Vector3 currentRotation = LeftGrab.HandCurrentPosition - RightGrab.HandCurrentPosition;
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

			Vector3 clampedEulerRotation = MathHelper.ClampEulerRotationXZ(targetRotation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
			Rotation = Quaternion.Euler(clampedEulerRotation);


		}
	}
}
