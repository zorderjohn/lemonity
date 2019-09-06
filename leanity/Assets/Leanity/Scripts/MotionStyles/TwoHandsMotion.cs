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
			Rotation = MathHelper.ClampRotationXZ(targetRotation, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
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

			// Update Inertial data with relative position and rotation
		/*	float t = GetTime();
			_inertialData.SetPosition(ccDeltaTranslation, t);
			_inertialData.SetRotation(hcYawDeltaRot, t);
			*/
			#endregion
		}

		/*public override bool InertialUpdate()
		{
			_inertialData.DragAngularVelocity(Options.AngularDrag);
			_inertialData.DragLinearVelocity(Options.LinearDrag);

			float deltaTime = GetDeltaTime();
			Position += _inertialData.LinearVelocity * deltaTime;

			Vector3 eulerVelocity = _inertialData.AngularVelocityEuler;
			Quaternion deltaRotation = Quaternion.Euler(eulerVelocity * deltaTime);
			Quaternion newOrientation = Rotation * deltaRotation;
			Rotation = MathHelper.ClampRotationXZ(newOrientation, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);


			return _inertialData.IsMoving();
		}*/

		/*protected override void UpdateInertialData()
		{
			// Debugging
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
			GraphDbg.Log("vel", _inertialData.LinearVelocity.magnitude);
			GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1001);
		}*/

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
