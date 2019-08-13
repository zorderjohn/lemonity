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
		private readonly float _workspaceWidth = 0.5f;
		private readonly float _workspaceDepth = 0.5f;

		public void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			var rightHandData = HandTracking.RightHandData;
			var leftHandData = HandTracking.LeftHandData;

			GUILayout.BeginVertical();

			GUILayout.Label("Right Hand Data");
			DrawHandData(rightHandData);

			GUILayout.Label("Left Hand Data");
			DrawHandData(leftHandData);

			Rect r = GUILayoutUtility.GetRect(50, 100, 50, 100);
			r.height = r.width;
			EditorGUI.IndentedRect(r);

			DrawHandPosition(rightHandData, r);
			DrawHandPosition(leftHandData, r);

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
			GUILayout.Label(hand.Detected ? (hand.Position * 1000f).ToString() : "(--, --, --)");
			GUILayout.EndHorizontal();
			Rect rProgressBar = GUILayoutUtility.GetRect(50, 100, 20, 20);
			EditorGUI.ProgressBar(rProgressBar, hand.Detected ? hand.GrabValue : 0f, "Grab");
		}

		private void DrawHandPosition(HandData hand, Rect r)
		{
			if (hand.Detected)
			{
				Rect rImage = new Rect(r);
				var handTexture = hand.IsRight ? _rightHandTexture : _leftHandTexture;
				rImage.width = handTexture.width;
				rImage.height = handTexture.height;


				rImage.x += (int)((hand.Position.x / _workspaceWidth) * r.width - rImage.width / 2 + r.width / 2);
				rImage.y -= (int)((hand.Position.z / _workspaceDepth) * r.height + rImage.height / 2 - r.height / 2);

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
