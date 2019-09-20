using UnityEngine;

namespace Leanity
{
	public class OneHandMotionPivoted : MotionStyleBase
	{
		Vector3 wcCamPivot;
		Vector3 wcLeftInitialPos;
		Vector3 wcRightInitialPos;
		Vector3 wcPivotToCam;
		Vector3 hcCenterInitialPos;
		Quaternion wcCamInitialRot;
		Leanity.GestureController _gestureController;

		public override bool RequiresTwoHands { get { return false; } }

		// hc = Hand Coordinates
		// cc = Camera Coordinates
		// wc = World Coordinates

		protected override void StartMotion()
		{
			_gestureController = GetDominantGrabController(latestHold: true);
			Vector3 wcCamInitialPos = _gestureController.ObjectInitialPosition;

			// Middle point between hands
			hcCenterInitialPos = _gestureController.HandInitialPosition;

			// Store camera initial rotation
			wcCamInitialRot = _gestureController.ObjectInitialRotation;

			// Calculate camera rotation pivot
			wcCamPivot = wcCamInitialPos + wcCamInitialRot * HandTracking.HandToCamCoordinates(hcCenterInitialPos);

			// Vector from pivot to camera which will be rotated by the gesture
			wcPivotToCam = wcCamInitialPos - wcCamPivot;
		}

		protected override void UpdateMotion()
		{
			// Detect hand change
			if (GetDominantGrabController(latestHold: true) != _gestureController)
			{
				StartMotion();
			}
			// ROTATION CALCULATION

			// Gesture rotation
			var hcDeltaRot = _gestureController.HandDeltaRotation;

			if (!InvertAxis)
			{
				hcDeltaRot = Quaternion.Inverse(hcDeltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(hcDeltaRot);
			eulerDeltaRot.Scale(Options.AxisRotScale);
			eulerDeltaRot *= Options.RotScale;
			hcDeltaRot = Quaternion.Euler(eulerDeltaRot);

			// POSITION CALCULATION

			// Calculate translation as the relative translation of the hands
			Vector3 ccDeltaTranslation = _gestureController.HandDeltaPosition * Options.PosScale;

			if (!InvertAxis)
			{
				ccDeltaTranslation *= -1f;
			}

			UpdatePose(hcDeltaRot, ccDeltaTranslation);

			// Update Inertial data with relative position and rotation
			float t = GetTime();
			_inertialData.SetPosition(ccDeltaTranslation, t);
			_inertialData.SetRotation(hcDeltaRot, t);
		}

		public override bool InertialUpdate()
		{
			_inertialData.DragAngularVelocity(Options.AngularDrag);
			_inertialData.DragLinearVelocity(Options.LinearDrag);

			_inertialData.Update(GetTime());

			UpdatePose(_inertialData.Rotation, _inertialData.Position);

			return _inertialData.IsMoving();
		}

		private void UpdatePose(Quaternion hcDeltaRot, Vector3 ccDeltaTranslation)
		{
			// Combine pitch and yaw rotations
			Quaternion targetRotation = wcCamInitialRot * hcDeltaRot;

			// Remove any roll rotation
			Rotation = MathHelper.ClampRotationXZ(targetRotation, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);

			// Gesture translation in world coordinates
			Vector3 wcDeltaTranslation = Rotation * ccDeltaTranslation;

			// Effect of rotation around the pivot
			Vector3 wcPivotedTranslation = Rotation * Quaternion.Inverse(wcCamInitialRot) * wcPivotToCam;

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
