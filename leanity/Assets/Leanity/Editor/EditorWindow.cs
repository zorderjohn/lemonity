using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Leanity
{
	[Serializable]
	public class EditorTabWindow : EditorWindow, IDisposable
	{
		private static Vector2 _scrollPosition;
		public void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			GUILayout.BeginVertical();
			GUILayout.Label("Hand Data");
			GUILayout.Label(HandTracking.MainHandData.Position.ToString());


			GUILayout.EndVertical();
			GUILayout.EndScrollView();

		}

		[MenuItem("Window/Leanity &l")]
		public static void Init()
		{
			EditorTabWindow windowInstance = GetWindow(typeof(EditorTabWindow)) as EditorTabWindow;

			if (windowInstance)
			{
				windowInstance.Show();
			}
		}

		public void OnInspectorUpdate()
		{
			// This will only get called 10 times per second.
			Repaint();
		}

		public void Dispose()
		{

		}

	}
}
