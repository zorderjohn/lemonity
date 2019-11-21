using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Globalization;

namespace Lemonity
{
	[Serializable]
	public class EditorOptionsWindow : EditorWindow, IDisposable
	{
		private enum Mode { Off = 0, Grab, Orbit, Fly};
		private enum SubMode { OneHand = 0, TwoHands, AnyHands};

		private static Vector2 _scrollPosition;
		private static Texture _logoTexture;
		private static Texture _lemonTexture;
		private static Texture _treeTexture;
		private static Texture _cityTexture;
		private static Texture _offTexture;
		private static Texture _grabTexture;
		private static Texture _orbitTexture;
		private static Texture _flyTexture;
		private static readonly Color _logoBackground = new Color(.2196f, .3529f, .4471f);
		private const int _inputTextWidth = 55;
		private const int _foldoutSpace = 4;
		private const int _foldoutFontSize = 14;
		private static GUIStyle _foldoutStyle;

		AnimBool _showSensitivity = new AnimBool(true);
		AnimBool _showGestures = new AnimBool();
		AnimBool _showCamera = new AnimBool();
		AnimBool _showFilters = new AnimBool();
		AnimBool _showInertia = new AnimBool();
		AnimBool _showHeuristic = new AnimBool();
		AnimBool _showDebug = new AnimBool();
		AnimBool _showFly = new AnimBool();

		public void OnGUI()
		{
			InitStyles();

			Mode mode;
			SubMode subMode;
			GestureToMode(Options.Gesture, out mode, out subMode);

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			#region Header
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_logoTexture = _logoTexture ?? Resources.Load<Texture>("logo");
				_offTexture = _offTexture ?? Resources.Load<Texture>("power");
				//_grabTexture = _grabTexture ?? Resources.Load<Texture>("grab");
				//_orbitTexture = _orbitTexture ?? Resources.Load<Texture>("orbit");
				//_flyTexture = _flyTexture ?? Resources.Load<Texture>("fly");

				Rect rImage = GUILayoutUtility.GetRect(50, 1000, 50, 50);
				EditorGUI.DrawRect(rImage, _logoBackground);
				GUI.DrawTexture(rImage, _logoTexture, ScaleMode.ScaleToFit);

				if (!HandTracking.IsConnected())
				{
					GUILayout.Space(4);
					EditorDebugWindow.DrawStatusLabel();
				}

				GUILayout.Label("Mode", EditorStyles.boldLabel);
				GUIContent[] gestures = new[]
				{
					new GUIContent("Off", "Disable Lemonity"),
					//new GUIContent("Off", _offTexture, "Disable Lemonity"),
					new GUIContent("Grab", "Move & Rotate the camera using current hand position in workspace as the pivot"),
					new GUIContent("Orbit", "Rotate the camera around using the selected object as the pivot"),
					new GUIContent("Fly", "Fly around the scene")
				};

				mode = (Mode) GUILayout.Toolbar((int)mode, gestures);

				if (mode == Mode.Grab)
				{
					GUILayout.Space(4);
					GUILayout.Label("Sub-Mode", EditorStyles.boldLabel);
					GUIContent[] panOptions = new[]
					{
					new GUIContent("One Hand", "Only one hand at a time can move and rotate the camera"),
					new GUIContent("Two Hands", "Two hands are used to move and rotate the camera"),
					new GUIContent("Any Hands", "One or two hands can be used to rotate and move the camera"),
					};

					subMode = (SubMode)GUILayout.Toolbar((int)subMode, panOptions);
				}
				else if (mode == Mode.Fly)
				{
					GUILayout.Space(4);
					GUILayout.Label("Sub-Mode", EditorStyles.boldLabel);
					GUIContent[] panOptions = new[]
					{
					new GUIContent("One Hand", "Only one hand at a time can move and rotate the camera"),
					new GUIContent("Two Hands", "Two hands are used to move and rotate the camera"),
					};

					subMode = (SubMode)GUILayout.Toolbar((int)subMode, panOptions);
				}

				Options.Gesture = ModeToGesture(mode, subMode);

				GUIContent[] invertAxisContent = new[]
				{
					new GUIContent("Move world", "Normal operation, hand movement corresponds to movement of the world"),
					new GUIContent("Move camera", "Hand movement corresponds to movement of the camera (invert axis)")
				};

				GUILayout.Space(4);
				if (mode == Mode.Grab)
				{
					Options.GrabInvertAxis = 1 == GUILayout.SelectionGrid(Options.GrabInvertAxis ? 1 : 0, invertAxisContent, 2, EditorStyles.radioButton);
				}
				else if (mode == Mode.Orbit)
				{
					Options.OrbitInvertAxis = 1 == GUILayout.SelectionGrid(Options.OrbitInvertAxis ? 1 : 0, invertAxisContent, 2, EditorStyles.radioButton);
				}
				else if (mode == Mode.Fly)
				{
					Options.FlyInvertAxis = 1 == GUILayout.SelectionGrid(Options.FlyInvertAxis ? 1 : 0, invertAxisContent, 2, EditorStyles.radioButton);
				}
				GUILayout.Space(_foldoutSpace);
			}
			#endregion

			if (mode == Mode.Fly)
			{
				FlyModeOptions();
			}

			if (Options.Gesture != WorkingGesture.Disabled)
			{
				#region Sensitivity
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					_showSensitivity.target = EditorGUILayout.Foldout(_showSensitivity.target, "Sensitivity / Scale", true, _foldoutStyle);


					using (new EditorGUILayout.FadeGroupScope(_showSensitivity.faded))
					{
						if (_showSensitivity.value)
						{
							GUILayout.Space(4);

							if (Options.Gesture == WorkingGesture.Orbit)
							{
								ScaleGUIOrbit();
							}
							else if (Options.Gesture == WorkingGesture.FlyOneHand || Options.Gesture == WorkingGesture.FlyTwoHands)
							{
								ScaleGUIFly();
							}
							else
							{
								ScaleGUI();
							}
						}
						GUILayout.Space(_foldoutSpace);
					}
				}
				#endregion

				#region Gestures
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					_showGestures.target = EditorGUILayout.Foldout(_showGestures.target, "Gestures", true, _foldoutStyle);

					using (new EditorGUILayout.FadeGroupScope(_showGestures.faded))
					{
						if (_showGestures.value)
						{
							GUILayout.Space(4);
							GUILayout.Label("Grab gesture", EditorStyles.boldLabel);
							EditorGUI.indentLevel++;

							Options.GrabMaxThreshold = CustomFloatField(Options.GrabMaxThreshold, "Start Threshold", 0f, 1f);
							if (Options.GrabMaxThreshold < Options.GrabMinThreshold) { Options.GrabMinThreshold = Options.GrabMaxThreshold; }

							Options.GrabMinThreshold = CustomFloatField(Options.GrabMinThreshold, "Stop Threshold", 0f, 1f);
							if (Options.GrabMinThreshold > Options.GrabMaxThreshold) { Options.GrabMaxThreshold = Options.GrabMinThreshold; }
							EditorGUI.indentLevel--;

							GUILayout.Label("Pinch gesture", EditorStyles.boldLabel);

							GUILayout.Space(4);
							EditorGUI.indentLevel++;

							Options.PinchMinThreshold = CustomFloatField(Options.PinchMinThreshold, "Start Distance", 0f, 100f);
							if (Options.PinchMinThreshold > Options.PinchMaxThreshold) { Options.PinchMaxThreshold = Options.PinchMinThreshold; }

							Options.PinchMaxThreshold = CustomFloatField(Options.PinchMaxThreshold, "Stop Distance", 0f, 100f);
							if (Options.PinchMaxThreshold < Options.PinchMinThreshold) { Options.PinchMinThreshold = Options.PinchMaxThreshold; }

							EditorGUI.indentLevel--;
						}
						GUILayout.Space(_foldoutSpace);
					}
				}
				#endregion

				#region Camera
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					_showCamera.target = EditorGUILayout.Foldout(_showCamera.target, "Camera Angle Limits", true, _foldoutStyle);

					using (new EditorGUILayout.FadeGroupScope(_showCamera.faded))
					{
						if (_showCamera.value)
						{
							GUILayout.Space(4);
							EditorGUI.indentLevel++;

							Options.PitchLimit = EditorGUILayout.Toggle("Pitch limitation", Options.PitchLimit);
							GUI.enabled = Options.PitchLimit;
							Options.PitchMinAngleLimit = CustomFloatField(Options.PitchMinAngleLimit, "Min pitch limit", 0f, 90f);
							if (Options.PitchMinAngleLimit > Options.PitchMaxAngleLimit) { Options.PitchMaxAngleLimit = Options.PitchMinAngleLimit; }
							Options.PitchMaxAngleLimit = CustomFloatField(Options.PitchMaxAngleLimit, "Max pitch limit", 0f, 90f);
							if (Options.PitchMaxAngleLimit < Options.PitchMinAngleLimit) { Options.PitchMinAngleLimit = Options.PitchMaxAngleLimit; }
							GUI.enabled = true;

							//Options.RollLimit = EditorGUILayout.Toggle("Roll limitation", Options.RollLimit);

							EditorGUI.indentLevel--;
						}
						GUILayout.Space(_foldoutSpace);
					}
				}
				#endregion

				#region Filters
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					_showFilters.target = EditorGUILayout.Foldout(_showFilters.target, "Advanced Motion Filtering", true, _foldoutStyle);

					const string freqTooltip = "Expected data frequency";
					const string minCOTooltip = "Minimum frequency cutoff. Lower values reduce jitter";
					const string betaTooltip = "Higher values reduce high speed lag";
					const string dCOTooltip = "Frequency cutoff for derivative";

					using (new EditorGUILayout.FadeGroupScope(_showFilters.faded))
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
						GUILayout.Space(_foldoutSpace);
					}
				}
				#endregion

				#region Inertia
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					_showInertia.target = EditorGUILayout.Foldout(_showInertia.target, "Inertia", true, _foldoutStyle);

					using (new EditorGUILayout.FadeGroupScope(_showInertia.faded))
					{
						if (_showInertia.value)
						{
							GUILayout.Space(4);
							EditorGUI.indentLevel++;
							Options.EnableInertia = EditorGUILayout.Toggle("Enable Inertia", Options.EnableInertia);
							Options.StopIfNotVisible = EditorGUILayout.Toggle("Stop if hands hidden", Options.StopIfNotVisible);
							Options.LinearDrag = CustomFloatField(Options.LinearDrag, "Linear Drag", 0f, 10f);
							Options.AngularDrag = CustomFloatField(Options.AngularDrag, "Angular Drag", 0f, 10f);
							// Options.VelocityFrames = CustomIntField(Options.VelocityFrames, "Velocity Frames", 2, 10);
							// Options.DiscardFrames = CustomIntField(Options.DiscardFrames, "Discard Frames", 1, 5);
							EditorGUI.indentLevel--;
						}
						GUILayout.Space(_foldoutSpace);
					}
				}
				#endregion

				#region Heuristics
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					_showHeuristic.target = EditorGUILayout.Foldout(_showHeuristic.target, "Gesture Filtering", true, _foldoutStyle);

					using (new EditorGUILayout.FadeGroupScope(_showHeuristic.faded))
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
						GUILayout.Space(_foldoutSpace);
					}
				}
				#endregion

				#region Visuals
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					_showDebug.target = EditorGUILayout.Foldout(_showDebug.target, "Visual Effects", true, _foldoutStyle);

					using (new EditorGUILayout.FadeGroupScope(_showDebug.faded))
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
						GUILayout.Space(_foldoutSpace);
					}
				}
				#endregion
			}

			GUILayout.EndScrollView();
			if (GUI.changed)
			{
				Options.Dirty = true;
			}

			GUI.enabled = Options.Dirty;
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			//buttonStyle.normal.textColor = Options.Dirty ? Color.red : GUI.skin.button.normal.textColor;
			buttonStyle.fontStyle = Options.Dirty ? FontStyle.Bold : FontStyle.Normal;

			if (GUILayout.Button("Save", buttonStyle))
			{
				Options.Save();
			}

			GUI.enabled = true;

		}

		[MenuItem("Window/Lemonity Options &l")]
		public static void Init()
		{
			GetWindow(typeof(EditorOptionsWindow), false, "Lemonity Options", true);
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
			_showFly.target = true;
			_logoTexture = Resources.Load<Texture>("logo");
		}

		private void InitStyles()
		{
			if (_foldoutStyle == null)
			{
				// Style definition
				_foldoutStyle = new GUIStyle(EditorStyles.foldout);
				_foldoutStyle.fontStyle = FontStyle.Bold;
				_foldoutStyle.fontSize = _foldoutFontSize;
			}
		}

		private static int CustomIntField(int value, string prefixLabel, int minSlider, int maxSlider, string tooltip = "")
		{
			var content = new GUIContent(prefixLabel, tooltip);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(content);
				value = (int)GUILayout.HorizontalSlider(value, minSlider, maxSlider);
				return EditorGUILayout.DelayedIntField(value, GUILayout.Width(_inputTextWidth));
			}
		}

		private static float CustomFloatField(float value, string prefixLabel, string tooltip = "")
		{
			var content = new GUIContent(prefixLabel, tooltip);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(content);
				return CustomFloatField(value);
			}
		}

		private static float CustomFloatField(float value, string prefixLabel, float minSlider, float maxSlider, string tooltip = "")
		{
			var content = new GUIContent(prefixLabel, tooltip);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(content);
				value = GUILayout.HorizontalSlider(value, minSlider, maxSlider);
				return CustomFloatField(value);
			}
		}

		private static float CustomFloatField(float value)
		{
			string floatString = value.ToString("0.00");

			var guistyle = EditorStyles.textField;
			guistyle.alignment = TextAnchor.MiddleRight;
			string input = EditorGUILayout.DelayedTextField(floatString, guistyle, GUILayout.Width(_inputTextWidth));

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

		private static void GestureToMode(WorkingGesture gesture, out Mode mode, out SubMode subMode)
		{
			mode = Mode.Off;
			subMode = SubMode.AnyHands;
			switch (gesture)
			{
				case WorkingGesture.Disabled:
					mode = Mode.Off;
					break;
				case WorkingGesture.OneHand:
					mode = Mode.Grab;
					subMode = SubMode.OneHand;
					break;
				case WorkingGesture.TwoHands:
					mode = Mode.Grab;
					subMode = SubMode.TwoHands;
					break;
				case WorkingGesture.Hybrid:
					mode = Mode.Grab;
					subMode = SubMode.AnyHands;
					break;
				case WorkingGesture.Orbit:
					mode = Mode.Orbit;
					break;
				case WorkingGesture.FlyOneHand:
					mode = Mode.Fly;
					subMode = SubMode.OneHand;
					break;
				case WorkingGesture.FlyTwoHands:
					mode = Mode.Fly;
					subMode = SubMode.TwoHands;
					break;
			}
		}

		private static WorkingGesture ModeToGesture(Mode mode, SubMode subMode)
		{
			switch (mode)
			{
				case Mode.Off:
					return WorkingGesture.Disabled;
				case Mode.Grab:
					switch (subMode)
					{
						case SubMode.OneHand:
							return WorkingGesture.OneHand;
						case SubMode.TwoHands:
							return WorkingGesture.TwoHands;
						case SubMode.AnyHands:
							return WorkingGesture.Hybrid;
					}
					break;
				case Mode.Orbit:
					return WorkingGesture.Orbit;
				case Mode.Fly:
					if (subMode == SubMode.OneHand)
					{
						return WorkingGesture.FlyOneHand;
					}
					else
					{
						return WorkingGesture.FlyTwoHands;
					}
			}
			return WorkingGesture.Disabled;
		}

		private void ScaleGUI()
		{

			EditorGUI.BeginChangeCheck();

			using (new GUILayout.HorizontalScope())
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
			string strWidth = xRange.ToString("0.#");

			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel("Scene Scale");
				EditorGUILayout.LabelField($"1 : {strScale}  ({strWidth} m.)", GUILayout.MaxWidth(130));
			}

			Options.AutoPosScaleOnLoad = EditorGUILayout.Toggle("Auto scale on load", Options.AutoPosScaleOnLoad);

			GUILayout.Space(4);
			Options.RotScale = CustomFloatField(Options.RotScale, "Rotation Factor", 0.5f, 5f);
			Options.ZoomScale = CustomFloatField(Options.ZoomScale, "Zoom Factor", 0f, 3f);
		}

		private void ScaleGUIOrbit()
		{
			EditorGUI.indentLevel++;
			// Use logaritmic scale starting from 0

			Options.OrbitYawScale = CustomFloatField(Options.OrbitYawScale, "Y Rotation Factor", 0.01f, 3f);
			Options.OrbitPitchScale = CustomFloatField(Options.OrbitPitchScale, "X Rotation Factor", 0.01f, 3f);
			Options.OrbitZoomScale = CustomFloatField(Options.OrbitZoomScale, "Zoom Factor", 0.01f, 3f);

			var content = new GUIContent("Exponential Zoom", "Zoom factor increases exponentially");
			Options.OrbitExponential = EditorGUILayout.Toggle(content, Options.OrbitExponential);

			EditorGUI.indentLevel--;
		}


		private void ScaleGUIFly()
		{
			EditorGUI.indentLevel++;
			// Use logaritmic scale starting from 0

			Options.FlyPosScale = CustomFloatField(Options.FlyPosScale, "Fly Speed", 0.01f, 10f);
			Options.FlyYawScale = CustomFloatField(Options.FlyYawScale, "Y Rotation Speed", 0.01f, 10f);
			Options.FlyPitchScale= CustomFloatField(Options.FlyPitchScale, "X Rotation Speed", 0.01f, 10f);
			Options.FlyExponential = CustomFloatField(Options.FlyExponential, "Exponential Factor", 1f, 5f);

			//var content = new GUIContent("Exponential Zoom", "Zoom factor increases exponentially");
			//Options.OrbitExponential = EditorGUILayout.Toggle(content, Options.OrbitExponential);

			EditorGUI.indentLevel--;
		}

		private void FlyModeOptions()
		{
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showFly.target = EditorGUILayout.Foldout(_showFly.target, "Fly", true, _foldoutStyle);

				using (new EditorGUILayout.FadeGroupScope(_showFly.faded))
				{
					if (_showFly.value)
					{
						GUILayout.Space(4);
						EditorGUI.indentLevel++;

						Options.FlyHover= EditorGUILayout.Toggle("Hover", Options.FlyHover);
						GUI.enabled = Options.FlyHover;
						Options.FlyHoverDistance = CustomFloatField(Options.FlyHoverDistance, "Ground Distance", 0.5f, 50f);
						GUI.enabled = true;

						GUILayout.Space(4);

						EditorGUI.indentLevel--;
					}
					GUILayout.Space(_foldoutSpace);
				}
			}
		}
	}
}
