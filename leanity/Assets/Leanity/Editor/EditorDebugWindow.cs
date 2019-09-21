using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.IMGUI.Controls;

namespace Leanity
{
	[Serializable]
	public class EditorDebugWindow : EditorWindow, IDisposable
	{
		private static Vector2 _scrollPosition;
		private static Texture _rightHandTexture;
		private static Texture _leftHandTexture;
		private static Texture _rightHandGrabTexture;
		private static Texture _leftHandGrabTexture;
		private readonly float _workspaceWidth = 0.5f;
		private readonly float _workspaceDepth = 0.5f;
		private static bool _handsVisible = false;

		void OnEnable()
		{
			LoadResources();
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			EditorController.EditorMotionController.OnHandsVisible += OnHandsVisible;
			EditorController.EditorMotionController.OnHandsInVisible += OnHandsInVisible;
			Options.OnOptionsChange += OnOptionsChange;
		}

		void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		}

		private void OnOptionsChange()
		{
			SceneView.RepaintAll();
		}

		public void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			var rightHandData = HandTracking.RightHandData;
			var leftHandData = HandTracking.LeftHandData;

			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				Options.ShowGrid = EditorGUILayout.Toggle("Show Grid", Options.ShowGrid);
				Options.ShowWorkspace = EditorGUILayout.Toggle("Show Workspace", Options.ShowWorkspace);
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Grid Lines");
					Options.NumGridLines = (int)GUILayout.HorizontalSlider(Options.NumGridLines, 1f, 20f);
					Options.NumGridLines = EditorGUILayout.IntField(Options.NumGridLines, GUILayout.Width(50));
				}
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Grid Transparency");
					Options.MaxGridTransparency = GUILayout.HorizontalSlider(Options.MaxGridTransparency, 0f, 1f);
					Options.MaxGridTransparency = EditorGUILayout.FloatField(Options.MaxGridTransparency, GUILayout.Width(50));
				}
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Grid Z Offset");
					Options.TrackingZOffset= GUILayout.HorizontalSlider(Options.TrackingZOffset, 0f, 2f);
					Options.TrackingZOffset = EditorGUILayout.FloatField(Options.TrackingZOffset, GUILayout.Width(50));
				}
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Hand Scale");
					Options.HandScale= GUILayout.HorizontalSlider(Options.HandScale, 0f, 2f);
					Options.HandScale = EditorGUILayout.FloatField(Options.HandScale, GUILayout.Width(50));
				}

				Options.GestureDebug = EditorGUILayout.Toggle("Gesture Debug", Options.GestureDebug);
				Options.ShowHandGuides = EditorGUILayout.Toggle("Hand Guides", Options.ShowHandGuides);

			}
			GUILayout.Space(4);
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				GUILayout.Label("Status:", EditorStyles.boldLabel);
				if (HandTracking.IsConnected())
				{
					GUILayout.Label("Leap is connected");
				}
				else
				{
					GUILayout.Label("Leap is not connected");
					if (GUILayout.Button("Reset connection"))
					{
						HandTracking.Reset();
					}
				}
			}

			GUILayout.Space(4);
			using (var verticalScope = new GUILayout.VerticalScope())
			{
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					using (var verticalScope2 = new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						GUILayout.Label("Left Hand Data", EditorStyles.boldLabel);
						DrawHandData(leftHandData);
					}
					GUILayout.Space(2);
					using (var verticalScope2 = new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						GUILayout.Label("Right Hand Data", EditorStyles.boldLabel);
						DrawHandData(rightHandData);
					}
				}
			}
			GUILayout.Space(4);
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				Rect r = GUILayoutUtility.GetAspectRect(1f);

				EditorGUI.IndentedRect(r);

				EditorGUI.DrawRect(r, Color.white);
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
			Rect rProgressBar = GUILayoutUtility.GetRect(50, 100, 20, 20);
			EditorGUI.ProgressBar(rProgressBar, hand.Detected ? hand.GrabValue : 0f, "Grab");

			GUI.enabled = true;
		}

		private string CustomVectorString(Vector3 v)
		{
			return "(" + v.x.ToString("0.###") + ", " + v.y.ToString("0.###") + ", " + v.z.ToString("0.###");
		}

		private void OnHandsVisible()
		{
			SceneView.RepaintAll();
			Options.GridFadeIn();
		}
		private void OnHandsInVisible()
		{
			Options.GridFadeOut();
		}

		private void DrawHandPosition(HandData hand, Rect r)
		{
			if (hand.Detected)
			{
				Rect rImage = new Rect(r);
				Texture handTexture;
				GrabController grabController;
				var motionController = EditorController.EditorMotionController;
				if (hand.IsRight)
				{
					grabController = motionController.RightGrab;
					handTexture = grabController.IsHolding ? _rightHandGrabTexture : _rightHandTexture;
				}
				else
				{
					grabController = motionController.LeftGrab;
					handTexture = grabController.IsHolding ? _leftHandGrabTexture : _leftHandTexture;
				}
				rImage.width = handTexture.width;
				rImage.height = handTexture.height;

				var handRectCoords = GetRectCoords(hand.Position, r);
				var handInitialRectCoords = GetRectCoords(grabController.HandInitialPosition, r);

				rImage.x = (int)(handRectCoords.x - rImage.width / 2);
				rImage.y = (int)(handRectCoords.y - rImage.height / 2);

				if (grabController.IsHolding)
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
			coords.x += (worldCoords.x / _workspaceWidth) * r.width + r.width * 0.5f;
			coords.y += -(worldCoords.z / _workspaceDepth) * r.height + r.height * 0.5f;
			return coords;
		}

		[MenuItem("Window/Leanity &l")]
		public static void Init()
		{
			GetWindow(typeof(EditorDebugWindow), false, "Leanity Debug", true);
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
			if (_rightHandTexture == null)
			{
				_rightHandTexture = (Texture)EditorGUIUtility.Load("Assets/Leanity/Editor/Resources/RightHand.png");
				_leftHandTexture = (Texture)EditorGUIUtility.Load("Assets/Leanity/Editor/Resources/LeftHand.png");
				_rightHandGrabTexture = (Texture)EditorGUIUtility.Load("Assets/Leanity/Editor/Resources/RightHandGrab.png");
				_leftHandGrabTexture = (Texture)EditorGUIUtility.Load("Assets/Leanity/Editor/Resources/LeftHandGrab.png");
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

				if (leftDetected && rightDetected && Options.Gesture == WorkingGesture.TwoHands)
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

	}
}
