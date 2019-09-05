using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;

namespace Leanity
{
	[Serializable]
	public class EditorOptionsWindow : EditorWindow, IDisposable
	{
		private static Vector2 _scrollPosition;
		private static Texture _rightHandTexture;
		private static Texture _leftHandTexture;

		AnimBool _showFilters = new AnimBool();


		public void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				GUILayout.Label("Working gesture", EditorStyles.boldLabel);
				GUIContent[] gestures = new[]
				{
					new GUIContent("One Hand", "Movement of the hand changes position/rotation of the camera"),
					new GUIContent("Two Hands", "Movement of the hand changes rotational and linear speed of the camera")
				};
				Options.Gesture = (WorkingGesture)GUILayout.Toolbar((int)Options.Gesture, gestures);

				GUILayout.Label("Working mode", EditorStyles.boldLabel);
				GUIContent[] modes = new[]
				{
					new GUIContent("Direct", "Movement of the hand changes position/rotation of the camera"),
					new GUIContent("Speed", "Movement of the hand changes rotational and linear speed of the camera")
				};
				//Options.Mode = (WorkingMode)GUILayout.SelectionGrid((int)Options.Mode, modes, 2);
				Options.Mode = (WorkingMode)GUILayout.Toolbar((int)Options.Mode, modes);

				var invertAxisText = new[] { "Move world", "Move camera" };
				GUILayout.Space(4);
				Options.InvertAxis = 1 == GUILayout.SelectionGrid(Options.InvertAxis ? 1 : 0, invertAxisText, 2, EditorStyles.radioButton);
				GUILayout.Space(8);
			}

			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{

				GUILayout.Label("Sensitivity", EditorStyles.boldLabel);
				GUILayout.Space(4);
				EditorGUI.indentLevel++;

				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Translation");
					Options.PosScale = GUILayout.HorizontalSlider(Options.PosScale, 0f, 50f);
					Options.PosScale = EditorGUILayout.FloatField(Options.PosScale, GUILayout.Width(50));
				}

				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Rotation");
					Options.RotScale = GUILayout.HorizontalSlider(Options.RotScale, 0f, 5f);
					Options.RotScale = EditorGUILayout.FloatField(Options.RotScale, GUILayout.Width(50));
				}

				EditorGUI.indentLevel--;

				GUILayout.Space(8);
			}

			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				GUILayout.Label("Grab Gesture", EditorStyles.boldLabel);
				GUILayout.Space(4);
				EditorGUI.indentLevel++;
				//Options.GrabEnabled = EditorGUILayout.ToggleLeft("Enable", Options.GrabEnabled);
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Min Threshold");
					Options.GrabMinThreshold = GUILayout.HorizontalSlider(Options.GrabMinThreshold, 0f, 1f);
					Options.GrabMinThreshold = EditorGUILayout.FloatField(Options.GrabMinThreshold, GUILayout.Width(50));

					if (Options.GrabMinThreshold > Options.GrabMaxThreshold)
					{
						Options.GrabMaxThreshold = Options.GrabMinThreshold;
					}
				}
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Max Threshold");
					Options.GrabMaxThreshold = GUILayout.HorizontalSlider(Options.GrabMaxThreshold, 0f, 1f);
					Options.GrabMaxThreshold = EditorGUILayout.FloatField(Options.GrabMaxThreshold, GUILayout.Width(50));

					if (Options.GrabMaxThreshold < Options.GrabMinThreshold)
					{
						Options.GrabMinThreshold = Options.GrabMaxThreshold;
					}
				}

				EditorGUI.indentLevel--;
				GUILayout.Space(8);
				GUILayout.Label("Camera", EditorStyles.boldLabel);
				GUILayout.Space(4);
				EditorGUI.indentLevel++;
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Pitch limit");
					Options.PitchLimit = GUILayout.HorizontalSlider(Options.PitchLimit, 0f, 90f);
					Options.PitchLimit = EditorGUILayout.FloatField(Options.PitchLimit, GUILayout.Width(50));
				}

				EditorGUI.indentLevel--;

				EditorGUI.indentLevel--;
				GUILayout.Space(8);
				GUILayout.Label("Advanced Options", EditorStyles.boldLabel);
				GUILayout.Space(4);
				EditorGUI.indentLevel++;

				#region Filters
				_showFilters.target = EditorGUILayout.Toggle("Configure filters", _showFilters.target);
				GUIContent[] filterOptions = new[]
				{
					new GUIContent("Frecuency", "Expected data frequency"),
					new GUIContent("Min Cutoff", "Lower values reduce jitter"),
					new GUIContent("Beta", "Higher values reduce high speed lag"),
					new GUIContent("D Cutoff", "Cutoff for derivative")
				};

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showFilters.faded))
				{
					if (_showFilters.value)
					{
						EditorGUI.indentLevel++;
						Options.FilterFrequency = EditorGUILayout.DelayedFloatField(filterOptions[0], Options.FilterFrequency);
						EditorGUILayout.PrefixLabel("Rotation");
						EditorGUI.indentLevel++;
						Options.RotFilterMinCutoff = EditorGUILayout.DelayedFloatField(filterOptions[1], Options.RotFilterMinCutoff);
						Options.RotFilterBeta = EditorGUILayout.DelayedFloatField(filterOptions[2], Options.RotFilterBeta);
						Options.RotFilterDcutoff = EditorGUILayout.DelayedFloatField(filterOptions[3], Options.RotFilterDcutoff);
						EditorGUI.indentLevel--;

						EditorGUILayout.PrefixLabel("Translation");
						EditorGUI.indentLevel++;
						Options.PosFilterMinCutoff = EditorGUILayout.DelayedFloatField(filterOptions[1], Options.PosFilterMinCutoff);
						Options.PosFilterBeta = EditorGUILayout.DelayedFloatField(filterOptions[2], Options.PosFilterBeta);
						Options.PosFilterDcutoff = EditorGUILayout.DelayedFloatField(filterOptions[3], Options.PosFilterDcutoff);
						EditorGUI.indentLevel--;

						EditorGUI.indentLevel--;
					}
				}
				#endregion
				GUILayout.Space(8);
			}

			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				GUILayout.Label("Inertia", EditorStyles.boldLabel);
				GUILayout.Space(4);
				Options.EnableInertia = EditorGUILayout.Toggle("Enable Inertia", Options.EnableInertia);
				if (Options.EnableInertia)
				{
					using (var horizontalScope = new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PrefixLabel("Linear Drag");
						Options.LinearDrag = GUILayout.HorizontalSlider(Options.LinearDrag, 0f, 1f);
						Options.LinearDrag = EditorGUILayout.FloatField(Options.LinearDrag, GUILayout.Width(50));
					}

					using (var horizontalScope = new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PrefixLabel("Angular Drag");
						Options.AngularDrag = GUILayout.HorizontalSlider(Options.AngularDrag, 0f, 1f);
						Options.AngularDrag = EditorGUILayout.FloatField(Options.AngularDrag, GUILayout.Width(50));
					}
					using (var horizontalScope = new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PrefixLabel("Velocity Frames");
						Options.VelocityFrames = EditorGUILayout.IntField(Options.VelocityFrames, GUILayout.Width(50));
					}
					using (var horizontalScope = new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PrefixLabel("Discard Frames");
						Options.DiscardFrames = EditorGUILayout.IntField(Options.DiscardFrames, GUILayout.Width(50));
					}
				}
			}

			GUILayout.EndScrollView();
			GUI.enabled = Options.Dirty;
			if (GUILayout.Button("Save"))
			{
				Options.Save();
			}

			GUI.enabled = true;

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
