using UnityEngine;

namespace Leanity
{
	public class PinchController : GestureControllerBase
	{
		protected override bool HoldingTest()
		{
			return _hand.PinchDistance < Options.PinchMinThreshold;
		}
	}

}
