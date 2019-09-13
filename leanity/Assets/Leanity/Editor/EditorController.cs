using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
namespace Leanity
{
	[InitializeOnLoad]
	public class EditorController
	{
		private const string CAMERA_GRID_PREFAB = "WorkspaceGridDummy";
		private static UnityEngine.SceneManagement.Scene _scene;

		public static MotionController EditorMotionController { get; private set; }

		static EditorController()
		{
			Debug.Log("EditorController constructor");
			EditorMotionController = new MotionController();
			InitWorkspace();
			EditorApplication.update += Update;
		}

		static public void Update()
		{
			var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			if (activeScene != _scene)
			{
				Debug.Log("Scene change, reset camera grid");
				_scene = activeScene;
				InitWorkspace();
			}

			// Calculate cam position and rotation
			var sceneView = SceneView.lastActiveSceneView;


			if (sceneView != null && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
			{
				var camRot = sceneView.rotation;
				var camPos = MathHelper.CameraPosition(sceneView.pivot, sceneView.rotation, sceneView.cameraDistance);
				UpdateWorkspace(HandTracking.ToWorldCoordinates(Vector3.zero), camRot);

				if (EditorMotionController.Update(camPos, camRot))
				{
					camPos = EditorMotionController.Position;
					camRot = EditorMotionController.Rotation;

					sceneView.rotation = camRot;
					sceneView.pivot = MathHelper.CameraPivot(camPos, camRot, sceneView.cameraDistance);
				}
			}
		}

		private static void InitWorkspace()
		{
			var workspace = LeanityWorkspace.Instance;
			GameObject workspaceGameObject;

			if (!workspace)
			{
				workspaceGameObject = Object.Instantiate(Resources.Load(CAMERA_GRID_PREFAB)) as GameObject;
			}
			else
			{
				workspaceGameObject = workspace.gameObject;
			}

			if (workspaceGameObject)
			{
				workspaceGameObject.hideFlags = HideFlags.HideAndDontSave;
			}
			else
			{
				Debug.LogError("Unable to load " + CAMERA_GRID_PREFAB + " prefab. Workspace visualization disabled.");
			}
		}

		private static void UpdateWorkspace(Vector3 camPos, Quaternion camRot)
		{
			var workspace = LeanityWorkspace.Instance;

			if (workspace)
			{
				workspace.SetTransform(camPos, camRot, Options.AxisRotScale * Options.PosScale);
				workspace.SetTransparency(Options.GridTransparency);
			}
		}
	}
}
#endif