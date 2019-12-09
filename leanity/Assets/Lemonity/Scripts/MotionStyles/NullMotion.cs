using UnityEngine;

namespace Lemonity
{
	public class NullMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return true; } }

		public NullMotion(Runtime runtime) : base(runtime) { }

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
