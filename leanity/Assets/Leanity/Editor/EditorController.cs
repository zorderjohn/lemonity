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
		public static WorkspaceController EditorWorkspaceController { get; private set; }
		private static Mesh _mesh;


		static EditorController()
		{
			Debug.Log("EditorController constructor");
			EditorMotionController = new MotionController();
			EditorWorkspaceController = new WorkspaceController(HandTracking.Workspace);
			InitWorkspace();
			EditorApplication.update += Update;
			Options.OnOptionsChange += OptionsChange;
		}

		static public void Update()
		{
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
				UpdateWorkspace(HandTracking.ToWorldCoordinates(Vector3.zero), camRot);

				if (EditorMotionController.Update(camPos, camRot))
				{
					camPos = EditorMotionController.Position;
					camRot = EditorMotionController.Rotation;

					sceneView.rotation = camRot;
					sceneView.pivot = MathHelper.CameraPivot(camPos, camRot, sceneView.cameraDistance);
				}

				if (Options.PinchEnabled && EditorMotionController.ScaleUpdate(Options.PosScale))
				{
					Options.PosScale = EditorMotionController.Scale;
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
				_mesh = workspaceGameObject.GetComponentInChildren<MeshFilter>().sharedMesh;
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

			if (workspace && Options.ShowWorkspace)
			{
				workspace.SetTransform(camPos, camRot, Options.AxisRotScale * Options.PosScale);
				workspace.SetTransparency(Options.GridTransparency);

				Graphics.DrawMeshNow(_mesh, Vector3.zero, Quaternion.identity);
			}
		}

		private static void OptionsChange()
		{
			var workspace = LeanityWorkspace.Instance;

			if (workspace && workspace.gameObject)
			{
				workspace.gameObject.SetActive(Options.ShowWorkspace);
			}
		}

		private static void OnSceneChange()
		{
			Debug.Log("On scene change");
			InitWorkspace();
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
	}
}
#endif