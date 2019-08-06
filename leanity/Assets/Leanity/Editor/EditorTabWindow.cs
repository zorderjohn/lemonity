using UnityEngine;
using UnityEditor;
using System;

namespace Leanity
{
	[Serializable]
	public class EditorTabWindow : EditorWindow, IDisposable
	{
		private static Vector2 _scrollPosition;
		private static Texture _rightHandTexture;
		private static Texture _leftHandTexture;

		public void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			var mainHandData = HandTracking.MainHandData;
			var auxHandData = HandTracking.AuxHandData;

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUI.enabled = mainHandData.Detected;
			GUILayout.Label("Main Hand Data");
			if (mainHandData.Detected)
			{
				if (mainHandData.IsRight) { GUILayout.Box(_rightHandTexture); }
				else { GUILayout.Box(_leftHandTexture); }
			}
			GUILayout.Label(mainHandData.Position.ToString());
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUI.enabled = auxHandData.Detected;
			GUILayout.Label("Aux Hand Data");
			if (auxHandData.Detected)
			{
				if (auxHandData.IsRight) { GUILayout.Box(_rightHandTexture); }
				else { GUILayout.Box(_leftHandTexture); }
			}

			GUILayout.Label(auxHandData.Position.ToString());
			GUILayout.EndHorizontal();

			Rect r = GUILayoutUtility.GetRect(50, 100, 50, 100);
			r.height = r.width;
			Rect rImage = new Rect(r);
			rImage.width = _rightHandTexture.width;
			rImage.height = _rightHandTexture.height;

			float workspaceWidth = 0.3f;
			float workspaceDepth = 0.3f;
			rImage.x += (int)( (mainHandData.Position.x / workspaceWidth) * r.width - rImage.width/2 + r.width/2);
			rImage.y -= (int)( (mainHandData.Position.z / workspaceDepth) * r.height + rImage.height/2 - r.height/2);

			GUI.DrawTexture(rImage, _rightHandTexture,ScaleMode.ScaleToFit);

			GUILayout.EndVertical();
			GUILayout.EndScrollView();


		}

		[MenuItem("Window/Leanity &l")]
		public static void Init()
		{
			GetWindow(typeof(EditorTabWindow), false, "Leanity", true);
		}

		public void OnInspectorUpdate()
		{
			// This will only get called 10 times per second.
			Repaint();
		}

		public void Dispose()
		{

		}

		private static void LoadResources()
		{
			if (_rightHandTexture == null)
			{
				_rightHandTexture = (Texture)EditorGUIUtility.Load("Assets/Leanity/Editor/Resources/RightHand.png");
				_leftHandTexture = (Texture)EditorGUIUtility.Load("Assets/Leanity/Editor/Resources/LeftHand.png");
			}
		}
		public void Awake()
		{
			LoadResources();
		}
		private void OnEnable()
		{
			LoadResources();
		}

	}
}
