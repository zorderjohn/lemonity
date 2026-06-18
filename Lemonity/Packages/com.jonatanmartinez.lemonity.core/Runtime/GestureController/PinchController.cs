using UnityEngine;

namespace Lemonity.Core
{
	public class PinchController : GestureControllerBase
	{
		public PinchController(GestureOptions gestureOptions, HeuristicOptions heuristicOptions) : base(gestureOptions, heuristicOptions)
		{
		}

		protected override bool HoldingTest()
		{
			return _hand.PinchDistance < GestureOptions.PinchMinThreshold;
		}

		protected override bool ReleaseTest()
		{
			return _hand.PinchDistance > GestureOptions.PinchMaxThreshold;
		}

		protected override HeuristicState HeuristicCondition()
		{
			return HeuristicState.AllowAll;
		}
	}

}
