using UnityEngine;

namespace Leanity
{
	public class TwoHandsMotion : MotionStyleBase
	{
		Vector3 wcCamPivot;
		Vector3 wcLeftInitialPos;
		Vector3 wcRightInitialPos;
		Vector3 wcPivotToCam;
		public override bool RequiresTwoHands { get { return true; } }

		public override void Update()
		{
			// hc = Hand Coordinates
			// cc = Camera Coordinates
			// wc = World Coordinates

			GrabController grabInfo = GetDominantGrabController(latestHold: false);
			Vector3 wcCamInitialPos = grabInfo.ObjectInitialPosition;
			Quaternion wcCamInitialRot = grabInfo.ObjectInitialRotation;

			Vector3 hcLeftInitialPos = LeftGrab.HandInitialPosition;
			Vector3 hcRightInitialPos = RightGrab.HandInitialPosition;
			Vector3 hcLeftFinalPos = LeftGrab.HandCurrentPosition;
			Vector3 hcRightFinalPos = RightGrab.HandCurrentPosition;

			Vector3 hcCenterInitialPos = (hcLeftInitialPos + hcRightInitialPos) * 0.5f;
			Vector3 hcCenterFinalPos = (hcLeftFinalPos + hcRightFinalPos) * 0.5f;


			#region Yaw Rotation
			// Gesture rotation
			Vector3 hcGestureInitialRotation = hcLeftInitialPos - hcRightInitialPos;
			Vector3 hcGestureCurrentRotation = hcLeftFinalPos - hcRightFinalPos;

			// Projecting to XZ plane to preserve only yaw rotation
			hcGestureInitialRotation.y = 0;
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
			// Calculate camera rotation pivot
			wcCamPivot = wcCamInitialPos + wcCamInitialRot * HandTracking.HandToCamCoordinates(hcCenterInitialPos);

			// Vector from pivot to camera which will be rotated by the gesture
			wcPivotToCam = wcCamInitialPos - wcCamPivot;

			// Calculate translation as the relative translation of the hands
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
