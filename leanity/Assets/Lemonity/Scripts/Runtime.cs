using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lemonity
{
	public class Runtime
	{
		public Func<Vector3> SelectionCenter { get; set; }

		public Runtime()
		{
			SelectionCenter = () => Vector3.zero;
		}
	}
}