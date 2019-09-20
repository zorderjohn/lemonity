using UnityEngine;

namespace Leanity
{
	public class TwoHandsMotion : MotionStyleBase
	{
		Vector3 wcCamPivot;
		Vector3 wcLeftInitialPos;
		Vector3 wcRightInitialPos;
		Vector3 wcPivotToCam;
		Vector3 hcCenterInitialPos;
		Vector3 hcGestureInitialRotation;
		Quaternion wcCamInitialRot;
		Quaternion hcPitchDeltaRot;

		public override bool RequiresTwoHands { get { return true; } }

		// hc = Hand Coordinates
		// cc = Camera Coordinates
		// wc = World Coordinates

		protected override void StartMotion()
		{
			var grabInfo = GetDominantGrabController(latestHold: false);
			LeftGesture.Reset();
			RightGesture.Reset();

			Vector3 wcCamInitialPos = grabInfo.ObjectInitialPosition;

			// Middle point between hands
			hcCenterInitialPos = (LeftGesture.HandInitialPosition + RightGesture.HandInitialPosition) * 0.5f;

			// Store camera initial rotation
			wcCamInitialRot = grabInfo.ObjectInitialRotation;

			// Calculate camera rotation pivot
			wcCamPivot = wcCamInitialPos + wcCamInitialRot * HandTracking.HandToCamCoordinates(hcCenterInitialPos);

			// Vector from pivot to camera which will be rotated by the gesture
			wcPivotToCam = wcCamInitialPos - wcCamPivot;

			hcGestureInitialRotation = LeftGesture.HandInitialPosition - RightGesture.HandInitialPosition;
			// Projecting to XZ plane to preserve only yaw rotation
			hcGestureInitialRotation.y = 0;
		}

		protected override void UpdateMotion()
		{
			// Yaw Rotation

			// Gesture rotation
			Vector3 hcGestureCurrentRotation = LeftGesture.HandCurrentPosition - RightGesture.HandCurrentPosition;

			// Projecting to XZ plane to preserve only yaw rotation
			hcGestureCurrentRotation.y = 0;

			var hcYawDeltaRot = Quaternion.FromToRotation(hcGestureCurrentRotation, hcGestureInitialRotation);

			// Pitch Rotation
			Quaternion hcLeftDeltaRot = LeftGesture.HandDeltaRotation;
			Quaternion hcRightDeltaRot = RightGesture.HandDeltaRotation;

			// Remove yaw and roll rotations to get hands pitch rotation
			hcRightDeltaRot.eulerAngles = new Vector3(hcRightDeltaRot.eulerAngles.x, 0f, 0f);
			hcLeftDeltaRot.eulerAngles = new Vector3(hcLeftDeltaRot.eulerAngles.x, 0f, 0f);

			// Promediate pitch rotations
			hcPitchDeltaRot = Quaternion.Lerp(hcLeftDeltaRot, hcRightDeltaRot, .5f);

			if (InvertAxis)
			{
				hcYawDeltaRot = Quaternion.Inverse(hcYawDeltaRot);
			}
			else
			{
				hcPitchDeltaRot = Quaternion.Inverse(hcPitchDeltaRot);
			}

			// Scale yaw rotation
			Vector3 eulerYawDeltaRot = MathHelper.NormalizedEulerAngles(hcYawDeltaRot);
			eulerYawDeltaRot.Scale(Options.AxisRotScale);
			eulerYawDeltaRot *= Options.RotScale;
			hcYawDeltaRot = Quaternion.Euler(eulerYawDeltaRot);

			// Position calculation

			// Calculate translation as the relative translation of the hands
			Vector3 hcCenterFinalPos = (LeftGesture.HandCurrentPosition + RightGesture.HandCurrentPosition) * 0.5f;
			Vector3 ccDeltaTranslation = (hcCenterInitialPos - hcCenterFinalPos) * Options.PosScale;

			if (InvertAxis)
			{
				ccDeltaTranslation *= -1f;
			}

			UpdatePose(hcYawDeltaRot, ccDeltaTranslation);

			// Update Inertial data with relative position and rotation
			float t = GetTime();
			_inertialData.SetPosition(ccDeltaTranslation, t);
			_inertialData.SetRotation(hcYawDeltaRot, t);

		}

		public override bool InertialUpdate()
		{
			_inertialData.DragAngularVelocity(Options.AngularDrag);
			_inertialData.DragLinearVelocity(Options.LinearDrag);

			_inertialData.Update(GetTime());

			UpdatePose(_inertialData.Rotation, _inertialData.Position);

			return _inertialData.IsMoving();
		}

		private void UpdatePose(Quaternion hcYawDeltaRot, Vector3 ccDeltaTranslation)
		{
			// Combine pitch and yaw rotations
			Quaternion targetRotation = hcYawDeltaRot * wcCamInitialRot * hcPitchDeltaRot;

			// Remove any roll rotation
			Rotation = MathHelper.ClampRotationXZ(targetRotation, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);

			// Gesture translation in world coordinates
			Vector3 wcDeltaTranslation = Rotation * ccDeltaTranslation;

			// Effect of rotation around the pivot
			Vector3 wcPivotedTranslation = hcYawDeltaRot * wcCamInitialRot * hcPitchDeltaRot * Quaternion.Inverse(wcCamInitialRot) * wcPivotToCam;

			Position = wcCamPivot + wcDeltaTranslation + wcPivotedTranslation;
		}

		protected override void UpdateInertialData()
		{
			// Debugging
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);
		}

		public override void DebugDraw()
		{
			wcLeftInitialPos = LeftGesture.ObjectInitialPosition + LeftGesture.ObjectInitialRotation * HandTracking.HandToCamCoordinates(LeftGesture.HandInitialPosition);
			wcRightInitialPos = LeftGesture.ObjectInitialPosition + LeftGesture.ObjectInitialRotation * HandTracking.HandToCamCoordinates(RightGesture.HandInitialPosition);

			Vector3 wcCamPivotFloor = wcCamPivot;
			wcCamPivotFloor.y = 0f;
			UnityEditor.Handles.color = Color.red;
			UnityEditor.Handles.DrawLine(wcCamPivot, wcCamPivotFloor);

			UnityEditor.Handles.color = Color.green;
			UnityEditor.Handles.DrawLine(wcLeftInitialPos, wcRightInitialPos);

			UnityEditor.Handles.color = Color.blue;
			UnityEditor.Handles.DrawLine(wcCamPivot, wcCamPivot + wcPivotToCam);
		}
	}
}
