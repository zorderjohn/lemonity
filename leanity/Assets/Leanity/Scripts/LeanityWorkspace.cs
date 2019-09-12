using UnityEngine;
using UnityEditor;
using System;

namespace Leanity
{
	[ExecuteInEditMode]
	public class LeanityWorkspace : MonoBehaviour
	{

		private static Color _cameraGridColor;
		private static Material _cameraGridMaterial;
		private static bool _init = false;

		void Init()
		{
			_cameraGridMaterial = GetComponentInChildren<MeshRenderer>().sharedMaterial;
			if (_cameraGridMaterial)
			{
				_cameraGridColor = _cameraGridMaterial.color;
			}
			_init = true;
		}

		private void Start()
		{
			if (Options.RegisteredLeanityWorkspace != this)
			{
				Debug.Log(gameObject.name + ": workspace start dying.");
				DestroyImmediate(gameObject);
			}
			else
			{
				Debug.Log(gameObject.name + ": workspace start succesfully .");
			}
		}

		private void Update()
		{
			if (Options.RegisteredLeanityWorkspace != this)
			{
				Debug.Log(gameObject.name + ": workspace update dying.");
				DestroyImmediate(gameObject);
			}
		}

		public void SetTransparency(float alpha)
		{
			if (!_init)
			{
				Init();
			}

			if (_cameraGridMaterial)
			{
				_cameraGridColor.a = alpha;
				_cameraGridMaterial.color = _cameraGridColor;
			}
		}

	}
}