using UnityEngine;

namespace Lemonity
{
	public class TwoHandsNoPivotMotion : MotionStyleBase
	{
		float _lastTime = 0f;
		bool _absoluteMovement = false;

		public override bool RequiresTwoHands { get { return true; } }

		public TwoHandsNoPivotMotion(Runtime runtime) : base(runtime) { }

		protected override void StartMotion()
		{
			_lastTime = GetTime();
		}

		protected override void UpdateMotion()
		{
			float curTime = GetTime();
			float deltaTime = curTime - _lastTime;
			_lastTime = curTime;
			deltaTime = 0.02f;

			var grabInfo = GetDominantGrabController();

			Vector3 leftInitialPos = LeftGesture.HandInitialPosition;
			Vector3 rightInitialPos = RightGesture.HandInitialPosition;
			Vector3 centerInitialPos = Vector3.Lerp(leftInitialPos, rightInitialPos, 0.5f);

			Vector3 centerFinalPos = Vector3.Lerp(LeftGesture.HandCurrentPosition, RightGesture.HandCurrentPosition, 0.5f);

			Vector3 deltaMovement = (centerInitialPos - centerFinalPos);
			deltaMovement = MathHelper.ExponentialScale(deltaMovement, Options.FlyPosScale * deltaTime * 100f, Options.FlyExponential);

			if (Options.FlyInvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = Rotation * deltaMovement;

			Position = _absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : Position + deltaMovement;

			if (Options.FlyHover)
			{
				Position = Hover();
			}

			// Initial Rotation
			Vector3 initialRotation = leftInitialPos - rightInitialPos;
			initialRotation.y = 0f;
			Vector3 currentRotation = LeftGesture.HandCurrentPosition - RightGesture.HandCurrentPosition;
			currentRotation.y = 0f;
			var deltaRot = Quaternion.FromToRotation(currentRotation, initialRotation);


			// Pitch Rotation
			Quaternion hcLeftDeltaRot = LeftGesture.HandDeltaRotation;
			Quaternion hcRightDeltaRot = RightGesture.HandDeltaRotation;

			// Remove yaw and roll rotations to get hands pitch rotation
			hcRightDeltaRot.eulerAngles = new Vector3(-hcRightDeltaRot.eulerAngles.x, 0f, 0f);
			hcLeftDeltaRot.eulerAngles = new Vector3(-hcLeftDeltaRot.eulerAngles.x, 0f, 0f);

			// Promediate pitch rotations
			var hcPitchDeltaRot = Quaternion.Lerp(hcLeftDeltaRot, hcRightDeltaRot, .5f);

			deltaRot = deltaRot * hcPitchDeltaRot;


			if (Options.FlyInvertAxis)
			{
				deltaRot = Quaternion.Inverse(deltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(deltaRot);
			eulerDeltaRot.x = MathHelper.ExponentialScale(eulerDeltaRot.x, Options.FlyPitchScale * deltaTime, Options.FlyExponential);
			eulerDeltaRot.y = MathHelper.ExponentialScale(eulerDeltaRot.y, Options.FlyYawScale * deltaTime, Options.FlyExponential);
			deltaRot = Quaternion.Euler(eulerDeltaRot);

			Quaternion targetRotation = _absoluteMovement ? grabInfo.ObjectInitialRotation * deltaRot : Rotation * deltaRot;

			Rotation = MathHelper.ClampRotationXZ(targetRotation, Options.PitchLimit, Options.PitchMinAngleLimit, Options.PitchMaxAngleLimit, Options.RollLimit);
		}

		protected override void PostInertialMovement()
		{
			if (Options.FlyHover)
			{
				Position = Hover();
			}
		}


	}
}
