using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Globalization;

namespace Leanity
{
	[Serializable]
	public class EditorOptionsWindow : EditorWindow, IDisposable
	{
		private static Vector2 _scrollPosition;
		private static Texture _logoTexture;

		AnimBool _showSensitivity = new AnimBool(true);
		AnimBool _showGestures = new AnimBool();
		AnimBool _showPinchGesture = new AnimBool();
		AnimBool _showCamera = new AnimBool();
		AnimBool _showFilters = new AnimBool();
		AnimBool _showInertia = new AnimBool();
		AnimBool _showHeuristic = new AnimBool();
		AnimBool _showDebug = new AnimBool();

		public void OnGUI()
		{
			// Style definition
			var foldoutStyle = EditorStyles.foldout;
			foldoutStyle.fontStyle = FontStyle.Bold;
			foldoutStyle.fontSize = 14;

			int foldoutSpace = 4;

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_logoTexture = _logoTexture ?? Resources.Load<Texture>("logo");
				Rect rImage = GUILayoutUtility.GetRect(50, 1000, 50, 50);
				GUI.DrawTexture(rImage, _logoTexture, ScaleMode.ScaleToFit);


				Options.Enabled = EditorGUILayout.Toggle("Leanity Enabled", Options.Enabled);
				GUILayout.Label("Working gesture", EditorStyles.boldLabel);
				GUIContent[] gestures = new[]
				{
					new GUIContent("One Hand", "Only one hand can move and rotate the camera"),
					new GUIContent("Two Hands", "Two hands are used to move and rotate the camera"),
					new GUIContent("Hybrid", "One or two hands can be used to rotate and move the camera")
				};
				Options.Gesture = (WorkingGesture)GUILayout.Toolbar((int)Options.Gesture, gestures);

				/*
				 * GUILayout.Label("Working mode", EditorStyles.boldLabel);
				GUIContent[] modes = new[]
				{
					new GUIContent("Direct", "Movement of the hand changes position/rotation of the camera"),
					new GUIContent("Speed", "Movement of the hand changes rotational and linear speed of the camera")
				};
				Options.Mode = (WorkingMode)GUILayout.Toolbar((int)Options.Mode, modes);
				*/

				var invertAxisText = new[] { "Move world", "Move camera" };
				GUILayout.Space(4);
				Options.InvertAxis = 1 == GUILayout.SelectionGrid(Options.InvertAxis ? 1 : 0, invertAxisText, 2, EditorStyles.radioButton);
				GUILayout.Space(8);
			}

			#region Sensitivity
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showSensitivity.target = EditorGUILayout.Foldout(_showSensitivity.target, "Sensitivity / Scale", true, foldoutStyle);

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showSensitivity.faded))
				{
					if (_showSensitivity.value)
					{
						GUILayout.Space(4);
						EditorGUI.indentLevel++;

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("World scale");
							// Use logaritmic scale starting from 0
							EditorGUI.BeginChangeCheck();
							float sliderValue = MathHelper.LinearToLogScale(Options.PosScale);
							sliderValue = GUILayout.HorizontalSlider(sliderValue, 0f, 10f);
							Options.PosScale = MathHelper.LogToLinearScale(sliderValue);
							//Options.PosScale = CustomFloatField(Options.PosScale);
							Options.PosScale = CustomFloatField(Options.PosScale);
							if (EditorGUI.EndChangeCheck())
							{
								EditorController.EditorWorkspaceController.GridFadeInEditor();
							}
						}

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Rotation factor");
							Options.RotScale = GUILayout.HorizontalSlider(Options.RotScale, 0f, 5f);
							Options.RotScale = CustomFloatField(Options.RotScale);
						}

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Zoom speed");
							Options.ZoomScale = GUILayout.HorizontalSlider(Options.ZoomScale, 0f, 3f);
							Options.ZoomScale = CustomFloatField(Options.ZoomScale);
						}

						float xRange = HandTracking.Workspace.x * Options.PosScale;
						EditorGUILayout.LabelField("Workspace width around " + xRange.ToString("0.##") + " meters");

						Options.AutoPosScaleOnLoad = EditorGUILayout.Toggle("Auto scale on load", Options.AutoPosScaleOnLoad);

						EditorGUI.indentLevel--;
					}
					GUILayout.Space(foldoutSpace);
				}

			}
			#endregion

			#region Gestures
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showGestures.target = EditorGUILayout.Foldout(_showGestures.target, "Gestures", true, foldoutStyle);

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showGestures.faded))
				{
					if (_showGestures.value)
					{
						GUILayout.Space(4);
						GUILayout.Label("Grab gesture", EditorStyles.boldLabel);
						EditorGUI.indentLevel++;

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Min Threshold");
							Options.GrabMinThreshold = GUILayout.HorizontalSlider(Options.GrabMinThreshold, 0f, 1f);
							Options.GrabMinThreshold = CustomFloatField(Options.GrabMinThreshold);

							if (Options.GrabMinThreshold > Options.GrabMaxThreshold)
							{
								Options.GrabMaxThreshold = Options.GrabMinThreshold;
							}
						}
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Max Threshold");
							Options.GrabMaxThreshold = GUILayout.HorizontalSlider(Options.GrabMaxThreshold, 0f, 1f);
							Options.GrabMaxThreshold = CustomFloatField(Options.GrabMaxThreshold);

							if (Options.GrabMaxThreshold < Options.GrabMinThreshold)
							{
								Options.GrabMinThreshold = Options.GrabMaxThreshold;
							}
						}
						EditorGUI.indentLevel--;

						GUILayout.Label("Pinch gesture", EditorStyles.boldLabel);

						GUILayout.Space(4);
						EditorGUI.indentLevel++;
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Min Threshold");
							Options.PinchMinThreshold = GUILayout.HorizontalSlider(Options.PinchMinThreshold, 0f, 100f);
							Options.PinchMinThreshold = CustomFloatField(Options.PinchMinThreshold);

							if (Options.PinchMinThreshold > Options.PinchMaxThreshold)
							{
								Options.PinchMaxThreshold = Options.PinchMinThreshold;
							}
						}
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Max Threshold");
							Options.PinchMaxThreshold = GUILayout.HorizontalSlider(Options.PinchMaxThreshold, 0f, 100f);
							Options.PinchMaxThreshold = CustomFloatField(Options.PinchMaxThreshold);

							if (Options.PinchMaxThreshold < Options.PinchMinThreshold)
							{
								Options.PinchMinThreshold = Options.PinchMaxThreshold;
							}
						}
						EditorGUI.indentLevel--;
					}
					GUILayout.Space(foldoutSpace);
				}
			}
			#endregion

			#region Camera
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showCamera.target = EditorGUILayout.Foldout(_showCamera.target, "Camera", true, foldoutStyle);

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showCamera.faded))
				{
					if (_showCamera.value)
					{
						GUILayout.Space(4);
						EditorGUI.indentLevel++;

						Options.PitchLimit = EditorGUILayout.Toggle("Pitch limitation", Options.PitchLimit);
						GUI.enabled = Options.PitchLimit;
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Pitch Angle limit");
							Options.PitchLimitAngle = GUILayout.HorizontalSlider(Options.PitchLimitAngle, 0f, 90f);
							Options.PitchLimitAngle = CustomFloatField(Options.PitchLimitAngle);
						}
						GUI.enabled = true;

						Options.RollLimit = EditorGUILayout.Toggle("Roll limitation", Options.RollLimit);

						EditorGUI.indentLevel--;
					}
					GUILayout.Space(foldoutSpace);
				}
			}
			#endregion

			#region Filters
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showFilters.target = EditorGUILayout.Foldout(_showFilters.target, "Filters", true, foldoutStyle);

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
						GUILayout.Space(4);
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
					GUILayout.Space(foldoutSpace);
				}
			}
			#endregion

			#region Inertia
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showInertia.target = EditorGUILayout.Foldout(_showInertia.target, "Inertia", true, foldoutStyle);

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showInertia.faded))
				{
					if (_showInertia.value)
					{
						GUILayout.Space(4);
						EditorGUI.indentLevel++;

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Linear Drag");
							Options.LinearDrag = GUILayout.HorizontalSlider(Options.LinearDrag, 0f, 10f);
							Options.LinearDrag = CustomFloatField(Options.LinearDrag);
						}

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Angular Drag");
							Options.AngularDrag = GUILayout.HorizontalSlider(Options.AngularDrag, 0f, 10f);
							Options.AngularDrag = CustomFloatField(Options.AngularDrag);
						}
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Velocity Frames");
							Options.VelocityFrames = EditorGUILayout.IntField(Options.VelocityFrames);
						}
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Discard Frames");
							Options.DiscardFrames = EditorGUILayout.IntField(Options.DiscardFrames);
						}
						EditorGUI.indentLevel--;
					}
					GUILayout.Space(foldoutSpace);
				}
			}
			#endregion

			#region Heuristics
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showHeuristic.target = EditorGUILayout.Foldout(_showHeuristic.target, "Unwanted gestures heuristic", true, foldoutStyle);

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showHeuristic.faded))
				{
					if (_showHeuristic.value)
					{
						GUILayout.Space(4);
						EditorGUI.indentLevel++;

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Safe zone radius");
							Options.HeuristicRadius = GUILayout.HorizontalSlider(Options.HeuristicRadius, 0.1f, 1f);
							Options.HeuristicRadius = CustomFloatField(Options.HeuristicRadius);
						}
						EditorGUI.indentLevel--;
					}
					GUILayout.Space(foldoutSpace);
				}
			}
			#endregion

			#region Debug
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showDebug.target = EditorGUILayout.Foldout(_showDebug.target, "Debug", true, foldoutStyle);

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showDebug.faded))
				{
					if (_showDebug.value)
					{
						GUILayout.Space(4);
						EditorGUI.indentLevel++;

						EditorGUI.BeginChangeCheck();

						Options.ShowWorkspace = EditorGUILayout.Toggle("Show Workspace", Options.ShowWorkspace);
						Options.ShowGrid = EditorGUILayout.Toggle("Show Grid", Options.ShowGrid);
						GUI.enabled = Options.ShowGrid;
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Grid Divisions");
							Options.NumGridLines = (int)GUILayout.HorizontalSlider(Options.NumGridLines, 0f, 20f);
							Options.NumGridLines = EditorGUILayout.IntField(Options.NumGridLines);
						}
						GUI.enabled = Options.ShowGrid | Options.ShowWorkspace;
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Grid/Workspace Transparency");
							Options.MaxGridTransparency = GUILayout.HorizontalSlider(Options.MaxGridTransparency, 0f, 1f);
							Options.MaxGridTransparency = CustomFloatField(Options.MaxGridTransparency);
						}
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Grid Z Offset");
							Options.TrackingZOffset = GUILayout.HorizontalSlider(Options.TrackingZOffset, 0f, 2f);
							Options.TrackingZOffset = CustomFloatField(Options.TrackingZOffset);
						}
						GUI.enabled = true;
						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Hand Scale");
							Options.HandScale = GUILayout.HorizontalSlider(Options.HandScale, 0f, 2f);
							Options.HandScale = CustomFloatField(Options.HandScale);
						}

						if (EditorGUI.EndChangeCheck())
						{
							EditorController.EditorWorkspaceController.GridFadeInEditor();
						}

						Options.GestureDebug = EditorGUILayout.Toggle("Gesture Debug", Options.GestureDebug);
						Options.ShowHandGuides = EditorGUILayout.Toggle("Hand Guides", Options.ShowHandGuides);

						EditorGUI.indentLevel--;
					}
					GUILayout.Space(foldoutSpace);
				}
			}
			#endregion

			GUILayout.EndScrollView();
			if (GUI.changed)
			{
				Options.Dirty = true;
			}

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
			_showSensitivity.target = true;
			_logoTexture = Resources.Load<Texture>("logo");
		}

		private static float CustomFloatField(float value)
		{
			string floatString = value.ToString("0.00");


			string input = EditorGUILayout.TextField(floatString, GUILayout.Width(70));

			if (input != floatString)
			{
				float inputValue;
				if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out inputValue))
				{
					value = inputValue;
				}
			}

			return value;
		}

	}
}
