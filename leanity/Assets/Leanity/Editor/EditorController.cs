using UnityEngine;
using UnityEditor;
using System;

namespace Leanity
{
	[InitializeOnLoad]
	[Serializable]
	class EditorController
	{
		static MotionController _motion;

		public static MotionController EditorMotionController
		{
			get { return _motion; }
		}

		static EditorController()
		{
			_motion = new MotionController();
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

				if (_motion.Update(camPos, camRot))
				{
					camPos = _motion.Position;
					camRot = _motion.Rotation;

					scene.rotation = camRot;
					scene.pivot = MathHelper.CameraPivot(camPos, camRot, scene.cameraDistance);
				}
			}
		}

	}
}