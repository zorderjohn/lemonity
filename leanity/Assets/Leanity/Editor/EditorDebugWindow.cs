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

		private readonly Vector3[] _cubeVertices =
		{
			new Vector3( 1f,  1f,  1f), // 0
			new Vector3( 1f,  1f, -1f), // 1
			new Vector3( 1f, -1f,  1f), // 2
			new Vector3( 1f, -1f, -1f), // 3
			new Vector3(-1f,  1f,  1f), // 4
			new Vector3(-1f,  1f, -1f), // 5
			new Vector3(-1f, -1f,  1f), // 6
			new Vector3(-1f, -1f, -1f) // 7
		};

		void OnEnable()
		{
			LoadResources();
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			EditorController.EditorMotionController.OnHandsVisible += OnHandsVisible;
		}

		void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		}

		public void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			var rightHandData = HandTracking.RightHandData;
			var leftHandData = HandTracking.LeftHandData;

			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				Options.ShowGrid = EditorGUILayout.Toggle("Show Grid", Options.ShowGrid);
				using (var horizontalScope = new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Grid Lines");
					Options.NumGridLines = (int)GUILayout.HorizontalSlider(Options.NumGridLines, 1f, 20f);
					Options.NumGridLines = EditorGUILayout.IntField(Options.NumGridLines, GUILayout.Width(50));
				}

				Options.GestureDebug = EditorGUILayout.Toggle("Gesture Debug", Options.GestureDebug);
			}
			GUILayout.Space(4);
			using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				GUILayout.Label("Right Hand Data");
				DrawHandData(rightHandData);

				GUILayout.Label("Left Hand Data");
				DrawHandData(leftHandData);
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

			GUI.enabled = true;
		}

		private void OnHandsVisible()
		{
			SceneView.RepaintAll();
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
			var e = Event.current;
			if (e != null && e.type == EventType.MouseDown)
			{
				EditorController.EditorMotionController.StopInertia();
			}

			Handles.color = Color.yellow;

			if (sceneView == null)
			{
				return;
			}

			if (HandTracking.LeftHandData.Detected || HandTracking.RightHandData.Detected)
			{
				Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

				if (HandTracking.LeftHandData.Detected)
				{
					Handles.color = Color.red;
					PaintHand(HandTracking.LeftHandData);
				}
				if (HandTracking.RightHandData.Detected)
				{
					Handles.color = Color.blue;
					PaintHand(HandTracking.RightHandData);
				}

				if (Options.ShowGrid)
				{
					PaintWorkspace();
				}

				SceneView.RepaintAll();
			}
		}

		private void PaintHand(HandData hand)
		{
			var handPos = HandTracking.ToWorldCoordinates(hand.Position);
			var handRot = HandTracking.ToWorldCoordinates(hand.Rotation);
			var motionController = EditorController.EditorMotionController;

			bool holding = hand.IsRight ? motionController.RightGrab.IsHolding : motionController.LeftGrab.IsHolding;
			if ( holding )
			{
				Handles.ConeHandleCap(1, handPos, handRot * Quaternion.Euler(90, 0f, 0f), .3f, EventType.Repaint);
			}
			else
			{
				Handles.SphereHandleCap(1, handPos, handRot * Quaternion.Euler(90, 0f, 0f), .2f, EventType.Repaint);
			}

			float x = HandTracking.Workspace.x / 2f;
			Vector3 v0, v1;
			v0 = HandTracking.ToWorldCoordinates(new Vector3(hand.Position.x, hand.Position.y, hand.Position.z));

			v1 = HandTracking.ToWorldCoordinates(new Vector3(hand.IsRight ? x : -x, hand.Position.y, hand.Position.z));
			Handles.DrawWireDisc(v1, Vector3.right, HandleUtility.GetHandleSize(v1) * 0.1f);
			Handles.DrawDottedLine(v1, v0, 5);

			float z = HandTracking.Workspace.z / 2f;
			v1 = HandTracking.ToWorldCoordinates(new Vector3(hand.Position.x, hand.Position.y, z));
			Handles.DrawWireDisc(v1, Vector3.back, HandleUtility.GetHandleSize(v1) * 0.1f);
			Handles.DrawDottedLine(v1, v0, 5);

			float y = HandTracking.Workspace.y / 2f;
			v1 = HandTracking.ToWorldCoordinates(new Vector3(hand.Position.x, -y, hand.Position.z));
			Handles.DrawWireDisc(v1, Vector3.back, HandleUtility.GetHandleSize(v1) * 0.1f);
			Handles.DrawDottedLine(v1, v0, 5);
		}

		private void PaintWorkspace()
		{
			var motionController = EditorController.EditorMotionController;
			EditorController.Update();

			Handles.color = motionController.IsHolding ? Color.red : Color.white;

			// Top
			PaintGrid(0, 1, 5, 4);

			// Bottom
			PaintGrid(2, 3, 7, 6);

			// Front
			PaintGrid(0, 2, 6, 4);

			// Left
			PaintGrid(4, 5, 7, 6);

			// Right
			PaintGrid(0, 1, 3, 2);

			if (Options.GestureDebug)
			{
				motionController.MotionStyle.DebugDraw();
			}
		}

		// Clockwise vertices
		private void PaintGrid(uint i0, uint i1, uint i2, uint i3)
		{
			var v0 = GetCubeCoord(i0);
			var v1 = GetCubeCoord(i1);
			var v2 = GetCubeCoord(i2);
			var v3 = GetCubeCoord(i3);

			int div = Options.NumGridLines + 1;
			for (int i = 0; i <= div; i++)
			{
				float f = i / (float)div;
				var vert0 = Vector3.Lerp(v0, v1, f);
				var vert1 = Vector3.Lerp(v3, v2, f);
				Handles.DrawLine(vert0, vert1);

				vert0 = Vector3.Lerp(v0, v3, f);
				vert1 = Vector3.Lerp(v1, v2, f);
				Handles.DrawLine(vert0, vert1);
			}
		}

		private Vector3 GetCubeCoord(uint id)
		{
			if (id < 8)
			{
				var localPosition = Vector3.Scale(HandTracking.Workspace, _cubeVertices[id]) * 0.5f;
				return HandTracking.ToWorldCoordinates(localPosition);
			}
			return Vector3.zero;
		}

	}
}
