using UnityEngine;

namespace Leanity
{
	public class GrabController
	{
		public float StartTime;
		public bool IsHolding;
		public Vector3 HandInitialPosition;
		public Quaternion HandInitialRotation;
		public Vector3 HandCurrentPosition
		{
			get { return _hand.Position; }
		}
		public Quaternion HandCurrentRotation
		{
			get { return _hand.Rotation; }
		}
		public Vector3 ObjectInitialPosition;
		public Quaternion ObjectInitialRotation;

		public Vector3 DeltaPosition
		{
			get { return _hand.Position - HandInitialPosition; }
		}

		public Quaternion DeltaRotation
		{
			get { return Quaternion.Inverse(HandInitialRotation) * _hand.Rotation; }
		}

		private HandData _hand;
		private Vector3 _latestObjectPosition;
		private Quaternion _latestObjectRotation;

		public GrabController()
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
				if (!hand.Detected || hand.GrabValue < Options.GrabThreshold)
				{
					IsHolding = false;
				}
			}
			else
			{
				if (hand.Detected && hand.GrabValue >= Options.GrabThreshold)
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
