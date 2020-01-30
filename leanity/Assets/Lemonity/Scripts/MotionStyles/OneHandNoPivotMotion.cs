using UnityEngine;

namespace Lemonity
{
	public class OneHandNoPivotMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return false; } }

		float _lastTime = 0f;
		bool _absoluteMovement = false;

		public OneHandNoPivotMotion(Runtime runtime) : base(runtime) { }

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

			if (!Options.FlyInvertAxis)
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

			Quaternion deltaRot = grabInfo.HandDeltaRotation;
			if (!Options.FlyInvertAxis)
			{
				deltaRot = Quaternion.Inverse(deltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(deltaRot);

			eulerDeltaRot.x = ExponentialScale(eulerDeltaRot.x * Options.FlyPitchScale * deltaTime);
			eulerDeltaRot.y = ExponentialScale(eulerDeltaRot.y * Options.FlyYawScale * deltaTime);
			eulerDeltaRot.z = 0f;
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

		protected override void PostInertialMovement()
		{
			if (Options.FlyHover)
			{
				Position = Hover();
			}
		}
	}
}
