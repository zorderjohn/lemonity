using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace Lemonity
{
	[Serializable]
	public class EditorDebugWindow : EditorWindow, IDisposable
	{
		private static Vector2 _scrollPosition;

		static Dictionary<MotionController.State, Texture> _leftTextures;
		static Dictionary<MotionController.State, Texture> _rightTextures;

		private static float _workspaceWidth = 0.5f;
		private static float _workspaceDepth = 0.5f;
		private static float _workspaceRatio = 1.0f;
		private static bool _handsVisible = false;

		void OnEnable()
		{
			LoadResources();
			Options.Load();
			_workspaceDepth = HandTracking.Workspace.z;
			_workspaceWidth = HandTracking.Workspace.x;
			_workspaceRatio = _workspaceWidth / _workspaceDepth;

			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			EditorController.EditorMotionController.OnHandsVisible += RepaintScene;
			EditorController.EditorMotionController.OnHandsInVisible += RepaintScene;
			EditorController.EditorMotionController.OnStateChange += RepaintScene;
			Options.OnOptionsChange += RepaintScene;
			HandTracking.OnConnect += RepaintScene;
			HandTracking.OnDisconnect += RepaintScene;
		}

		void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			EditorController.EditorMotionController.OnHandsVisible -= RepaintScene;
			EditorController.EditorMotionController.OnHandsInVisible -= RepaintScene;
			EditorController.EditorMotionController.OnStateChange -= RepaintScene;
			Options.OnOptionsChange -= RepaintScene;
			HandTracking.OnConnect -= RepaintScene;
			HandTracking.OnDisconnect -= RepaintScene;
		}

		private void RepaintScene()
		{
			Repaint();
			SceneView.RepaintAll();
		}

		public void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			var rightHandData = HandTracking.RightHandData;
			var leftHandData = HandTracking.LeftHandData;

			var labelStyle = EditorStyles.label;
			labelStyle.richText = true;

			GUILayout.Space(4);
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				GUILayout.Label("<b>Tracking Status</b>", labelStyle);
				if (HandTracking.IsConnected())
				{

					GUILayout.Label("Leap is <b><color=green>connected</color></b>", labelStyle);
				}
				else
				{
					GUI.contentColor = Color.red;
					GUILayout.Label("Leap is <color=red><b>NOT</b></color> connected", labelStyle);
					if (GUILayout.Button("Reset connection"))
					{
						HandTracking.Reset();
					}
				}
			}

			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				GUILayout.Label("Gesture Filtering", EditorStyles.boldLabel);
				var motion = EditorController.EditorMotionController;
				if (!Options.HeuristicEnabled)
				{
					GUILayout.Label("Disabled", labelStyle);
				}
				else
				{
					bool lg = motion.LeftGrab.Heuristic != HeuristicState.AllowAll;
					bool rg = motion.RightGrab.Heuristic != HeuristicState.AllowAll;
					bool lp = motion.LeftPinch.Heuristic != HeuristicState.AllowAll;
					bool rp = motion.RightPinch.Heuristic != HeuristicState.AllowAll;

					string filterNames = (lg ? "Left Grab " : "") + (rg ? "Right Grab " : "") + (lp ? "Left Pinch " : "") + (rp ? "Right Pinch" : "");
					if (lg || rg || lp || rp)
					{
						GUILayout.Label($"<b><Color=red> Filtering {filterNames}</Color></b>", labelStyle);
					}
					else if (motion.IsPinching || motion.IsGrabbing)
					{
						GUILayout.Label("Gesture not filtered", labelStyle);
					}
					else
					{
						GUILayout.Label("Iddle", labelStyle);
					}
				}
			}

			using (var verticalScope = new GUILayout.VerticalScope())
			{
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					using (var verticalScope2 = new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						GUILayout.Label("Left Hand Data", EditorStyles.boldLabel);
						DrawHandData(leftHandData);
					}
					using (var verticalScope2 = new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						GUILayout.Label("Right Hand Data", EditorStyles.boldLabel);
						DrawHandData(rightHandData);
					}
				}
			}
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				Rect r = GUILayoutUtility.GetAspectRect(_workspaceRatio);

				EditorGUI.IndentedRect(r);

				var color = Color.white;
				color.a = 0.5f;
				EditorGUI.DrawRect(r, color);

				if (Options.HeuristicEnabled)
				{
					//Draw heuristic safe zone
					Handles.color = Color.grey;
					Handles.DrawPolyLine(GenerateHeuristicCircle(r));
				}

				DrawHandPosition(rightHandData, r);
				DrawHandPosition(leftHandData, r);
			}

			GUILayout.EndScrollView();

			if (GUI.changed)
			{
				Options.Dirty = true;
			}
		}


		private void DrawHandData(HandData hand)
		{
			GUI.enabled = hand.Detected;

			GUILayout.BeginHorizontal();

			GUILayout.Label(hand.Detected ? CustomVectorString(hand.Position) : "(--, --, --)");
			GUILayout.EndHorizontal();

			GUILayout.Label("Pinch distance: " + (hand.Detected ? (hand.PinchDistance).ToString() : "--"));

			Rect rProgressBar = GUILayoutUtility.GetRect(50, 1000, 15, 15);

			float grabValue = hand.Detected ? hand.GrabValue : 0f;
			EditorGUI.ProgressBar(rProgressBar, grabValue, $"Grab: {grabValue.ToString("0.00")}");

			GUI.enabled = true;
		}

		private string CustomVectorString(Vector3 v)
		{
			return "(" + v.x.ToString("0.###") + ", " + v.y.ToString("0.###") + ", " + v.z.ToString("0.###");
		}


		private void DrawHandPosition(HandData hand, Rect r)
		{
			if (hand.Detected)
			{
				Rect rImage = new Rect(r);
				Texture handTexture;
				var motionController = EditorController.EditorMotionController;
				var handState = motionController.GetHandState(hand.IsRight);

				IGestureController gestureController = motionController.GetCurrentGestureController(hand.IsRight);

				handTexture = hand.IsRight ? _rightTextures[handState] : _leftTextures[handState];

				rImage.width = handTexture.width;
				rImage.height = handTexture.height;

				var handRectCoords = GetRectCoords(hand.Position, r);
				var handInitialRectCoords = GetRectCoords(gestureController.HandInitialPosition, r);

				rImage.x = (int)(handRectCoords.x - rImage.width / 2);
				rImage.y = (int)(handRectCoords.y - rImage.height / 2);

				if (gestureController.IsHolding)
				{
					Handles.color = Color.red;
					Handles.DrawLine(handInitialRectCoords, handRectCoords);
				}

				//rotatearoundpivot
				GUI.DrawTexture(rImage, handTexture, ScaleMode.ScaleToFit);
			}
		}

		private Vector2 GetRectCoords(Vector3 worldCoords, Rect r)
		{
			Vector2 coords = new Vector2(r.x, r.y);
			coords.x +=  Mathf.Clamp(worldCoords.x / _workspaceWidth, -.5f, .5f) * r.width + r.width * 0.5f;
			coords.y += -Mathf.Clamp(worldCoords.z / _workspaceDepth, -.5f, .5f) * r.height + r.height * 0.5f;
			return coords;
		}

		[MenuItem("Window/Lemonity Debug &l")]
		public static void Init()
		{
			GetWindow(typeof(EditorDebugWindow), false, "Lemonity Debug", true);
		}

		public void OnInspectorUpdate()
		{
			// This will only get called 10 times per second.
			if (_handsVisible)
			{
				Repaint();
			}
			_handsVisible = HandTracking.LeftHandData.Detected || HandTracking.RightHandData.Detected;
		}

		public void Dispose()
		{

		}

		private static void LoadResources()
		{
			if (_leftTextures == null)
			{
				_leftTextures = new Dictionary<MotionController.State, Texture>()
				{
					{MotionController.State.Hided, null },
					{MotionController.State.Idle, Resources.Load<Texture>("LeftHand")},
					{MotionController.State.Grabbing, Resources.Load<Texture>("LeftHandClosed")},
					{MotionController.State.Pinching, Resources.Load<Texture>("LeftHandPinch")}
				};

				_rightTextures = new Dictionary<MotionController.State, Texture>()
				{
					{MotionController.State.Hided, null },
					{MotionController.State.Idle, Resources.Load<Texture>("RightHand")},
					{MotionController.State.Grabbing, Resources.Load<Texture>("RightHandClosed")},
					{MotionController.State.Pinching, Resources.Load<Texture>("RightHandPinch")}
				};
			}
		}
		public void Awake()
		{
			LoadResources();
		}

		private void OnSceneGUI(SceneView sceneView)
		{
			if (sceneView == null)
			{
				return;
			}

			if (Options.ShowHandGuides)
			{
				var leftDetected = HandTracking.LeftHandData.Detected;
				var rightDetected = HandTracking.RightHandData.Detected;

				Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

				if (leftDetected)
				{
					Handles.color = Color.red;
					PaintHandGuides(HandTracking.LeftHandData);
				}

				if (rightDetected)
				{
					Handles.color = Color.blue;
					PaintHandGuides(HandTracking.RightHandData);
				}

				if (leftDetected && rightDetected && (Options.Gesture == WorkingGesture.TwoHands || Options.Gesture == WorkingGesture.Hybrid))
				{
					PaintPivot();
				}
			}

		}

		private void PaintHandGuides(HandData hand)
		{
			var cameraRot = HandTracking.ToWorldCoordinates(Quaternion.identity);

			float x = HandTracking.Workspace.x / 2f;
			Vector3 v0, v1;
			v0 = HandTracking.ToWorldCoordinates(new Vector3(hand.Position.x, hand.Position.y, hand.Position.z));

			// Dotted line to the right/left
			v1 = HandTracking.ToWorldCoordinates(new Vector3(hand.IsRight ? x : -x, hand.Position.y, hand.Position.z));
			Handles.DrawWireDisc(v1, cameraRot * Vector3.right, Options.PosScale * 0.005f);
			Handles.DrawDottedLine(v1, v0, 5);

			// Dotted line to the back
			float z = HandTracking.Workspace.z / 2f;
			v1 = HandTracking.ToWorldCoordinates(new Vector3(hand.Position.x, hand.Position.y, z));
			Handles.DrawWireDisc(v1, cameraRot * Vector3.back, Options.PosScale * 0.005f);
			Handles.DrawDottedLine(v1, v0, 5);

			// Dotted line to the bottom
			float y = HandTracking.Workspace.y / 2f;
			v1 = HandTracking.ToWorldCoordinates(new Vector3(hand.Position.x, -y, hand.Position.z));
			Handles.DrawWireDisc(v1, cameraRot * Vector3.down, Options.PosScale * 0.005f);
			Handles.DrawDottedLine(v1, v0, 5);

			// Dotted line to the top
			v1 = HandTracking.ToWorldCoordinates(new Vector3(hand.Position.x, y, hand.Position.z));
			Handles.DrawWireDisc(v1, cameraRot * Vector3.up, Options.PosScale * 0.005f);
			Handles.DrawDottedLine(v1, v0, 5);
		}

		private void PaintPivot()
		{
			var leftHand = HandTracking.LeftHandData;
			var rightHand = HandTracking.RightHandData;

			var leftPos = HandTracking.ToWorldCoordinates(leftHand.Position);
			var rightPos = HandTracking.ToWorldCoordinates(rightHand.Position);
			var centerPos = (leftPos + rightPos) * 0.5f;
			var motionController = EditorController.EditorMotionController;

			bool isGrabbing = motionController.IsGrabbing;
			bool isPinching = motionController.IsPinching;
			bool isIddle = !isGrabbing && !isPinching;

			var handleColor = Color.magenta;
			Handles.color = handleColor;

			Handles.DrawLine(leftPos, rightPos);
			if (!isPinching)
			{
				handleColor.a = isIddle ? 0.3f : 0.8f;
				Handles.color = handleColor;
				Handles.ConeHandleCap(1, centerPos, Quaternion.Euler(90f, 0f, 0f), Options.HandScale * Options.PosScale * .03f, EventType.Repaint);
			}
		}

		private Vector3[] GenerateHeuristicCircle(Rect r)
		{
			int numPoints = 37;
			float radius = Options.HeuristicRadius;
			var heuristicCircle = new Vector3[numPoints];

			float angleIncrement = 2 * Mathf.PI / (numPoints - 1);
			for (int i = 0; i < numPoints; i++)
			{
				float px = radius * Mathf.Cos(i * angleIncrement);
				float py = radius * Mathf.Sin(i * angleIncrement);
				Vector3 localVector = new Vector3(px, 0f, py);

				heuristicCircle[i] = GetRectCoords(localVector, r);
			}

			return heuristicCircle;
		}

	}
}
