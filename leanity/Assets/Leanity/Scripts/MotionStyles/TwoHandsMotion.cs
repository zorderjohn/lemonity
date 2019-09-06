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

		public override bool RequiresTwoHands { get { return true; } }

		// hc = Hand Coordinates
		// cc = Camera Coordinates
		// wc = World Coordinates

		protected override void StartMotion()
		{
			GrabController grabInfo = GetDominantGrabController(latestHold: false);
			Vector3 wcCamInitialPos = grabInfo.ObjectInitialPosition;

			// Middle point between hands
			hcCenterInitialPos = (LeftGrab.HandInitialPosition + RightGrab.HandInitialPosition) * 0.5f;

			// Store camera initial rotation
			wcCamInitialRot = grabInfo.ObjectInitialRotation;

			// Calculate camera rotation pivot
			wcCamPivot = wcCamInitialPos + wcCamInitialRot * HandTracking.HandToCamCoordinates(hcCenterInitialPos);

			// Vector from pivot to camera which will be rotated by the gesture
			wcPivotToCam = wcCamInitialPos - wcCamPivot;

			hcGestureInitialRotation = LeftGrab.HandInitialPosition - RightGrab.HandInitialPosition;
			// Projecting to XZ plane to preserve only yaw rotation
			hcGestureInitialRotation.y = 0;
		}

		protected override void UpdateMotion()
		{

			#region Yaw Rotation
			// Gesture rotation
			Vector3 hcGestureCurrentRotation = LeftGrab.HandCurrentPosition - RightGrab.HandCurrentPosition;

			// Projecting to XZ plane to preserve only yaw rotation
			hcGestureCurrentRotation.y = 0;

			var hcYawDeltaRot = Quaternion.FromToRotation(hcGestureCurrentRotation, hcGestureInitialRotation);
			#endregion

			#region Pitch Rotation
			Quaternion hcLeftDeltaRot = LeftGrab.DeltaRotation;
			Quaternion hcRightDeltaRot = RightGrab.DeltaRotation;

			// Remove yaw and roll rotations to get hands pitch rotation
			hcRightDeltaRot.eulerAngles = new Vector3(hcRightDeltaRot.eulerAngles.x, 0f, 0f);
			hcLeftDeltaRot.eulerAngles = new Vector3(hcLeftDeltaRot.eulerAngles.x, 0f, 0f);

			// Promediate pitch rotations
			Quaternion hcPitchDeltaRot = Quaternion.Lerp(hcLeftDeltaRot, hcRightDeltaRot, .5f);
			#endregion

			#region Rotation calculation
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

			// Combine pitch and yaw rotations
			Quaternion targetRotation = hcYawDeltaRot * wcCamInitialRot * hcPitchDeltaRot;

			// Remove any roll rotation
			Vector3 clampedEulerRotation = MathHelper.ClampEulerRotationXZ(targetRotation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
			Rotation = Quaternion.Euler(clampedEulerRotation);
			#endregion

			#region Position calculation

			// Calculate translation as the relative translation of the hands
			Vector3 hcCenterFinalPos = (LeftGrab.HandCurrentPosition + RightGrab.HandCurrentPosition) * 0.5f;
			Vector3 ccDeltaTranslation = (hcCenterInitialPos - hcCenterFinalPos) * Options.PosScale;

			if (InvertAxis)
			{
				ccDeltaTranslation *= -1f;
			}

			// Gesture translation in world coordinates
			Vector3 wcDeltaTranslation = Rotation * ccDeltaTranslation;

			// Effect of rotation around the pivot
			Vector3 wcPivotedTranslation = hcYawDeltaRot * wcCamInitialRot * hcPitchDeltaRot * Quaternion.Inverse(wcCamInitialRot) * wcPivotToCam;

			Position = wcCamPivot + wcDeltaTranslation + wcPivotedTranslation;

			#endregion
		}


		public override void DebugDraw()
		{
			wcLeftInitialPos = LeftGrab.ObjectInitialPosition + LeftGrab.ObjectInitialRotation * HandTracking.HandToCamCoordinates(LeftGrab.HandInitialPosition);
			wcRightInitialPos = LeftGrab.ObjectInitialPosition + LeftGrab.ObjectInitialRotation * HandTracking.HandToCamCoordinates(RightGrab.HandInitialPosition);

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
