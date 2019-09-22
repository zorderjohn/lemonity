using UnityEngine;

namespace Leanity
{
	public class OneHandMotion : MotionStyleBase
	{
		Vector3 _wcCamPivot;
		Vector3 _wcPivotToCam;
		Vector3 _hcCenterInitialPos;
		Quaternion _wcCamInitialRot;
		Quaternion _hcPitchDeltaRot;
		Leanity.GestureController _gestureController;

		public override bool RequiresTwoHands { get { return false; } }

		// hc = Hand Coordinates
		// cc = Camera Coordinates
		// wc = World Coordinates

		protected override void StartMotion()
		{
			_gestureController = GetDominantGrabController(latestHold: true);
			_gestureController.Reset();

			Vector3 wcCamInitialPos = _gestureController.ObjectInitialPosition;

			// Middle point between hands
			_hcCenterInitialPos = _gestureController.HandInitialPosition;

			// Store camera initial rotation
			_wcCamInitialRot = _gestureController.ObjectInitialRotation;

			// Calculate camera rotation pivot
			_wcCamPivot = wcCamInitialPos + _wcCamInitialRot * HandTracking.HandToCamCoordinates(_hcCenterInitialPos);

			// Vector from pivot to camera which will be rotated by the gesture
			_wcPivotToCam = wcCamInitialPos - _wcCamPivot;
		}

		protected override void UpdateMotion()
		{
			// Detect hand change
			if (GetDominantGrabController(latestHold: true) != _gestureController)
			{
				Start();
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

			var hcYawDeltaRot = Quaternion.Euler(new Vector3(0f, eulerDeltaRot.y, 0f));
			_hcPitchDeltaRot = Quaternion.Euler(new Vector3(eulerDeltaRot.x, 0f, 0f));

			// POSITION CALCULATION

			// Calculate translation as the relative translation of the hands
			Vector3 ccDeltaTranslation = _gestureController.HandDeltaPosition * Options.PosScale;

			if (!InvertAxis)
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
			Rotation = MathHelper.ClampRotationXZ(targetRotation, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);

			// Gesture translation in world coordinates
			Vector3 wcDeltaTranslation = Rotation * ccDeltaTranslation;

			// Effect of rotation around the pivot
			Vector3 wcPivotedTranslation = hcYawDeltaRot * _wcCamInitialRot * _hcPitchDeltaRot * Quaternion.Inverse(_wcCamInitialRot) * _wcPivotToCam;

			Position = _wcCamPivot + wcDeltaTranslation + wcPivotedTranslation;
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
		}
	}
}
