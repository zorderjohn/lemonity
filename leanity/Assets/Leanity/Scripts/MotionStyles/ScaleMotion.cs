using UnityEngine;

namespace Leanity
{
	public class ScaleMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return true; } }

		private float _initialLogScale;

		private Vector3 _initialRelativeVector;
		private Vector3 _wcInitialPosition;

		protected override void StartMotion()
		{
			LeftGesture.Reset();
			RightGesture.Reset();

			_initialLogScale = MathHelper.LinearToLogScale(Scale);

			_wcInitialPosition = Position;
			_initialRelativeVector = Rotation * HandTracking.CamToHandOffset(Scale);
		}

		protected override void UpdateMotion()
		{
			var initialSep = Vector3.Distance(LeftGesture.HandInitialPosition, RightGesture.HandInitialPosition);
			var finalSep = Vector3.Distance(LeftGesture.HandCurrentPosition, RightGesture.HandCurrentPosition);

			float sep = -finalSep + initialSep;
			float sepDelta = 5f * sep * Options.ZoomScale;
			float currentLogScale = Mathf.Clamp(_initialLogScale + sepDelta, 0f, 10f);
			Scale = MathHelper.LogToLinearScale(currentLogScale);

			var finalRelativeVector = Rotation * HandTracking.CamToHandOffset(Scale);
			var deltaVector = -finalRelativeVector + _initialRelativeVector;

			Position = _wcInitialPosition + deltaVector;
		}
	}
}
