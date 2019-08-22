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

		static EditorController()
		{
			_motion = new MotionController();
			EditorApplication.update += Update;
		}

		static void Update()
		{
			// Calculate cam position and rotation
			var scene = SceneView.lastActiveSceneView;

			if (scene != null)
			{
				var camRot = scene.rotation;
				var camPos = MathHelper.CameraPosition(scene.pivot, scene.rotation, scene.cameraDistance);

				if (_motion.Update(camPos, camRot))
				{
					camPos = _motion.ObjectPosition;
					camRot = _motion.ObjectRotation;

					scene.rotation = camRot;
					scene.pivot = MathHelper.CameraPivot(camPos, camRot, scene.cameraDistance);
				}
			}
		}
	}
}