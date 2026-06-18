using UnityEngine;

namespace Lemonity.Core
{
	public class GrabController : GestureControllerBase
	{
		public GrabController(GestureOptions gestureOptions, HeuristicOptions heuristicOptions) : base(gestureOptions, heuristicOptions)
		{
		}

		protected override bool HoldingTest()
		{
			return _hand.GrabValue >= GestureOptions.GrabMaxThreshold;
		}

		protected override bool ReleaseTest()
		{
			return _hand.GrabValue < GestureOptions.GrabMinThreshold;
		}
	}

}
