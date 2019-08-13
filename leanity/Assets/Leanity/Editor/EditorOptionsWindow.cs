using UnityEngine;
using UnityEditor;
using System;

namespace Leanity
{
	[Serializable]
	public class EditorOptionsWindow : EditorWindow, IDisposable
	{
		private static Vector2 _scrollPosition;
		private static Texture _rightHandTexture;
		private static Texture _leftHandTexture;


		public void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			GUILayout.BeginVertical();

			GUILayout.Label("Working mode", EditorStyles.largeLabel);
			GUIContent[] modes = new[]
			{
				new GUIContent("Direct", "Movement of the hand changes position/rotation of the camera"),
				new GUIContent("Speed", "Movement of the hand changes rotational and linear speed of the camera")
			};
			Options.Mode = (WorkingMode)GUILayout.SelectionGrid((int)Options.Mode, modes, 2);

			GUILayout.Space(8);
			GUILayout.Label("Sensitivity", EditorStyles.largeLabel);
			GUILayout.Space(4);

			GUILayout.Label("Translation");
			GUILayout.BeginHorizontal();
			Options.PosScale = EditorGUILayout.FloatField(Options.PosScale, GUILayout.Width(50));
			Options.PosScale = GUILayout.HorizontalSlider(Options.PosScale, 0f, 10f);
			GUILayout.EndHorizontal();

			GUILayout.Label("Rotation");
			GUILayout.BeginHorizontal();
			Options.RotScale = EditorGUILayout.FloatField(Options.RotScale, GUILayout.Width(50));
			Options.RotScale = GUILayout.HorizontalSlider(Options.RotScale, 0f, 10f);
			GUILayout.EndHorizontal();

			Options.InvertAxis = EditorGUILayout.ToggleLeft("Invert axis", Options.InvertAxis);
			GUILayout.Space(8);
			GUILayout.Label("Grab Gesture", EditorStyles.largeLabel);
			GUILayout.Space(4);
			Options.GrabEnabled = EditorGUILayout.ToggleLeft("Enable", Options.GrabEnabled);
			GUILayout.BeginHorizontal();
			Options.GrabThreshold = EditorGUILayout.FloatField(Options.GrabThreshold, GUILayout.Width(50));
			Options.GrabThreshold = GUILayout.HorizontalSlider(Options.GrabThreshold, 0f, 1f);
			GUILayout.EndHorizontal();

			GUILayout.Label("Pitch Limit");
			GUILayout.BeginHorizontal();
			Options.PitchLimit = EditorGUILayout.FloatField(Options.PitchLimit, GUILayout.Width(50));
			Options.PitchLimit = GUILayout.HorizontalSlider(Options.PitchLimit, 0f, 90f);
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}


		[MenuItem("Window/Leanity Options &l")]
		public static void Init()
		{
			GetWindow(typeof(EditorOptionsWindow), false, "Leanity Options", true);
		}

		public void OnInspectorUpdate()
		{
			// This will only get called 10 times per second.
			Repaint();
		}

		public void Dispose()
		{

		}

		public void Awake()
		{
			Options.Load();
		}

	}
}
