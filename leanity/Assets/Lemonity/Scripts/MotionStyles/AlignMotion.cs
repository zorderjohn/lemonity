using UnityEngine;

namespace Lemonity
{
	public class AlignMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return false; } }

		IGestureController _gestureController;
		Vector3 _selectionCenter;
		float _distanceToCenter;

		const float _alignDistance = 0.1f;
		const float _alignMinDistance = 0.01f;

		protected override void StartMotion()
		{
			var newController = GetDominantGrabController(latestHold: true);
			if (newController != null)
			{
				_gestureController = newController;
				_gestureController.Reset();
			}

			_selectionCenter = MathHelper.GetSelectionCenter();
			_distanceToCenter = Vector3.Distance(_gestureController.ObjectInitialPosition, _selectionCenter);
		}

		protected override void UpdateMotion()
		{
			// Detect hand change
			if (GetDominantGrabController(latestHold: true) != _gestureController)
			{
				Start();
			}

			var delta = _gestureController.HandDeltaPosition;

			var deltaOrtho = MathHelper.GetSingleAxis(delta);
			float deltaMagnitude = deltaOrtho.magnitude;


			if (deltaMagnitude > _alignMinDistance)
			{
				float normalizedDistance = Mathf.Clamp01((deltaMagnitude - _alignMinDistance) / _alignDistance);
				Quaternion targetRotation = Quaternion.LookRotation(deltaOrtho);
				Rotation = Quaternion.Lerp(_gestureController.ObjectInitialRotation, targetRotation, normalizedDistance);
				Position = _selectionCenter + Rotation * (Vector3.back * _distanceToCenter);
			}
		}
	}
}