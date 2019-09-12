using UnityEngine;
using UnityEditor;
using System;

#if UNITY_EDITOR
namespace Leanity
{
	[InitializeOnLoad]
	[Serializable]
	class EditorController
	{
		private static LeanityWorkspace _workspace;
		private static MotionController _motion;
		private const string CAMERA_GRID_PREFAB = "WorkspaceGridDummy";

		public static MotionController EditorMotionController
		{
			get { return _motion; }
		}

		static EditorController()
		{
			_motion = new MotionController();
			InitCameraGrid();
			EditorApplication.update += Update;
		}

		static public void Update()
		{
			// Calculate cam position and rotation
			var scene = SceneView.lastActiveSceneView;


			if (scene != null && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
			{
				var camRot = scene.rotation;
				var camPos = MathHelper.CameraPosition(scene.pivot, scene.rotation, scene.cameraDistance);
				UpdateCameraGrid(HandTracking.ToWorldCoordinates(Vector3.zero), camRot);

				if (_motion.Update(camPos, camRot))
				{
					camPos = _motion.Position;
					camRot = _motion.Rotation;

					scene.rotation = camRot;
					scene.pivot = MathHelper.CameraPivot(camPos, camRot, scene.cameraDistance);
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

			//_cameraGrid.hideFlags = HideFlags.HideAndDontSave;
		}

		public static void UpdateCameraGrid(Vector3 camPos, Quaternion camRot)
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
#endif