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
			var mainHandData = HandTracking.RightHandData;
			var auxHandData = HandTracking.LeftHandData;

			GUILayout.BeginVertical();

			GUILayout.Label("Main Hand Data");
			DrawHandData(mainHandData);

			GUILayout.Label("Aux Hand Data");
			DrawHandData(auxHandData);

			Rect r = GUILayoutUtility.GetRect(50, 100, 50, 100);
			r.height = r.width;
			EditorGUI.IndentedRect(r);

			DrawHandPosition(mainHandData, r);
			DrawHandPosition(auxHandData, r);

			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}

		private void DrawHandData(HandData hand)
		{
			GUILayout.BeginHorizontal();
			GUI.enabled = hand.Detected;

			var handTexture = hand.IsRight ? _rightHandTexture : _leftHandTexture;

			if (hand.Detected)
			{
				GUILayout.Box(handTexture);
			}
			GUILayout.Label(hand.Position.ToString());
			GUILayout.EndHorizontal();
			Rect rProgressBar = GUILayoutUtility.GetRect(50, 100, 20, 20);
			EditorGUI.ProgressBar(rProgressBar, hand.GrabValue, "Grab");
		}

		private void DrawHandPosition(HandData hand, Rect r)
		{
			if (hand.Detected)
			{
				Rect rImage = new Rect(r);
				var handTexture = hand.IsRight ? _rightHandTexture : _leftHandTexture;
				rImage.width = handTexture.width;
				rImage.height = handTexture.height;

				float workspaceWidth = 0.4f;
				float workspaceDepth = 0.3f;
				rImage.x += (int)((hand.Position.x / workspaceWidth) * r.width - rImage.width / 2 + r.width / 2);
				rImage.y -= (int)((hand.Position.z / workspaceDepth) * r.height + rImage.height / 2 - r.height / 2);

				//rotatearoundpivot
				GUI.DrawTexture(rImage, handTexture, ScaleMode.ScaleToFit);
			}
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
