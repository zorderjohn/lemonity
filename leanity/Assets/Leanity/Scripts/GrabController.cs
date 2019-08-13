using UnityEngine;

namespace Leanity
{
	public class GrabController
	{
		public float StartTime;
		public bool IsHolding;
		public Vector3 HandInitialPosition;
		public Quaternion HandInitialRotation;
		public Vector3 ObjectInitialPosition;
		public Quaternion ObjectInitialRotation;

		private HandData _hand;

		public GrabController()
		{
			IsHolding = false;
		}

		public void Update(HandData hand, Vector3 objectPosition, Quaternion objectRotation)
		{
			_hand = hand;
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
					StartTime = Time.time;
					HandInitialPosition = hand.Position;
					HandInitialRotation = hand.Rotation;
					ObjectInitialPosition = objectPosition;
					ObjectInitialRotation = objectRotation;
				}
			}
		}

		public Vector3 DeltaPosition
		{
			get { return _hand.Position - HandInitialPosition; }
		}

		public Quaternion DeltaRotation
		{
			get { return Quaternion.Inverse(HandInitialRotation) * _hand.Rotation; }
		}
	}
}
