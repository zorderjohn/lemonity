using UnityEngine;

namespace Lemonity
{
	public class OneHandNoPivotMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return false; } }

		float _lastTime = 0f;
		bool _absoluteMovement = false;

		protected override void StartMotion()
		{
			_lastTime = GetTime();
		}

		protected override void UpdateMotion()
		{
			float curTime = GetTime();
			float deltaTime = curTime - _lastTime;
			_lastTime = curTime;

			var grabInfo = GetDominantGrabController(latestHold: true);

			Vector3 deltaMovement = MathHelper.ExponentialScale(grabInfo.HandDeltaPosition, Options.FlyPosScale * deltaTime * 100f, Options.FlyExponential);

			if (!InvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = Rotation * deltaMovement;

			Position = _absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : Position + deltaMovement;


			Quaternion deltaRot = grabInfo.HandDeltaRotation;
			if (!InvertAxis)
			{
				deltaRot = Quaternion.Inverse(deltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(deltaRot);

			eulerDeltaRot.x = ExponentialScale(eulerDeltaRot.x * Options.FlyPitchScale * deltaTime);
			eulerDeltaRot.y = ExponentialScale(eulerDeltaRot.y * Options.FlyYawScale * deltaTime);
			deltaRot = Quaternion.Euler(eulerDeltaRot);

			Quaternion targetRotation = _absoluteMovement ? grabInfo.ObjectInitialRotation * deltaRot : Rotation * deltaRot;

			Rotation = MathHelper.ClampRotationXZ(targetRotation, Options.PitchLimit, Options.PitchMinAngleLimit, Options.PitchMaxAngleLimit, Options.RollLimit);

			// Not camera
			//transform.rotation = deltaRot * initialObjectRot;
		}

		protected float ExponentialScale(float value)
		{
			float sign = Mathf.Sign(value);
			float absValue = Mathf.Abs(value);
			return sign * Mathf.Pow(absValue, Options.FlyExponential);
		}
	}
}
