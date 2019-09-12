using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

//#if UNITY_EDITOR
namespace Leanity
{
	[InitializeOnLoad]
	//[Serializable]
	public class EditorController
	{
		private static LeanityWorkspace _workspace;
		private const string CAMERA_GRID_PREFAB = "WorkspaceGridDummy";
		private static UnityEngine.SceneManagement.Scene _scene;

		public static MotionController EditorMotionController { get; private set; }

		static EditorController()
		{
			EditorMotionController = new MotionController();
			InitCameraGrid();
			EditorApplication.update += Update;
		}

		static public void Update()
		{
			var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			if (activeScene != _scene)
			{
				_scene = activeScene;
				InitCameraGrid();
			}

			// Calculate cam position and rotation
			var sceneView = SceneView.lastActiveSceneView;


			if (sceneView != null && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
			{
				var camRot = sceneView.rotation;
				var camPos = MathHelper.CameraPosition(sceneView.pivot, sceneView.rotation, sceneView.cameraDistance);
				UpdateCameraGrid(HandTracking.ToWorldCoordinates(Vector3.zero), camRot);

				if (EditorMotionController.Update(camPos, camRot))
				{
					camPos = EditorMotionController.Position;
					camRot = EditorMotionController.Rotation;

					sceneView.rotation = camRot;
					sceneView.pivot = MathHelper.CameraPivot(camPos, camRot, sceneView.cameraDistance);
				}
			}
		}

		private static void InitCameraGrid()
		{
			_workspace = GameObject.FindObjectOfType<LeanityWorkspace>();
			GameObject cameraGrid;

			if (!_workspace)
			{
				cameraGrid = UnityEngine.Object.Instantiate(Resources.Load(CAMERA_GRID_PREFAB)) as GameObject;
				_workspace = cameraGrid.GetComponent<LeanityWorkspace>();
			} else
			{
				cameraGrid = _workspace.gameObject;
			}

			Options.RegisteredLeanityWorkspace = _workspace;
			cameraGrid.hideFlags = HideFlags.HideAndDontSave;
		}

		private static void UpdateCameraGrid(Vector3 camPos, Quaternion camRot)
		{
			if (_workspace)
			{
				_workspace.transform.position = camPos;
				_workspace.transform.rotation = camRot;
				_workspace.transform.localScale = Options.AxisRotScale * Options.PosScale;
				_workspace.SetTransparency(Options.GridTransparency);
			}
		}



	}
}
//#endif