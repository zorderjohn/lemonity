using UnityEngine;

namespace Leanity
{
	public class GrabController : GestureControllerBase
	{
		protected override bool HoldingTest()
		{
			return _hand.GrabValue >= Options.GrabMaxThreshold;
		}

		protected override bool ReleaseTest()
		{
			return _hand.GrabValue < Options.GrabMinThreshold;
		}
	}

}
