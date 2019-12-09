using UnityEngine;

namespace Lemonity
{
	public class AlignMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return false; } }

		IGestureController _gestureController;
		Vector3            _selectionCenter;
		float              _distanceToCenter;
		int?               _mainAxis;
		Vector3            _initialPos;
		Quaternion         _initialRot;

		const float _alignDistance = 0.08f;
		const float _alignMinDistance = 0.02f;

		public AlignMotion(Runtime runtime) : base(runtime) { }

		protected override void StartMotion()
		{
			_mainAxis = null;
			var newController = GetDominantGrabController(latestHold: true);
			if (newController != null)
			{
				_gestureController = newController;
				_gestureController.Reset();
			}

			_selectionCenter = _motionRuntime.SelectionCenter();
			_distanceToCenter = Vector3.Distance(_gestureController.ObjectInitialPosition, _selectionCenter);

			// Set default value (the initial one)
			_initialRot = MathHelper.ClampRotationXZ(_gestureController.ObjectInitialRotation, false, 0f, 0f, true);
			_initialPos = _selectionCenter + Rotation * (Vector3.back * _distanceToCenter);
		}

		protected override void UpdateMotion()
		{
			// Detect hand change
			if (GetDominantGrabController(latestHold: true) != _gestureController)
			{
				Start();
			}

			// Set default value (the initial one)
			Rotation = Quaternion.Lerp(Rotation, _initialRot, 0.5f);
			Position = Vector3.Lerp(Position, _initialPos, 0.5f);

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
					Rotation = Quaternion.Lerp(_initialRot, targetRotation, normalizedDistance);
					Position = _selectionCenter + Rotation * (Vector3.back * _distanceToCenter);
				}
			}
		}

		protected override void StopMotion()
		{
			Rotation = MathHelper.ClampRotationXZ(Rotation, false, 0f, 0f, true);
		}
	}
}