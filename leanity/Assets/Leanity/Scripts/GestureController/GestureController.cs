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

		protected enum HeuristicState { AllowAll, OnlyRotation, DenyAll };
		protected HeuristicState _heuristic = HeuristicState.AllowAll;


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
			bool holdingTest = HoldingTest();
			bool releaseTest = ReleaseTest();

			if (IsHolding)
			{
				if (!hand.Detected || releaseTest)
				{
					IsHolding = false;
				}
				else
				{
					Holding();
				}
			}
			else
			{
				if (hand.Detected && holdingTest)
				{
					IsHolding = true;
					StartHolding();
				}
			}
		}

		protected void StartHolding()
		{
			_heuristic = HeuristicCondition();
			SetInitialRotation();
			SetInitialPosition();
		}

		protected void Holding()
		{
			if (_heuristic == HeuristicState.AllowAll)
			{
				return;
			}
			_heuristic = HeuristicCondition();
			switch (_heuristic)
			{
				case HeuristicState.AllowAll:
					break;
				case HeuristicState.OnlyRotation:
					SetInitialPosition();
					break;
				case HeuristicState.DenyAll:
					SetInitialPosition();
					SetInitialRotation();
					break;
			}
		}


		protected void SetInitialPosition()
		{
			StartTime = Time.time;
			ObjectInitialPosition = _latestObjectPosition;
			ObjectInitialRotation = _latestObjectRotation;
		}

		protected void SetInitialRotation()
		{
			StartTime = Time.time;
			HandInitialPosition = _hand.Position;
			HandInitialRotation = _hand.Rotation;
		}

		protected abstract bool HoldingTest();
		protected abstract bool ReleaseTest();

		protected virtual HeuristicState HeuristicCondition()
		{
			if (!Options.HeuristicEnabled)
			{
				return HeuristicState.AllowAll;
			}

			Vector3 handToCenter = -_hand.Position;
			float distanceToCenter = handToCenter.magnitude;
			Vector3 handVelocity = _hand.LinearVelocity;
			float handSpeed = handVelocity.magnitude;
			float dotProd = Vector3.Dot(handToCenter.normalized, handVelocity.normalized);

			Vector3 handAngularVelocity = _hand.AngularVelocity;
			float handAngularSpeed = handAngularVelocity.magnitude;

			bool isNear = distanceToCenter < Options.HeuristicRadius;
			bool isMoving = handSpeed > 0.1f;
			bool isApproaching = dotProd > 0f;
			bool isOk = isNear || (isMoving && isApproaching);
			if (isOk)
			{
				Debug.Log(
					(isOk ? "<<<OK>>>  " : "") +
					$"Distance = {distanceToCenter}({isNear}) " +
					$"Linear Speed = {handSpeed}({isMoving}) " +
					$"Angular Speed = {handAngularSpeed}(?) " +
					$"DotProd = {dotProd}({isApproaching})"
				);
				return HeuristicState.AllowAll;
			}
			return HeuristicState.DenyAll;
		}
	}
}
