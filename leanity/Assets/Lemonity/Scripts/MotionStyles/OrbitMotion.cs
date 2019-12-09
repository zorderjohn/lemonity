using UnityEditor;
using UnityEngine;

namespace Lemonity
{
	public class OrbitMotion : MotionStyleBase
	{
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
		public OrbitMotion(Runtime runtime) : base(runtime) { }

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
			float yawDegrees = (deltaPos.x / 0.5f) * 360f * Options.OrbitYawScale;
			float pitchDegrees = (-deltaPos.y / 0.6f) * 360f * Options.OrbitPitchScale;

			Quaternion hcYawDeltaRot = Quaternion.Euler(0f, yawDegrees, 0f);
			_hcPitchDeltaRot = Quaternion.Euler(pitchDegrees, 0f, 0f);

			if (Options.OrbitInvertAxis)
			{
				hcYawDeltaRot = Quaternion.Inverse(hcYawDeltaRot);
				_hcPitchDeltaRot = Quaternion.Inverse(_hcPitchDeltaRot);
			}

			// Position calculation
			float translationScaled;
			if (Options.OrbitExponential)
			{
				float sign = Mathf.Sign(deltaPos.z);
				float absDeltaZ = Mathf.Abs(deltaPos.z);
				translationScaled = sign * Mathf.Pow((absDeltaZ / HandTracking.Workspace.z) * Options.OrbitZoomScale * 10f, 2f);
			}
			else
			{
				translationScaled = (deltaPos.z / HandTracking.Workspace.z) * Options.OrbitZoomScale * _wcPivotToCamLength;
			}

			Vector3 ccDeltaTranslation = new Vector3(0f, 0f, translationScaled);

			if (Options.OrbitInvertAxis)
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
			_inertialData.DragAngularVelocity(Options.AngularDrag, t);
			_inertialData.DragLinearVelocity(Options.LinearDrag, t);


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
			Rotation = MathHelper.ClampRotationXZ(targetRotation, Options.PitchLimit, Options.PitchMinAngleLimit, Options.PitchMaxAngleLimit, Options.RollLimit);

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
