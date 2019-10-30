using UnityEngine;

namespace Lemonity
{
	public class ScaleMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return true; } }

		private float _initialLogScale;

		private Vector3 _wcInitialPosition;
		private Vector3 _hcCenterInitialPos;
		private Vector3 _ccInitialCamToCenter;

		protected override void StartMotion()
		{
			LeftGesture.Reset();
			RightGesture.Reset();

			_initialLogScale = MathHelper.LinearToLogScale(Scale);

			_wcInitialPosition = Position;
			_hcCenterInitialPos = (LeftGesture.HandCurrentPosition + RightGesture.HandCurrentPosition) * 0.5f;
			_ccInitialCamToCenter = HandTracking.CamToHandOffset(Scale) + Scale * _hcCenterInitialPos;
		}

		protected override void UpdateMotion()
		{
			var initialSep = Vector3.Distance(LeftGesture.HandInitialPosition, RightGesture.HandInitialPosition);
			var finalSep = Vector3.Distance(LeftGesture.HandCurrentPosition, RightGesture.HandCurrentPosition);

			float sep = initialSep - finalSep;
			float sepDelta = 5f * sep * Options.ZoomScale;
			float currentLogScale = Mathf.Clamp(_initialLogScale + sepDelta, 0f, 10f);
			Scale = MathHelper.LogToLinearScale(currentLogScale);

			var hcCenterFinalPos = (LeftGesture.HandCurrentPosition + RightGesture.HandCurrentPosition) * 0.5f;
			var ccFinalCamToCenter = HandTracking.CamToHandOffset(Scale) + Scale * hcCenterFinalPos;
			var deltaVector = _ccInitialCamToCenter - ccFinalCamToCenter;

			Position = _wcInitialPosition + Rotation * deltaVector;
		}

		public override bool InertialMovement()
		{
			return false;
		}

		protected override void UpdateInertialData()
		{
		}
	}
}
