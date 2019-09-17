using UnityEngine;

namespace Leanity
{
	public class PinchController : GestureController
	{
		public float StartTime { get; private set; }
		public bool IsHolding { get; private set; }

		public Vector3 HandInitialPosition { get; private set; }
		public Quaternion HandInitialRotation { get; private set; }

		public Vector3 HandCurrentPosition
		{
			get { return _hand.Position; }
		}
		public Quaternion HandCurrentRotation
		{
			get { return _hand.Rotation; }
		}

		public Vector3 ObjectInitialPosition { get; private set; }
		public Quaternion ObjectInitialRotation { get; private set; }

		public Vector3 HandDeltaPosition
		{
			get { return _hand.Position - HandInitialPosition; }
		}

		public Quaternion HandDeltaRotation
		{
			get { return Quaternion.Inverse(HandInitialRotation) * _hand.Rotation; }
		}

		private HandData _hand;
		private Vector3 _latestObjectPosition;
		private Quaternion _latestObjectRotation;

		public PinchController
()
		{
			IsHolding = false;
		}

		public void Reset()
		{
			if (IsHolding)
			{
				StartHolding();
			}
		}

		public void Update(HandData hand, Vector3 objectPosition, Quaternion objectRotation)
		{
			_hand = hand;
			_latestObjectPosition = objectPosition;
			_latestObjectRotation = objectRotation;

			if (IsHolding)
			{
				if (!hand.Detected || hand.PinchDistance >= Options.PinchMaxThreshold)
				{
					IsHolding = false;
				}
			}
			else
			{
				if (hand.Detected && hand.PinchDistance < Options.PinchMinThreshold)
				{
					IsHolding = true;
					StartHolding();
				}
			}
		}

		private void StartHolding()
		{
			StartTime = Time.time;
			HandInitialPosition = _hand.Position;
			HandInitialRotation = _hand.Rotation;
			ObjectInitialPosition = _latestObjectPosition;
			ObjectInitialRotation = _latestObjectRotation;
		}
	}

}
