using UnityEngine;

namespace Leanity
{
	public interface IMotionStyle
	{
		void Update();
		Vector3 ObjectPosition { get; set; }
		Quaternion ObjectRotation { get; set; }
		GrabController LeftGrab { get; set; }
		GrabController RightGrab { get; set; }
		InertialObject InertialData { get; set; }
		bool InvertAxis { get; set; }
		void DebugDraw();
	}

	public abstract class MotionStyleBase : IMotionStyle
	{
		public Vector3 ObjectPosition { get; set; }
		public Quaternion ObjectRotation { get; set; }
		public GrabController LeftGrab { get; set; }
		public GrabController RightGrab { get; set; }
		public InertialObject InertialData { get; set; }
		public bool InvertAxis { get; set; }
		public virtual void DebugDraw() {; }

		public abstract void Update();

		protected GrabController GetDominantGrabController(bool newest = true)
		{
			if (LeftGrab.IsHolding && RightGrab.IsHolding)
			{
				if (LeftGrab.StartTime > RightGrab.StartTime && newest)
				{
					return newest ? LeftGrab : RightGrab;
				}
				else
				{
					return newest ? RightGrab : LeftGrab;
				}
			}
			else if (LeftGrab.IsHolding)
			{
				return LeftGrab;
			}
			else if (RightGrab.IsHolding)
			{
				return RightGrab;
			}
			else
			{
				return null;
			}
		}
	}

	public class AbsoluteMotion : MotionStyleBase
	{
		public override void Update()
		{
			bool absoluteMovement = true;
			GrabController grabInfo = GetDominantGrabController();
			Vector3 deltaMovement = grabInfo.DeltaPosition * Options.PosScale;
			if (InvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = ObjectRotation * deltaMovement;

			ObjectPosition = absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : ObjectPosition + deltaMovement;


			Quaternion deltaRot = grabInfo.DeltaRotation;
			if (InvertAxis)
			{
				deltaRot = Quaternion.Inverse(deltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(deltaRot);
			eulerDeltaRot.Scale(Options.AxisRotScale);
			eulerDeltaRot *= Options.RotScale;
			deltaRot = Quaternion.Euler(eulerDeltaRot);

			Quaternion targetRotation = absoluteMovement ? grabInfo.ObjectInitialRotation * deltaRot : ObjectRotation * deltaRot;

			Vector3 clampedEulerRotation = MathHelper.ClampEulerRotationXZ(targetRotation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
			ObjectRotation = Quaternion.Euler(clampedEulerRotation);

			// Not camera
			//transform.rotation = deltaRot * initialObjectRot;

		}
	}

	public class HandlebarMotion : MotionStyleBase
	{
		public override void Update()
		{
			bool absoluteMovement = true;

			GrabController grabInfo = GetDominantGrabController();

			Vector3 leftInitialPos = LeftGrab.HandInitialPosition;
			Vector3 rightInitialPos = RightGrab.HandInitialPosition;
			Vector3 centerInitialPos = Vector3.Lerp(leftInitialPos, rightInitialPos, 0.5f);

			Vector3 centerFinalPos = Vector3.Lerp(LeftGrab.HandCurrentPosition, RightGrab.HandCurrentPosition, 0.5f);

			Vector3 deltaMovement = (centerInitialPos - centerFinalPos) * Options.PosScale;
			if (InvertAxis)
			{
				deltaMovement *= -1f;
			}

			// Only for cameras
			deltaMovement = ObjectRotation * deltaMovement;

			ObjectPosition = absoluteMovement ? grabInfo.ObjectInitialPosition + deltaMovement : ObjectPosition + deltaMovement;


			// Initial Rotation
			Vector3 initialRotation = leftInitialPos - rightInitialPos;
			Vector3 currentRotation = LeftGrab.HandCurrentPosition - RightGrab.HandCurrentPosition;
			var deltaRot = Quaternion.FromToRotation(currentRotation, initialRotation);

			if (InvertAxis)
			{
				deltaRot = Quaternion.Inverse(deltaRot);
			}

			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(deltaRot);
			eulerDeltaRot.Scale(Options.AxisRotScale);
			eulerDeltaRot *= Options.RotScale;
			deltaRot = Quaternion.Euler(eulerDeltaRot);

			Quaternion targetRotation = absoluteMovement ? grabInfo.ObjectInitialRotation * deltaRot : ObjectRotation * deltaRot;

			Vector3 clampedEulerRotation = MathHelper.ClampEulerRotationXZ(targetRotation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
			ObjectRotation = Quaternion.Euler(clampedEulerRotation);


		}
	}

	public class TwoHandsMotion : MotionStyleBase
	{
		Vector3 wcPivotToCam;
		Vector3 wcCamPivot;
		Vector3 wcLeftInitialPos;
		Vector3 wcRightInitialPos;

		public override void Update()
		{
			//hc = Hand Coordinates
			//cc = Camera Coordinates
			//wc = World Coordinates

			GrabController grabInfo = GetDominantGrabController(newest:false);
			Vector3 wcCamInitialPos = grabInfo.ObjectInitialPosition;
			Quaternion wcCamInitialRot = grabInfo.ObjectInitialRotation;


			Vector3 hcLeftInitialPos = LeftGrab.HandInitialPosition;
			Vector3 hcRightInitialPos = RightGrab.HandInitialPosition;
			Vector3 hcLeftFinalPos = LeftGrab.HandCurrentPosition;
			Vector3 hcRightFinalPos = RightGrab.HandCurrentPosition;


			Quaternion ObjectInitialRotation = grabInfo.ObjectInitialRotation;
			Vector3 ObjectInitialPosition = grabInfo.ObjectInitialPosition;
			wcLeftInitialPos = ObjectInitialPosition + ObjectInitialRotation * HandTracking.HandToCamCoordinates(hcLeftInitialPos);
			wcRightInitialPos = ObjectInitialPosition + ObjectInitialRotation * HandTracking.HandToCamCoordinates(hcRightInitialPos);


			Vector3 hcCenterInitialPos = (hcLeftInitialPos + hcRightInitialPos) * 0.5f;
			Vector3 hcCenterFinalPos = (hcLeftFinalPos + hcRightFinalPos) * 0.5f;

			//wcCamPivot Y coordinate must be set to zero
			wcCamPivot =  wcCamInitialPos + wcCamInitialRot * HandTracking.HandToCamCoordinates(hcCenterInitialPos);
			//wcCamPivot.y = wcCamInitialPos.y;

			wcPivotToCam = wcCamInitialPos - wcCamPivot;

			//Debug.DrawRay(wcCamPivot, Vector3.down, Color.red, 1f);

			Vector3 ccDeltaMovement = (hcCenterInitialPos - hcCenterFinalPos) * Options.PosScale;
			if (InvertAxis)
			{
				ccDeltaMovement *= -1f;
			}

			// Initial Rotation
			Vector3 ccInitialRotation = hcLeftInitialPos - hcRightInitialPos;
			Vector3 ccCurrentRotation = hcLeftFinalPos - hcRightFinalPos;

			// Projecting to XZ plane to preserve only yaw rotation
			ccInitialRotation.y = 0;
			ccCurrentRotation.y = 0;

			var ccDeltaRot = Quaternion.FromToRotation(ccCurrentRotation, ccInitialRotation);

			var normEuler = MathHelper.NormalizedEulerAngles(ccDeltaRot);

			#region Pitch Rotation
			// Pitch Rotation

			Quaternion hcLeftDeltaRot = LeftGrab.DeltaRotation;
			Quaternion hcRightDeltaRot = RightGrab.DeltaRotation;

			Vector3 eulerLeftPitchRot = MathHelper.NormalizedEulerAngles(hcLeftDeltaRot);
			eulerLeftPitchRot.y = 0f;
			eulerLeftPitchRot.z = 0f;
			hcLeftDeltaRot.eulerAngles = eulerLeftPitchRot;
			#endregion


			if (InvertAxis)
			{
				ccDeltaRot = Quaternion.Inverse(ccDeltaRot);
				hcLeftDeltaRot = Quaternion.Inverse(hcLeftDeltaRot);
			}


			// Scale rotation
			Vector3 eulerDeltaRot = MathHelper.NormalizedEulerAngles(ccDeltaRot);
			eulerDeltaRot.Scale(Options.AxisRotScale);
			eulerDeltaRot *= Options.RotScale;
			ccDeltaRot = Quaternion.Euler(eulerDeltaRot);

			Quaternion targetRotation = ccDeltaRot * wcCamInitialRot * Quaternion.Inverse(hcLeftDeltaRot);


			Vector3 clampedEulerRotation = MathHelper.ClampEulerRotationXZ(targetRotation.eulerAngles, -Options.PitchLimit, Options.PitchLimit, 0f, 0f);
			ObjectRotation = Quaternion.Euler(clampedEulerRotation);

			ObjectPosition = wcCamPivot + ccDeltaRot * (wcCamInitialRot * Quaternion.Inverse(hcLeftDeltaRot) * Quaternion.Inverse(wcCamInitialRot)) * wcPivotToCam + ObjectRotation * ccDeltaMovement;
		}

		public override void DebugDraw()
		{
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
