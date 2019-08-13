using UnityEngine;
using UnityEditor;
using System;

namespace Leanity
{
	[InitializeOnLoad]
	[Serializable]
	class EditorController
	{
		static MotionController _motion;

		static EditorController()
		{
			_motion = new MotionController();
			EditorApplication.update += Update;
		}

		static void Update()
		{
			HandTracking.Update();
		}
	}
}