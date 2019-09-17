using UnityEngine;

namespace Leanity
{
	public class ScaleMotion : MotionStyleBase
	{
		float _initialScale;

		protected override void StartMotion()
		{
			_initialScale = Scale;
		}

		protected override void UpdateMotion()
		{
			var initialSep = Vector3.Distance(LeftGesture.HandInitialPosition, RightGesture.HandInitialPosition);
			var finalSep = Vector3.Distance(LeftGesture.HandCurrentPosition, RightGesture.HandCurrentPosition);

			float sep = finalSep - initialSep;
			float sepDelta = 5f * sep * Options.PinchScale;
			Scale = Mathf.Clamp(_initialScale + sepDelta, 0f, 10f);
		}
	}
}
