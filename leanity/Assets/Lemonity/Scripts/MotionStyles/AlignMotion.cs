using UnityEngine;

namespace Lemonity
{
	public class AlignMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return false; } }

		IGestureController _gestureController;
		Vector3 _selectionCenter;
		float _distanceToCenter;
		int? _mainAxis;

		const float _alignDistance = 0.1f;
		const float _alignMinDistance = 0.02f;

		protected override void StartMotion()
		{
			_mainAxis = null;
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

			// Set default value (the initial one)
			Position = _gestureController.ObjectInitialPosition;
			Rotation = _gestureController.ObjectInitialRotation;

			var delta = _gestureController.HandDeltaPosition;
			float deltaMagnitude = delta.magnitude;

			if (deltaMagnitude > _alignMinDistance && _mainAxis == null)
			{
				_mainAxis = MathHelper.GetMainAxis(delta);
			}

			if (_mainAxis != null)
			{
				var deltaOrtho = MathHelper.GetOrthogonalAxis(delta, _mainAxis.Value);
				float deltaOrthoMagnitude = deltaOrtho.magnitude;

				if (deltaOrthoMagnitude > _alignMinDistance)
				{
					float normalizedDistance = Mathf.Clamp01((deltaOrthoMagnitude - _alignMinDistance) / _alignDistance);
					normalizedDistance = MathHelper.EaseInOutSin(normalizedDistance);
					Quaternion targetRotation = Quaternion.LookRotation(deltaOrtho);
					Rotation = Quaternion.Lerp(_gestureController.ObjectInitialRotation, targetRotation, normalizedDistance);
					Position = _selectionCenter + Rotation * (Vector3.back * _distanceToCenter);
				}
			}
		}
	}
}