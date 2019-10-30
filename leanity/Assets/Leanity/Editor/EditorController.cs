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
			EditorMotionController = new MotionController();
			EditorWorkspaceController = new WorkspaceController(HandTracking.Workspace, EditorMotionController);
			EditorApplication.update += EditorUpdate;
			SceneView.onSceneGUIDelegate += OnSceneGUI;
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
			if (Time.realtimeSinceStartup - _lastUpdate > 0.5f ||
				Options.EnableInertia && EditorMotionController.GrabStyle.HasInertia)
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

			if (Options.Gesture == WorkingGesture.Disabled)
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

			if (sceneView != null && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
			{
				var camRot = sceneView.rotation;
				var camPos = MathHelper.CameraPosition(sceneView.pivot, sceneView.rotation, sceneView.cameraDistance);

				// As we are not drawing on PostRender we need to draw workspace using data from previous update
				// before changing camera
				DrawWorkspace(camPos + camRot * HandTracking.CamToHandOffset(), camRot);

				if (EditorMotionController.Update(camPos, camRot))
				{
					camPos = EditorMotionController.Position;
					camRot = EditorMotionController.Rotation;

					sceneView.rotation = camRot;
					sceneView.size = 1f + (Options.PosScale - 1f) * 0.1f;
					sceneView.pivot = MathHelper.CameraPivot(camPos, camRot, sceneView.cameraDistance);
				}

				if (EditorMotionController.ScaleUpdate(Options.PosScale))
				{
					Options.PosScale = EditorMotionController.Scale;
				}

				bool anyHandVisible = HandTracking.LeftHandData.Detected || HandTracking.RightHandData.Detected;
				if (anyHandVisible || EditorWorkspaceController.GridVisible)
				{
					if (GUIUpdate && Options.GestureDebug)
					{
						EditorMotionController.GrabStyle.DebugDraw();
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
	}
}
#endif