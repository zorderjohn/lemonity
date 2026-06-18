using UnityEngine;

namespace Lemonity.Core
{
	public class NullMotion : MotionStyleBase
	{
		public override bool RequiresTwoHands { get { return true; } }

		public NullMotion(Runtime runtime, InertiaOptions inertiaOptions) : base(runtime, inertiaOptions) { }

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
