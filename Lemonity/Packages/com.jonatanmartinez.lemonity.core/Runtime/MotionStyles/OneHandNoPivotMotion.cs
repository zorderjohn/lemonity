using UnityEngine;

namespace Lemonity.Core
{
	public class OneHandNoPivotMotion : MotionStyleBase
	{
		private readonly FlyModeOptions _flyModeOptions;
		private readonly CameraOptions _cameraOptions;

		public override bool RequiresTwoHands { get { return false; } }

		float _lastTime = 0f;
		bool _absoluteMovement = false;

		public OneHandNoPivotMotion(Runtime runtime, FlyModeOptions flyModeOptions, CameraOptions cameraOptions, InertiaOptions inertiaOptions) : base(runtime, inertiaOptions, cameraOptions)
		{
			_flyModeOptions = flyModeOptions;
			_cameraOptions = cameraOptions;
		}

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

			Vector3 deltaMovement = MathHelper.ExponentialScale(grabInfo.HandDeltaPosition, _flyModeOptions.PosScale * deltaTime * 100f, _flyModeOptions.ExponentialFactor);

			if (!_flyModeOptions.InvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = Rotation * deltaMovement;

			Position = _absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : Position + deltaMovement;
			if (_flyModeOptions.Hover)
			{
				Position = Hover(_flyModeOptions.HoverDistance);
			}

			Quaternion deltaRot = grabInfo.HandDeltaRotation;
			if (!_flyModeOptions.InvertAxis)
			{
				deltaRot = Quaternion.Inverse(deltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(deltaRot);

			eulerDeltaRot.x = ExponentialScale(eulerDeltaRot.x * _flyModeOptions.PitchScale * deltaTime);
			eulerDeltaRot.y = ExponentialScale(eulerDeltaRot.y * _flyModeOptions.YawScale * deltaTime);
			eulerDeltaRot.z = 0f;
			deltaRot = Quaternion.Euler(eulerDeltaRot);

			Quaternion targetRotation = _absoluteMovement ? grabInfo.ObjectInitialRotation * deltaRot : Rotation * deltaRot;

			Rotation = MathHelper.ClampRotationXZ(targetRotation, _cameraOptions.PitchLimit, _cameraOptions.PitchMinAngleLimit, _cameraOptions.PitchMaxAngleLimit, _cameraOptions.RollLimit);

			// Not camera
			//transform.rotation = deltaRot * initialObjectRot;
		}

		protected float ExponentialScale(float value)
		{
			float sign = Mathf.Sign(value);
			float absValue = Mathf.Abs(value);
			return sign * Mathf.Pow(absValue, _flyModeOptions.ExponentialFactor);
		}

		protected override void PostInertialMovement()
		{
			if (_flyModeOptions.Hover)
			{
				Position = Hover(_flyModeOptions.HoverDistance);
			}
		}
	}
}
