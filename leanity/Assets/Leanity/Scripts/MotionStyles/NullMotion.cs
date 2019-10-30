using UnityEngine;

namespace Leanity
{
	public class NullMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return true; } }

		protected override void StartMotion()
		{
		}

		protected override void UpdateMotion()
		{
		}

		public override bool InertialMovement()
		{
			return false;
		}

		protected override void UpdateInertialData()
		{
		}
	}
}
