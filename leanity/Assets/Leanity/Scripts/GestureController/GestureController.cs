using UnityEngine;

namespace Leanity
{
	public interface IGestureController
	{
		float StartTime { get; }
		bool IsHolding { get; }

		Vector3 HandInitialPosition { get; }
		Quaternion HandInitialRotation { get; }

		Vector3 HandCurrentPosition { get; }
		Quaternion HandCurrentRotation { get; }

		Vector3 HandDeltaPosition { get; }
		Quaternion HandDeltaRotation { get; }

		Vector3 ObjectInitialPosition { get; }
		Quaternion ObjectInitialRotation { get; }


		void Update(HandData hand, Vector3 objectPosition, Quaternion objectRotation);
		void Reset();
	}

	public abstract class GestureControllerBase : IGestureController
	{
		public float StartTime { get; protected set; }
		public bool IsHolding { get; protected set; } = false;

		public Vector3 HandInitialPosition { get; protected set; }
		public Quaternion HandInitialRotation { get; protected set; }

		public Vector3 HandCurrentPosition
		{
			get { return _hand.Position; }
		}
		public Quaternion HandCurrentRotation
		{
			get { return _hand.Rotation; }
		}

		public Vector3 HandDeltaPosition
		{
			get { return _hand.Position - HandInitialPosition; }
		}

		public Quaternion HandDeltaRotation
		{
			get { return _hand.Rotation * Quaternion.Inverse(HandInitialRotation); }
		}

		public Vector3 ObjectInitialPosition { get; protected set; }

		public Quaternion ObjectInitialRotation { get; protected set; }

		protected HandData _hand;
		protected Vector3 _latestObjectPosition;
		protected Quaternion _latestObjectRotation;


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
			bool newHoldingUpdate = HoldingTest();

			if (IsHolding)
			{
				if (!hand.Detected || !newHoldingUpdate)
				{
					IsHolding = false;
				}
			}
			else
			{
				if (hand.Detected && newHoldingUpdate)
				{
					if (HeuristicCondition())
					{
						IsHolding = true;
						StartHolding();
					} else
					{
						Debug.LogWarning("Heuristic activated");
					}

				}
			}
		}

		protected void StartHolding()
		{
			StartTime = Time.time;
			HandInitialPosition = _hand.Position;
			HandInitialRotation = _hand.Rotation;
			ObjectInitialPosition = _latestObjectPosition;
			ObjectInitialRotation = _latestObjectRotation;
		}

		protected abstract bool HoldingTest();

		protected bool HeuristicCondition()
		{
			if (!Options.HeuristicEnabled)
			{
				return true;
			}

			Vector3 handToCenter = -_hand.Position;
			float distanceToCenter = handToCenter.magnitude;
			Vector3 handVelocity = _hand.LinearVelocity;
			float handSpeed = handVelocity.magnitude;
			float dotProd = Vector3.Dot(handToCenter, handVelocity);

			return (distanceToCenter < Options.HeuristicRadius) || (handSpeed > 0.001f && dotProd > 0f);
		}
	}
}
