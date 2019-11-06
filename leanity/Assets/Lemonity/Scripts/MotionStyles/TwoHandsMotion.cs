using UnityEngine;

namespace Lemonity
{
	public class TwoHandsMotion : MotionStyleBase
	{
		private Vector3 _wcCamPivot;
		private Vector3 _wcPivotToCam;
		private Vector3 _hcCenterInitialPos;
		private Vector3 _hcGestureInitialRotation;
		private Quaternion _wcCamInitialRot;
		private Quaternion _hcPitchDeltaRot;

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
			_hcCenterInitialPos = (LeftGesture.HandInitialPosition + RightGesture.HandInitialPosition) * 0.5f;

			// Store camera initial rotation
			_wcCamInitialRot = grabInfo.ObjectInitialRotation;

			// Calculate camera rotation pivot
			_wcCamPivot = wcCamInitialPos + _wcCamInitialRot * HandTracking.HandToCamCoordinates(_hcCenterInitialPos);

			// Vector from pivot to camera which will be rotated by the gesture
			_wcPivotToCam = wcCamInitialPos - _wcCamPivot;

			_hcGestureInitialRotation = LeftGesture.HandInitialPosition - RightGesture.HandInitialPosition;
			// Projecting to XZ plane to preserve only yaw rotation
			_hcGestureInitialRotation.y = 0;
		}

		protected override void UpdateMotion()
		{
			// Yaw Rotation

			// Gesture rotation
			Vector3 hcGestureCurrentRotation = LeftGesture.HandCurrentPosition - RightGesture.HandCurrentPosition;

			// Projecting to XZ plane to preserve only yaw rotation
			hcGestureCurrentRotation.y = 0;

			var hcYawDeltaRot = Quaternion.FromToRotation(hcGestureCurrentRotation, _hcGestureInitialRotation);

			// Pitch Rotation
			Quaternion hcLeftDeltaRot = LeftGesture.HandDeltaRotation;
			Quaternion hcRightDeltaRot = RightGesture.HandDeltaRotation;

			// Remove yaw and roll rotations to get hands pitch rotation
			hcRightDeltaRot.eulerAngles = new Vector3(hcRightDeltaRot.eulerAngles.x, 0f, 0f);
			hcLeftDeltaRot.eulerAngles = new Vector3(hcLeftDeltaRot.eulerAngles.x, 0f, 0f);

			// Promediate pitch rotations
			_hcPitchDeltaRot = Quaternion.Lerp(hcLeftDeltaRot, hcRightDeltaRot, .5f);

			if (Options.GrabInvertAxis)
			{
				hcYawDeltaRot = Quaternion.Inverse(hcYawDeltaRot);
			}
			else
			{
				_hcPitchDeltaRot = Quaternion.Inverse(_hcPitchDeltaRot);
			}

			// Scale yaw rotation
			Vector3 eulerYawDeltaRot = MathHelper.NormalizedEulerAngles(hcYawDeltaRot);
			eulerYawDeltaRot.Scale(Options.AxisRotScale);
			eulerYawDeltaRot *= Options.RotScale;
			hcYawDeltaRot = Quaternion.Euler(eulerYawDeltaRot);

			// Position calculation

			// Calculate translation as the relative translation of the hands
			Vector3 hcCenterFinalPos = (LeftGesture.HandCurrentPosition + RightGesture.HandCurrentPosition) * 0.5f;
			Vector3 ccDeltaTranslation = (_hcCenterInitialPos - hcCenterFinalPos) * Options.PosScale;

			if (Options.GrabInvertAxis)
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

			// Gesture translation in world coordinates
			Vector3 wcDeltaTranslation = Rotation * ccDeltaTranslation;

			// Effect of rotation around the pivot
			Vector3 wcPivotedTranslation = Rotation * Quaternion.Inverse(_wcCamInitialRot) * _wcPivotToCam;

			Position = _wcCamPivot + wcDeltaTranslation + wcPivotedTranslation;
		}

		protected override void UpdateInertialData()
		{
			// Debugging
			_inertialData.CalculateAngularVelocity();
			_inertialData.CalculateLinearVelocity();
		}

		public override void DebugDraw()
		{
			var wcLeftInitialPos = LeftGesture.ObjectInitialPosition + LeftGesture.ObjectInitialRotation * HandTracking.HandToCamCoordinates(LeftGesture.HandInitialPosition);
			var wcRightInitialPos = LeftGesture.ObjectInitialPosition + LeftGesture.ObjectInitialRotation * HandTracking.HandToCamCoordinates(RightGesture.HandInitialPosition);

			Vector3 wcCamPivotFloor = _wcCamPivot;
			wcCamPivotFloor.y = 0f;
			UnityEditor.Handles.color = Color.red;
			UnityEditor.Handles.DrawLine(_wcCamPivot, wcCamPivotFloor);

			UnityEditor.Handles.color = Color.green;
			UnityEditor.Handles.DrawLine(wcLeftInitialPos, wcRightInitialPos);

			UnityEditor.Handles.color = Color.blue;
			UnityEditor.Handles.DrawLine(_wcCamPivot, _wcCamPivot + _wcPivotToCam);
		}
	}
}
