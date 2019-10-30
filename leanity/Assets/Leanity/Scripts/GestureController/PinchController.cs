using UnityEngine;

namespace Lemonity
{
	public class PinchController : GestureControllerBase
	{
		protected override bool HoldingTest()
		{
			return _hand.PinchDistance < Options.PinchMinThreshold;
		}

		protected override bool ReleaseTest()
		{
			return _hand.PinchDistance > Options.PinchMaxThreshold;
		}

		protected override HeuristicState HeuristicCondition()
		{
			return HeuristicState.AllowAll;
		}
	}

}
