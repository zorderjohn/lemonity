using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Lemonity.Core;

namespace Lemonity.Editor
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
		private const string _openUpmUrl = "https://package.openupm.com";
		private const string _ultraleapTrackingPackage = "com.ultraleap.tracking@7.3.0";
		private const string _ultraleapProviderPackage = "com.lemonity.provider.ultraleap@2.0.0";
		private static GUIStyle _foldoutStyle;
		private static Queue<string> _installQueue;
		private static AddRequest _installRequest;

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
			if (!HandTracking.HasProvider)
			{
				DrawMissingProviderMessage();
				return;
			}

			Mode mode;
			SubMode subMode;
			GestureToMode(Options.Mode, out mode, out subMode);

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
					new GUIContent("One/Two Hands", "One or two hands can be used to rotate and move the camera"),
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

				Options.Mode = ModeToGesture(mode, subMode);

				GUIContent[] invertAxisContent = new[]
				{
					new GUIContent("Move world", "Normal operation, hand movement corresponds to movement of the world"),
					new GUIContent("Move camera", "Hand movement corresponds to movement of the camera (invert axis)")
				};

				GUILayout.Space(4);
				if (mode == Mode.Grab)
				{
					Options.GrabMode.InvertAxis = 1 == GUILayout.SelectionGrid(Options.GrabMode.InvertAxis ? 1 : 0, invertAxisContent, 2, EditorStyles.radioButton);
				}
				else if (mode == Mode.Orbit)
				{
					Options.OrbitMode.InvertAxis = 1 == GUILayout.SelectionGrid(Options.OrbitMode.InvertAxis ? 1 : 0, invertAxisContent, 2, EditorStyles.radioButton);
				}
				else if (mode == Mode.Fly)
				{
					Options.FlyMode.InvertAxis = 1 == GUILayout.SelectionGrid(Options.FlyMode.InvertAxis ? 1 : 0, invertAxisContent, 2, EditorStyles.radioButton);
				}
				GUILayout.Space(_foldoutSpace);
			}
			#endregion

			if (mode == Mode.Fly)
			{
				FlyModeOptions();
			}

			if (Options.Mode != WorkingMode.Disabled)
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

							if (Options.Mode == WorkingMode.Orbit)
							{
								ScaleGUIOrbit();
							}
							else if (Options.Mode == WorkingMode.FlyOneHand || Options.Mode == WorkingMode.FlyTwoHands)
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
					_showGestures.target = EditorGUILayout.Foldout(_showGestures.target, "Gesture Thresholds", true, _foldoutStyle);

					using (new EditorGUILayout.FadeGroupScope(_showGestures.faded))
					{
						if (_showGestures.value)
						{
							GUILayout.Space(4);
							GUILayout.Label("Grab gesture", EditorStyles.boldLabel);
							EditorGUI.indentLevel++;

							Options.Gestures.GrabMaxThreshold = CustomFloatField(Options.Gestures.GrabMaxThreshold, "Start Threshold", 0f, 1f);
							if (Options.Gestures.GrabMaxThreshold < Options.Gestures.GrabMinThreshold) { Options.Gestures.GrabMinThreshold = Options.Gestures.GrabMaxThreshold; }

							Options.Gestures.GrabMinThreshold = CustomFloatField(Options.Gestures.GrabMinThreshold, "Stop Threshold", 0f, 1f);
							if (Options.Gestures.GrabMinThreshold > Options.Gestures.GrabMaxThreshold) { Options.Gestures.GrabMaxThreshold = Options.Gestures.GrabMinThreshold; }
							EditorGUI.indentLevel--;

							GUILayout.Label("Pinch gesture", EditorStyles.boldLabel);

							GUILayout.Space(4);
							EditorGUI.indentLevel++;

							Options.Gestures.PinchMinThreshold = CustomFloatField(Options.Gestures.PinchMinThreshold, "Start Distance", 0f, 0.100f);
							if (Options.Gestures.PinchMinThreshold > Options.Gestures.PinchMaxThreshold) { Options.Gestures.PinchMaxThreshold = Options.Gestures.PinchMinThreshold; }

							Options.Gestures.PinchMaxThreshold = CustomFloatField(Options.Gestures.PinchMaxThreshold, "Stop Distance", 0f, 0.100f);
							if (Options.Gestures.PinchMaxThreshold < Options.Gestures.PinchMinThreshold) { Options.Gestures.PinchMinThreshold = Options.Gestures.PinchMaxThreshold; }

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

							Options.Camera.PitchLimit = EditorGUILayout.Toggle("Pitch limitation", Options.Camera.PitchLimit);
							GUI.enabled = Options.Camera.PitchLimit;
							Options.Camera.PitchMinAngleLimit = CustomFloatField(Options.Camera.PitchMinAngleLimit, "Min pitch limit", 0f, 90f);
							if (Options.Camera.PitchMinAngleLimit > Options.Camera.PitchMaxAngleLimit) { Options.Camera.PitchMaxAngleLimit = Options.Camera.PitchMinAngleLimit; }
							Options.Camera.PitchMaxAngleLimit = CustomFloatField(Options.Camera.PitchMaxAngleLimit, "Max pitch limit", 0f, 90f);
							if (Options.Camera.PitchMaxAngleLimit < Options.Camera.PitchMinAngleLimit) { Options.Camera.PitchMinAngleLimit = Options.Camera.PitchMaxAngleLimit; }
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
					_showFilters.target = EditorGUILayout.Foldout(_showFilters.target, "Tracking Filtering", true, _foldoutStyle);

					const string freqTooltip = "Expected data frequency";
					const string minCOTooltip = "Minimum frequency cutoff. Lower values reduce jitter";
					const string betaTooltip = "Higher values reduce high speed lag";
					const string dCOTooltip = "Frequency cutoff for derivative";

					using (new EditorGUILayout.FadeGroupScope(_showFilters.faded))
					{
						if (_showFilters.value)
						{
							GUILayout.Space(4);

							Options.Filter.Frequency = CustomFloatField(Options.Filter.Frequency, "Frequency", freqTooltip);
							GUILayout.Label("Rotation", EditorStyles.boldLabel);
							EditorGUI.indentLevel++;
							Options.Filter.RotationMinCutoff = CustomFloatField(Options.Filter.RotationMinCutoff, "Min CutOff", 0f, 5f, minCOTooltip);
							Options.Filter.RotationBeta = CustomFloatField(Options.Filter.RotationBeta, "Beta", 1f, 20f, betaTooltip);
							Options.Filter.RotationDerivativeCutoff = CustomFloatField(Options.Filter.RotationDerivativeCutoff, "Deriv Cutoff", 1f, 2f, dCOTooltip);
							EditorGUI.indentLevel--;

							GUILayout.Label("Position", EditorStyles.boldLabel);
							EditorGUI.indentLevel++;
							Options.Filter.PositionMinCutoff = CustomFloatField(Options.Filter.PositionMinCutoff, "Min CutOff", 0f, 5f, minCOTooltip);
							Options.Filter.PositionBeta = CustomFloatField(Options.Filter.PositionBeta, "Beta", 1f, 20f, betaTooltip);
							Options.Filter.PositionDerivativeCutoff = CustomFloatField(Options.Filter.PositionDerivativeCutoff, "Deriv Cutoff", 1f, 2f, dCOTooltip);
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
							Options.Inertia.EnableInertia = EditorGUILayout.Toggle("Enable Inertia", Options.Inertia.EnableInertia);
							Options.Inertia.StopIfNotVisible = EditorGUILayout.Toggle("Stop if hands hidden", Options.Inertia.StopIfNotVisible);
							Options.Inertia.LinearDrag = CustomFloatField(Options.Inertia.LinearDrag, "Linear Drag", 0f, 10f);
							Options.Inertia.AngularDrag = CustomFloatField(Options.Inertia.AngularDrag, "Angular Drag", 0f, 10f);
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
					_showHeuristic.target = EditorGUILayout.Foldout(_showHeuristic.target, "Involuntary Gestures", true, _foldoutStyle);

					using (new EditorGUILayout.FadeGroupScope(_showHeuristic.faded))
					{
						if (_showHeuristic.value)
						{
							GUILayout.Space(4);
							EditorGUI.indentLevel++;

							Options.Heuristic.Enabled = EditorGUILayout.Toggle("Enable heuristic", Options.Heuristic.Enabled);
							Options.Heuristic.Radius = CustomFloatField(Options.Heuristic.Radius, "Safe zone radius", 0.1f, 1f);

							GUILayout.Space(4);
							EditorGUILayout.LabelField("Gestures that start out of the safe zone in outward direction are considered involuntary and ignored.\n" +
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
					_showDebug.target = EditorGUILayout.Foldout(_showDebug.target, "Appearance", true, _foldoutStyle);

					using (new EditorGUILayout.FadeGroupScope(_showDebug.faded))
					{
						if (_showDebug.value)
						{
							GUILayout.Space(4);
							EditorGUI.indentLevel++;

							EditorGUI.BeginChangeCheck();

							EditorOptions.Visuals.ShowWorkspace = EditorGUILayout.Toggle("Show Workspace", EditorOptions.Visuals.ShowWorkspace);
							EditorOptions.Visuals.ShowGrid = EditorGUILayout.Toggle("Show Grid", EditorOptions.Visuals.ShowGrid);

							GUI.enabled = EditorOptions.Visuals.ShowGrid;
							EditorOptions.Visuals.NumGridLines = CustomIntField(EditorOptions.Visuals.NumGridLines, "Grid Divisions", 0, 20);

							GUI.enabled = EditorOptions.Visuals.ShowGrid | EditorOptions.Visuals.ShowWorkspace;
							EditorOptions.Visuals.MaxGridTransparency = CustomFloatField(EditorOptions.Visuals.MaxGridTransparency, "Transparency", 0f, 1f);
							Options.TrackingSpace.TrackingZOffset = CustomFloatField(Options.TrackingSpace.TrackingZOffset, "Grid Z Offset", 0f, 2f);

							GUI.enabled = true;
							EditorOptions.Visuals.HandScale = CustomFloatField(EditorOptions.Visuals.HandScale, "Hand Scale", 0f, 2f);

							if (EditorGUI.EndChangeCheck())
							{
								EditorController.EditorWorkspaceController.GridFadeInEditor();
							}

							EditorOptions.Visuals.GestureDebug = EditorGUILayout.Toggle("Gesture Debug", EditorOptions.Visuals.GestureDebug);
							EditorOptions.Visuals.ShowHandGuides = EditorGUILayout.Toggle("Hand Guides", EditorOptions.Visuals.ShowHandGuides);

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

			GUI.enabled = Options.Dirty || EditorOptions.Dirty;
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			//buttonStyle.normal.textColor = Options.Dirty ? Color.red : GUI.skin.button.normal.textColor;
			buttonStyle.fontStyle = Options.Dirty || EditorOptions.Dirty ? FontStyle.Bold : FontStyle.Normal;

			if (GUILayout.Button("Save", buttonStyle))
			{
				Options.Save();
				EditorOptions.Save();
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
			EditorOptions.Load();
			_showSensitivity.target = true;
			_showFly.target = true;
			_logoTexture = Resources.Load<Texture>("logo");
		}

		private static void DrawMissingProviderMessage()
		{
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_logoTexture = _logoTexture ?? Resources.Load<Texture>("logo");
				Rect rImage = GUILayoutUtility.GetRect(50, 1000, 50, 50);
				EditorGUI.DrawRect(rImage, _logoBackground);
				GUI.DrawTexture(rImage, _logoTexture, ScaleMode.ScaleToFit);

				GUILayout.Space(8);
				GUILayout.Label("Lemonity detected that Ultraleap Tracking is not installed.", EditorStyles.wordWrappedLabel);
				GUILayout.Space(4);

				GUI.enabled = _installQueue == null && (_installRequest == null || _installRequest.IsCompleted);
				if (GUILayout.Button("Install Ultraleap support"))
				{
					InstallUltraleapSupport();
				}
				GUI.enabled = true;

				if (_installRequest != null && !_installRequest.IsCompleted)
				{
					GUILayout.Space(4);
					GUILayout.Label("Installing Ultraleap support...", EditorStyles.miniLabel);
				}
			}
		}

		private static void InstallUltraleapSupport()
		{
			try
			{
				EnsureOpenUpmScopedRegistry();
			}
			catch (Exception ex)
			{
				Debug.LogError("[Lemonity] Could not add the OpenUPM scoped registry. " + ex.Message);
				return;
			}

			_installQueue = new Queue<string>();
			_installQueue.Enqueue(_ultraleapTrackingPackage);
			_installQueue.Enqueue(_ultraleapProviderPackage);
			EditorApplication.update -= ProcessInstallQueue;
			EditorApplication.update += ProcessInstallQueue;
			ProcessInstallQueue();
		}

		private static void ProcessInstallQueue()
		{
			if (_installRequest != null && !_installRequest.IsCompleted)
			{
				return;
			}

			if (_installRequest != null && _installRequest.Status == StatusCode.Failure)
			{
				Debug.LogError("[Lemonity] Package installation failed: " + _installRequest.Error.message);
				_installRequest = null;
				_installQueue = null;
				EditorApplication.update -= ProcessInstallQueue;
				return;
			}

			if (_installQueue == null || _installQueue.Count == 0)
			{
				_installRequest = null;
				_installQueue = null;
				EditorApplication.update -= ProcessInstallQueue;
				Debug.Log("[Lemonity] Ultraleap support installed.");
				return;
			}

			_installRequest = Client.Add(_installQueue.Dequeue());
		}

		private static void EnsureOpenUpmScopedRegistry()
		{
			string manifestPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Packages/manifest.json"));
			string manifest = File.ReadAllText(manifestPath);
			string updatedManifest = AddOrUpdateOpenUpmRegistry(manifest);

			if (updatedManifest != manifest)
			{
				File.WriteAllText(manifestPath, updatedManifest);
				AssetDatabase.Refresh();
			}
		}

		private static string AddOrUpdateOpenUpmRegistry(string manifest)
		{
			int urlIndex = manifest.IndexOf("\"" + _openUpmUrl + "\"", StringComparison.Ordinal);
			if (urlIndex >= 0)
			{
				int registryStart = manifest.LastIndexOf('{', urlIndex);
				if (registryStart >= 0)
				{
					int registryEnd = FindMatchingChar(manifest, registryStart, '{', '}');
					if (registryEnd > registryStart)
					{
						string registry = manifest.Substring(registryStart, registryEnd - registryStart + 1);
						registry = Regex.Replace(registry, "\\\"name\\\"\\s*:\\s*\\\"[^\\\"]*\\\"", "\"name\": \"OpenUPM\"", RegexOptions.Multiline);
						registry = EnsureRegistryScope(registry, "com.ultraleap");
						registry = EnsureRegistryScope(registry, "com.lemonity");
						return manifest.Substring(0, registryStart) + registry + manifest.Substring(registryEnd + 1);
					}
				}
			}

			string registryBlock =
				"    {\n" +
				"      \"name\": \"OpenUPM\",\n" +
				"      \"url\": \"" + _openUpmUrl + "\",\n" +
				"      \"scopes\": [\n" +
				"        \"com.ultraleap\",\n" +
				"        \"com.lemonity\"\n" +
				"      ]\n" +
				"    }";

			Match scopedRegistries = Regex.Match(manifest, "\\\"scopedRegistries\\\"\\s*:\\s*\\[");
			if (scopedRegistries.Success)
			{
				int arrayStart = manifest.IndexOf('[', scopedRegistries.Index);
				int arrayEnd = FindMatchingChar(manifest, arrayStart, '[', ']');
				if (arrayEnd <= arrayStart)
				{
					throw new InvalidDataException("Could not parse scopedRegistries in Packages/manifest.json.");
				}

				string arrayContent = manifest.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
				string separator = string.IsNullOrWhiteSpace(arrayContent) ? "\n" : "\n" + registryBlock + ",";
				string replacement = string.IsNullOrWhiteSpace(arrayContent) ? registryBlock + "\n  " : separator;
				return manifest.Substring(0, arrayStart + 1) + replacement + manifest.Substring(arrayStart + 1);
			}

			int rootEnd = manifest.LastIndexOf('}');
			string suffix = manifest.Substring(rootEnd);
			string prefix = manifest.Substring(0, rootEnd).TrimEnd();
			return prefix + ",\n  \"scopedRegistries\": [\n" + registryBlock + "\n  ]\n" + suffix;
		}

		private static string EnsureRegistryScope(string registry, string scope)
		{
			if (registry.Contains("\"" + scope + "\""))
			{
				return registry;
			}

			Match scopes = Regex.Match(registry, "\\\"scopes\\\"\\s*:\\s*\\[");
			if (!scopes.Success)
			{
				int objectEnd = registry.LastIndexOf('}');
				return registry.Substring(0, objectEnd).TrimEnd() + ",\n      \"scopes\": [\n        \"" + scope + "\"\n      ]\n" + registry.Substring(objectEnd);
			}

			int arrayStart = registry.IndexOf('[', scopes.Index);
			int arrayEnd = FindMatchingChar(registry, arrayStart, '[', ']');
			if (arrayEnd <= arrayStart)
			{
				throw new InvalidDataException("Could not parse the OpenUPM scoped registry scopes.");
			}

			string arrayContent = registry.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
			string insertion = string.IsNullOrWhiteSpace(arrayContent) ? "\n        \"" + scope + "\"\n      " : ",\n        \"" + scope + "\"\n      ";
			return registry.Substring(0, arrayEnd) + insertion + registry.Substring(arrayEnd);
		}

		private static int FindMatchingChar(string text, int start, char open, char close)
		{
			if (start < 0)
			{
				return -1;
			}

			int depth = 0;
			for (int i = start; i < text.Length; i++)
			{
				if (text[i] == open)
				{
					depth++;
				}
				else if (text[i] == close)
				{
					depth--;
					if (depth == 0)
					{
						return i;
					}
				}
			}

			return -1;
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

		private static void GestureToMode(WorkingMode gesture, out Mode mode, out SubMode subMode)
		{
			mode = Mode.Off;
			subMode = SubMode.AnyHands;
			switch (gesture)
			{
				case WorkingMode.Disabled:
					mode = Mode.Off;
					break;
				case WorkingMode.GrabOneHand:
					mode = Mode.Grab;
					subMode = SubMode.OneHand;
					break;
				case WorkingMode.GrabTwoHands:
					mode = Mode.Grab;
					subMode = SubMode.TwoHands;
					break;
				case WorkingMode.GrabHybrid:
					mode = Mode.Grab;
					subMode = SubMode.AnyHands;
					break;
				case WorkingMode.Orbit:
					mode = Mode.Orbit;
					break;
				case WorkingMode.FlyOneHand:
					mode = Mode.Fly;
					subMode = SubMode.OneHand;
					break;
				case WorkingMode.FlyTwoHands:
					mode = Mode.Fly;
					subMode = SubMode.TwoHands;
					break;
			}
		}

		private static WorkingMode ModeToGesture(Mode mode, SubMode subMode)
		{
			switch (mode)
			{
				case Mode.Off:
					return WorkingMode.Disabled;
				case Mode.Grab:
					switch (subMode)
					{
						case SubMode.OneHand:
							return WorkingMode.GrabOneHand;
						case SubMode.TwoHands:
							return WorkingMode.GrabTwoHands;
						case SubMode.AnyHands:
							return WorkingMode.GrabHybrid;
					}
					break;
				case Mode.Orbit:
					return WorkingMode.Orbit;
				case Mode.Fly:
					if (subMode == SubMode.OneHand)
					{
						return WorkingMode.FlyOneHand;
					}
					else
					{
						return WorkingMode.FlyTwoHands;
					}
			}
			return WorkingMode.Disabled;
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
			float sliderLogValue = MathHelper.LinearToLogScale(Options.TrackingSpace.PosScale);
			sliderLogValue = GUILayout.HorizontalSlider(sliderLogValue, .1f, 10f);
			if (EditorGUI.EndChangeCheck())
			{
				Options.TrackingSpace.PosScale = MathHelper.LogToLinearScale(sliderLogValue);
				EditorController.EditorWorkspaceController.GridFadeInEditor();
			}

			float xRange = HandTracking.Workspace.x * Options.TrackingSpace.PosScale;
			string strScale = Options.TrackingSpace.PosScale.ToString("0.0");
			string strWidth = xRange.ToString("0.#");

			GUILayout.Space(6);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel("Scene Scale");
				EditorGUILayout.LabelField("1 : " + strScale + "  (" + strWidth + " m.)", GUILayout.MaxWidth(130));
			}
			Options.TrackingSpace.AutoPosScaleOnLoad = EditorGUILayout.Toggle("Auto scale on load", Options.TrackingSpace.AutoPosScaleOnLoad);

			GUILayout.Space(4);
			Options.GrabMode.RotScale = CustomFloatField(Options.GrabMode.RotScale, "Rotation Factor", 0.5f, 5f);
			Options.GrabMode.ZoomScale = CustomFloatField(Options.GrabMode.ZoomScale, "Zoom Factor", 0f, 3f);
		}

		private void ScaleGUIOrbit()
		{
			EditorGUI.indentLevel++;
			// Use logaritmic scale starting from 0

			Options.OrbitMode.YawScale = CustomFloatField(Options.OrbitMode.YawScale, "Y Rotation Factor", 0.01f, 3f);
			Options.OrbitMode.PitchScale = CustomFloatField(Options.OrbitMode.PitchScale, "X Rotation Factor", 0.01f, 3f);
			Options.OrbitMode.ZoomScale = CustomFloatField(Options.OrbitMode.ZoomScale, "Zoom Factor", 0.01f, 3f);

			var content = new GUIContent("Exponential Zoom", "Zoom factor increases exponentially");
			Options.OrbitMode.ExponentialZoom = EditorGUILayout.Toggle(content, Options.OrbitMode.ExponentialZoom);

			EditorGUI.indentLevel--;
		}


		private void ScaleGUIFly()
		{
			EditorGUI.indentLevel++;
			// Use logaritmic scale starting from 0

			Options.FlyMode.PosScale = CustomFloatField(Options.FlyMode.PosScale, "Fly Speed", 0.01f, 10f);
			Options.FlyMode.YawScale = CustomFloatField(Options.FlyMode.YawScale, "Y Rotation Speed", 0.01f, 10f);
			Options.FlyMode.PitchScale = CustomFloatField(Options.FlyMode.PitchScale, "X Rotation Speed", 0.01f, 10f);
			Options.FlyMode.ExponentialFactor = CustomFloatField(Options.FlyMode.ExponentialFactor, "Exponential Factor", 1f, 5f);

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

						Options.FlyMode.Hover = EditorGUILayout.Toggle("Hover", Options.FlyMode.Hover);
						GUI.enabled = Options.FlyMode.Hover;
						Options.FlyMode.HoverDistance = CustomFloatField(Options.FlyMode.HoverDistance, "Ground Distance", 0.5f, 50f);
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
