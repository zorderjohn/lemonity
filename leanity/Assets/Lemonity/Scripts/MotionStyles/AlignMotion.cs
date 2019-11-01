using UnityEngine;

namespace Lemonity
{
	public class AlignMotion : MotionStyleBase
	{
		IGestureController _gestureController;

		public override bool RequiresTwoHands { get { return false; } }

		protected override void StartMotion()
		{
			var newController = GetDominantGrabController(latestHold: true);
			if (newController != null)
			{
				_gestureController = newController;
				_gestureController.Reset();
			}
		}

		protected override void UpdateMotion()
		{
			// Detect hand change
			if (GetDominantGrabController(latestHold: true) != _gestureController)
			{
				Start();
			}

			var delta = _gestureController.HandDeltaPosition;
			float deltaMagnitude = delta.magnitude;

			if (deltaMagnitude > 0.01f)
			{
				Rotation = Quaternion.LookRotation(delta);
			}
		}
	}
}