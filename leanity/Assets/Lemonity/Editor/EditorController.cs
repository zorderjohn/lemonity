using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace Lemonity
{
	[InitializeOnLoad]
	public class EditorController
	{
		private static UnityEngine.SceneManagement.Scene _scene;

		public static MotionController EditorMotionController { get; private set; }
		public static WorkspaceController EditorWorkspaceController { get; private set; }
		private static Mesh _mesh;
		private static float _lastUpdate = 0f;


		static EditorController()
		{
			EditorMotionController = new MotionController(Options.Mode);
			var runtime = EditorMotionController.MotionRuntime;
			runtime.SelectionCenter = GetSelectionCenter;

			EditorWorkspaceController = new WorkspaceController(HandTracking.Workspace, EditorMotionController);
			EditorApplication.update += EditorUpdate;
			#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += OnSceneGUI;
			#else
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			#endif
			EditorMotionController.OnHandsVisible += OnHandsVisible;
			EditorMotionController.OnHandsInVisible += OnHandsInvisible;
			EditorMotionController.OnStateChange += OnStateChange;
		}

		static private void OnSceneGUI(SceneView sceneView)
		{
			Update(true);
		}

		static public void EditorUpdate()
		{
			Options.OnOptionsChange += OnOptionsChange;
			Options.Load();
			OnOptionsChange();

			if (Time.realtimeSinceStartup - _lastUpdate > 0.5f ||
				Options.EnableInertia && EditorMotionController.GrabMotion.HasInertia)
			{
				Update(false);
			}
		}

		static public void Update(bool GUIUpdate)
		{
			_lastUpdate = Time.realtimeSinceStartup;

			if (Event.current != null && Event.current.type == EventType.MouseDown)
			{
				EditorMotionController.StopInertia();
			}

			if (Options.Mode == WorkingMode.Disabled)
			{
				return;
			}


			var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			if (activeScene != _scene)
			{
				_scene = activeScene;
				OnSceneChange();
			}

			// Calculate cam position and rotation
			var sceneView = SceneView.lastActiveSceneView;

			bool editorActive = UnityEditorInternal.InternalEditorUtility.isApplicationActive &&
				(!MotionController.MultipleInstances || !EditorApplication.isPlaying || SceneView.focusedWindow == sceneView);


			if (sceneView != null && editorActive)
			{
#if UNITY_2018_2_OR_NEWER
				var camRot = sceneView.rotation.normalized;
#else
				var camRot = sceneView.rotation.GetNormalized();
#endif
				var camPos = MathHelper.CameraPosition(sceneView.pivot, sceneView.rotation, sceneView.cameraDistance);

				// As we are not drawing on PostRender we need to draw workspace using data from previous update
				// before changing camera
				DrawWorkspace(camPos + camRot * HandTracking.CamToHandOffset(), camRot);

				if (EditorMotionController.Update(camPos, camRot, Options.PosScale))
				{
					camPos = EditorMotionController.Position;
					camRot = EditorMotionController.Rotation;
					Options.PosScale = EditorMotionController.Scale;

					sceneView.rotation = camRot;
					sceneView.size = 1f + (Options.PosScale - 1f) * 0.1f;
					sceneView.pivot = MathHelper.CameraPivot(camPos, camRot, sceneView.cameraDistance);
				}

				bool anyHandVisible = HandTracking.LeftHandData.Detected || HandTracking.RightHandData.Detected;
				if (anyHandVisible || EditorWorkspaceController.GridVisible)
				{
					if (GUIUpdate && Options.GestureDebug)
					{
						EditorMotionController.GrabMotion.DebugDraw();
					}

					SceneView.RepaintAll();
				}

			}
		}

		private static void DrawWorkspace(Vector3 camPos, Quaternion camRot)
		{
			var position = camPos;
			var scale = Options.AxisRotScale * Options.PosScale;
			var rotation = camRot;
			EditorWorkspaceController.Draw(position, rotation, scale);
		}

		private static void OnSceneChange()
		{
			if (Options.AutoPosScaleOnLoad)
			{
				AutoScale();
			}
		}

		private static void OnOptionsChange()
		{
			EditorMotionController.CurrentMode = Options.Mode;
		}

		private static void AutoScale()
		{
			var sceneBounds = GetSceneBounds();
			var bbox = sceneBounds.size;
			var workspace = HandTracking.Workspace;
			Options.PosScale = Mathf.Max(bbox.x / workspace.x, bbox.y / workspace.y, bbox.z / workspace.z);
		}

		private static Bounds GetSceneBounds()
		{
			Bounds b = new Bounds(Vector3.zero, Vector3.zero);
			foreach (Renderer r in Object.FindObjectsOfType(typeof(Renderer)))
			{
				b.Encapsulate(r.bounds);
			}
			return b;
		}

		private static void OnHandsVisible()
		{
			EditorWorkspaceController.GridFadeIn();
		}

		private static void OnHandsInvisible()
		{
			EditorWorkspaceController.GridFadeOut();
		}

		private static void OnStateChange()
		{
		}

		public static Vector3 GetSelectionCenter()
		{
			var transforms = Selection.GetTransforms(SelectionMode.Deep | SelectionMode.ExcludePrefab);
			if (transforms == null || transforms.Length == 0)
			{
				return Vector3.zero;
			}

			Bounds b = new Bounds(transforms[0].position, Vector3.zero);
			foreach (var t in transforms)
			{
				b.Encapsulate(t.position);
			}
			return b.center;
		}
	}
}
#endif