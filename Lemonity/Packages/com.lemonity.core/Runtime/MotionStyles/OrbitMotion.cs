using UnityEngine;

namespace Lemonity.Core
{
	public class OrbitMotion : MotionStyleBase
	{
		private readonly OrbitModeOptions _orbitModeOptions;
		private readonly CameraOptions _cameraOptions;

		private IGestureController _gesture;
		private Vector3 _wcCamPivot;
		private Vector3 _wcPivotToCamNormalized;
		private float _wcPivotToCamLength;
		private Quaternion _wcCamInitialRot;
		private Quaternion _hcPitchDeltaRot;

		public override bool RequiresTwoHands { get { return false; } }

		// hc = Hand Coordinates
		// cc = Camera Coordinates
		// wc = World Coordinates
		public OrbitMotion(Runtime runtime, OrbitModeOptions orbitModeOptions, CameraOptions cameraOptions, InertiaOptions inertiaOptions) : base(runtime, inertiaOptions, cameraOptions)
		{
			_orbitModeOptions = orbitModeOptions;
			_cameraOptions = cameraOptions;
		}

		protected override void StartMotion()
		{
	//		_debug = GameObject.Find("Axis").transform;

			_gesture = GetDominantGrabController(latestHold: true);

			Vector3 wcCamInitialPos = _gesture.ObjectInitialPosition;

			// Store camera initial rotation
			_wcCamInitialRot = _gesture.ObjectInitialRotation;

			// Calculate camera rotation pivot
			_wcCamPivot = _motionRuntime.SelectionCenter();

			// Vector from pivot to camera which will be rotated by the gesture
			var wcPivotTocam = wcCamInitialPos - _wcCamPivot;
			_wcPivotToCamLength = wcPivotTocam.magnitude;
			_wcPivotToCamNormalized = _wcPivotToCamLength <= Mathf.Epsilon ? wcPivotTocam :  wcPivotTocam / _wcPivotToCamLength;
		}

		protected override void UpdateMotion()
		{
			// Detect hand change
			if (GetDominantGrabController(latestHold: true) != _gesture)
			{
				Start();
			}

			var deltaPos = _gesture.HandDeltaPosition;
			float yawDegrees = (deltaPos.x / 0.5f) * 360f * _orbitModeOptions.YawScale;
			float pitchDegrees = (-deltaPos.y / 0.6f) * 360f * _orbitModeOptions.PitchScale;

			Quaternion hcYawDeltaRot = Quaternion.Euler(0f, yawDegrees, 0f);
			_hcPitchDeltaRot = Quaternion.Euler(pitchDegrees, 0f, 0f);

			if (_orbitModeOptions.InvertAxis)
			{
				hcYawDeltaRot = Quaternion.Inverse(hcYawDeltaRot);
				_hcPitchDeltaRot = Quaternion.Inverse(_hcPitchDeltaRot);
			}

			// Position calculation
			float translationScaled;
			if (_orbitModeOptions.ExponentialZoom)
			{
				float sign = Mathf.Sign(deltaPos.z);
				float absDeltaZ = Mathf.Abs(deltaPos.z);
				translationScaled = sign * Mathf.Pow((absDeltaZ / HandTracking.Workspace.z) * _orbitModeOptions.ZoomScale * 10f, 2f);
			}
			else
			{
				translationScaled = (deltaPos.z / HandTracking.Workspace.z) * _orbitModeOptions.ZoomScale * _wcPivotToCamLength;
			}

			Vector3 ccDeltaTranslation = new Vector3(0f, 0f, translationScaled);

			if (_orbitModeOptions.InvertAxis)
			{
				ccDeltaTranslation *= -1f;
			}

			UpdatePose(hcYawDeltaRot, ccDeltaTranslation);

			// Update Inertial data with relative position and rotation
			float t = GetTime();
			_inertialData.SetPosition(ccDeltaTranslation, t);
			_inertialData.SetRotation(hcYawDeltaRot, t);
		}

		public override bool InertialMovement()
		{
			float t = GetTime();
			_inertialData.DragAngularVelocity(_inertiaOptions.AngularDrag, t);
			_inertialData.DragLinearVelocity(_inertiaOptions.LinearDrag, t);


			if (_inertialData.Update(t))
			{
				UpdatePose(_inertialData.Rotation, _inertialData.Position);
				return true;
			}
			return false;
		}

		private void UpdatePose(Quaternion hcYawDeltaRot, Vector3 ccDeltaTranslation)
		{
			// Combine pitch and yaw rotations
			Quaternion targetRotation = hcYawDeltaRot * _wcCamInitialRot * _hcPitchDeltaRot;

			// Remove any roll rotation
			Rotation = MathHelper.ClampRotationXZ(targetRotation, _cameraOptions.PitchLimit, _cameraOptions.PitchMinAngleLimit, _cameraOptions.PitchMaxAngleLimit, _cameraOptions.RollLimit);

			// Effect of rotation around the pivot
			var wcPivotToCam = _wcPivotToCamNormalized * Mathf.Max(0f, (_wcPivotToCamLength + ccDeltaTranslation.z));
			Vector3 wcPivotedTranslation = Rotation * Quaternion.Inverse(_wcCamInitialRot) * wcPivotToCam;

			Position = _wcCamPivot + wcPivotedTranslation;
		}

		protected override void UpdateInertialData()
		{
			// Debugging
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}

		public override void DebugDraw()
		{
#if UNITY_EDITOR
			if (_gesture == null)
			{
				return;
			}
			Vector3 wcCamPivotFloor = _wcCamPivot;
			wcCamPivotFloor.y *= -2f;
			Vector3 wcCamPivotCeil = _wcCamPivot;
			wcCamPivotCeil.y *= 2f;
			UnityEditor.Handles.color = Color.red;
			UnityEditor.Handles.DrawLine(_wcCamPivot, wcCamPivotFloor);
#endif
		}



	}
}
