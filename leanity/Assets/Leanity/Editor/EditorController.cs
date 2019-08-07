using UnityEngine;
using UnityEditor;
using System;

namespace Leanity
{
	[InitializeOnLoad]
	[Serializable]
	class EditorController
	{

		static EditorController()
		{
			EditorApplication.update += Update;
		}

		static void Update()
		{
			
		}
	}
}