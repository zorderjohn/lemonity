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
		private static Texture _lemonTexture;
		private static Texture _treeTexture;
		private static Texture _cityTexture;
		private static readonly Color _logoBackground = new Color(.110f, .184f, .196f);

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

			#region Header
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_logoTexture = _logoTexture ?? Resources.Load<Texture>("logo");
				Rect rImage = GUILayoutUtility.GetRect(50, 1000, 50, 50);
				EditorGUI.DrawRect(rImage, _logoBackground);
				GUI.DrawTexture(rImage, _logoTexture, ScaleMode.ScaleToFit);

				GUILayout.Label("Mode", EditorStyles.boldLabel);
				GUIContent[] gestures = new[]
				{
					new GUIContent("Off", "Disable Lemonity"),
					new GUIContent("1 Hand", "Only one hand at a time can move and rotate the camera"),
					new GUIContent("2 Hands", "Two hands are used to move and rotate the camera"),
					new GUIContent("1&2 Hands", "One or two hands can be used to rotate and move the camera"),
					new GUIContent("Orbit", "Rotate the camera around the selected object")
				};
				Options.Gesture = (WorkingGesture)GUILayout.Toolbar((int)Options.Gesture, gestures/*, GUILayout.MaxWidth(350)*/);

				GUIContent[] invertAxisContent = new[]
				{
					new GUIContent("Move world", "Normal operation, hand movement corresponds to movement of the world"),
					new GUIContent("Move camera", "Hand movement corresponds to movement of the camera (invert axis)")
				};

				GUILayout.Space(4);
				Options.InvertAxis = 1 == GUILayout.SelectionGrid(Options.InvertAxis ? 1 : 0, invertAxisContent, 2, EditorStyles.radioButton);
				GUILayout.Space(foldoutSpace);
			}
			#endregion

			#region Sensitivity
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showSensitivity.target = EditorGUILayout.Foldout(_showSensitivity.target, "Sensitivity / Scale", true, foldoutStyle);


				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showSensitivity.faded))
				{
					if (_showSensitivity.value)
					{
						GUILayout.Space(4);
						//EditorGUI.indentLevel++;

						EditorGUI.BeginChangeCheck();

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							_lemonTexture = _lemonTexture ?? Resources.Load<Texture>("lemon_32");
							_treeTexture = _treeTexture ?? Resources.Load<Texture>("tree_32");
							_cityTexture = _cityTexture ?? Resources.Load<Texture>("city_32");
							Rect rImage = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32));
							GUI.DrawTexture(rImage, _lemonTexture);

							GUILayout.FlexibleSpace();
							rImage = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32));
							GUI.DrawTexture(rImage, _treeTexture);

							GUILayout.FlexibleSpace();
							rImage = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32));
							GUI.DrawTexture(rImage, _cityTexture);

						}

						// Use logaritmic scale starting from 0
						float sliderLogValue = MathHelper.LinearToLogScale(Options.PosScale);
						sliderLogValue = GUILayout.HorizontalSlider(sliderLogValue, .1f, 10f);
						if (EditorGUI.EndChangeCheck())
						{
							Options.PosScale = MathHelper.LogToLinearScale(sliderLogValue);
							EditorController.EditorWorkspaceController.GridFadeInEditor();
						}

						float xRange = HandTracking.Workspace.x * Options.PosScale;
						string strScale = Options.PosScale.ToString("0.0");
						string strWidth = xRange.ToString("0.##");

						using (var horizontalScope = new GUILayout.HorizontalScope())
						{
							EditorGUILayout.PrefixLabel("Scene Scale");
							EditorGUILayout.LabelField($"1 : {strScale}  ({strWidth} m.)");
						}

						Options.AutoPosScaleOnLoad = EditorGUILayout.Toggle("Auto scale on load", Options.AutoPosScaleOnLoad);

						GUILayout.Space(4);
						Options.RotScale = CustomFloatField(Options.RotScale, "Rotation Factor", 0.5f, 5f);

						GUILayout.Space(4);
						Options.ZoomScale = CustomFloatField(Options.ZoomScale, "Zoom factor", 0f, 3f);

						//EditorGUI.indentLevel--;
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

						Options.GrabMinThreshold = CustomFloatField(Options.GrabMinThreshold, "Min Threshold", 0f, 1f);
						if (Options.GrabMinThreshold > Options.GrabMaxThreshold) { Options.GrabMaxThreshold = Options.GrabMinThreshold; }

						Options.GrabMaxThreshold = CustomFloatField(Options.GrabMaxThreshold, "Max Threshold", 0f, 1f);
						if (Options.GrabMaxThreshold < Options.GrabMinThreshold) { Options.GrabMinThreshold = Options.GrabMaxThreshold; }
						EditorGUI.indentLevel--;

						GUILayout.Label("Pinch gesture", EditorStyles.boldLabel);

						GUILayout.Space(4);
						EditorGUI.indentLevel++;
						Options.PinchMinThreshold = CustomFloatField(Options.PinchMinThreshold, "Min Threshold", 0f, 100f);
						if (Options.PinchMinThreshold > Options.PinchMaxThreshold) { Options.PinchMaxThreshold = Options.PinchMinThreshold; }

						Options.PinchMaxThreshold = CustomFloatField(Options.PinchMaxThreshold, "Max Threshold", 0f, 100f);
						if (Options.PinchMaxThreshold < Options.PinchMinThreshold) { Options.PinchMinThreshold = Options.PinchMaxThreshold; }
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
						Options.PitchLimitAngle = CustomFloatField(Options.PitchLimitAngle, "Pitch Angle Limit", 0f, 90f);
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
				_showFilters.target = EditorGUILayout.Foldout(_showFilters.target, "Advanced motion filter", true, foldoutStyle);

				const string freqTooltip =  "Expected data frequency";
				const string minCOTooltip = "Minimum frequency cutoff. Lower values reduce jitter";
				const string betaTooltip = "Higher values reduce high speed lag";
				const string dCOTooltip = "Frequency cutoff for derivative";

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showFilters.faded))
				{
					if (_showFilters.value)
					{
						GUILayout.Space(4);

						Options.FilterFrequency = CustomFloatField(Options.FilterFrequency, "Frequency", freqTooltip);
						GUILayout.Label("Rotation", EditorStyles.boldLabel);
						EditorGUI.indentLevel++;
						Options.RotFilterMinCutoff = CustomFloatField(Options.RotFilterMinCutoff, "Min CutOff", 0f, 5f, minCOTooltip);
						Options.RotFilterBeta = CustomFloatField(Options.RotFilterBeta, "Beta", 1f, 20f, betaTooltip);
						Options.RotFilterDcutoff = CustomFloatField(Options.RotFilterDcutoff, "Deriv Cutoff", 1f, 2f, dCOTooltip);
						EditorGUI.indentLevel--;

						GUILayout.Label("Translation", EditorStyles.boldLabel);
						EditorGUI.indentLevel++;
						Options.PosFilterMinCutoff = CustomFloatField(Options.PosFilterMinCutoff, "Min CutOff", 0f, 5f, minCOTooltip);
						Options.PosFilterBeta = CustomFloatField(Options.PosFilterBeta, "Beta", 1f, 20f, betaTooltip);
						Options.PosFilterDcutoff = CustomFloatField(Options.PosFilterDcutoff, "Deriv Cutoff", 1f, 2f, dCOTooltip);
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
						Options.EnableInertia = EditorGUILayout.Toggle("Enable Inertia", Options.EnableInertia);
						Options.LinearDrag = CustomFloatField(Options.LinearDrag, "Linear Drag", 0f, 10f);
						Options.AngularDrag = CustomFloatField(Options.AngularDrag, "Angular Drag", 0f, 10f);
						// Options.VelocityFrames = CustomIntField(Options.VelocityFrames, "Velocity Frames", 2, 10);
						// Options.DiscardFrames = CustomIntField(Options.DiscardFrames, "Discard Frames", 1, 5);
						EditorGUI.indentLevel--;
					}
					GUILayout.Space(foldoutSpace);
				}
			}
			#endregion

			#region Heuristics
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showHeuristic.target = EditorGUILayout.Foldout(_showHeuristic.target, "Gesture filter", true, foldoutStyle);

				using (var filterGroup = new EditorGUILayout.FadeGroupScope(_showHeuristic.faded))
				{
					if (_showHeuristic.value)
					{
						GUILayout.Space(4);
						EditorGUI.indentLevel++;

						Options.HeuristicEnabled = EditorGUILayout.Toggle("Enable filter", Options.HeuristicEnabled);
						Options.HeuristicRadius = CustomFloatField(Options.HeuristicRadius, "Safe zone radius", 0.1f, 1f);

						GUILayout.Space(4);
						EditorGUILayout.LabelField("Gestures that start out of the safe zone in outward direction are considered involuntary and filtered.\n" +
							"Check the safe zone circle in the Debug window.", EditorStyles.wordWrappedLabel);

						EditorGUI.indentLevel--;
					}
					GUILayout.Space(foldoutSpace);
				}
			}
			#endregion

			#region Visuals
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showDebug.target = EditorGUILayout.Foldout(_showDebug.target, "Visuals", true, foldoutStyle);

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
						Options.NumGridLines = CustomIntField(Options.NumGridLines, "Grid Divisions", 0, 20);

						GUI.enabled = Options.ShowGrid | Options.ShowWorkspace;
						Options.MaxGridTransparency = CustomFloatField(Options.MaxGridTransparency, "Transparency", 0f, 1f);
						Options.TrackingZOffset = CustomFloatField(Options.TrackingZOffset, "Grid Z Offset", 0f, 2f);

						GUI.enabled = true;
						Options.HandScale = CustomFloatField(Options.HandScale, "Hand Scale", 0f, 2f);

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

		private static int CustomIntField(int value, string prefixLabel, int minSlider, int maxSlider, string tooltip = "")
		{
			var content = new GUIContent(prefixLabel, tooltip);
			using (var horizontalScope = new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(content);
				value = (int)GUILayout.HorizontalSlider(value, minSlider, maxSlider);
				return EditorGUILayout.DelayedIntField(value, GUILayout.Width(50));
			}
		}

		private static float CustomFloatField(float value, string prefixLabel, string tooltip = "")
		{
			var content = new GUIContent(prefixLabel, tooltip);
			using (var horizontalScope = new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(content);
				return CustomFloatField(value);
			}
		}

		private static float CustomFloatField(float value, string prefixLabel, float minSlider, float maxSlider, string tooltip = "")
		{
			var content = new GUIContent(prefixLabel, tooltip);
			using (var horizontalScope = new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(content);
				value = GUILayout.HorizontalSlider(value, minSlider, maxSlider);
				return CustomFloatField(value);
			}
		}

		private static float CustomFloatField(float value)
		{
			string floatString = value.ToString("0.00");

			string input = EditorGUILayout.DelayedTextField(floatString, GUILayout.Width(50));

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
