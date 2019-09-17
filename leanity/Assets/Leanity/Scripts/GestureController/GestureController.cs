using UnityEngine;

namespace Leanity
{
	public interface GestureController {

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
}