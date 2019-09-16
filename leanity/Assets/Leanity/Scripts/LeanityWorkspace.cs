using UnityEngine;
using UnityEditor;
using System;

namespace Leanity
{
	[ExecuteInEditMode]
	public class LeanityWorkspace : MonoBehaviour
	{
		public static LeanityWorkspace Instance { get; private set; } = null;

		private Color _cameraGridColor;
		private Material _cameraGridMaterial;
		private bool _init = false;

		void Init()
		{
			Debug.Log(gameObject.name + gameObject.GetHashCode() + ": init");
			_cameraGridMaterial = GetComponentInChildren<MeshRenderer>().sharedMaterial;
			if (_cameraGridMaterial)
			{
				_cameraGridColor = _cameraGridMaterial.color;
			}
			_init = true;
		}

		private void Start()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			if (Instance == this)
			{
				Debug.Log(gameObject.name + gameObject.GetHashCode() + ": workspace start succesfully .");
			}
			else
			{
				Debug.Log(gameObject.name + gameObject.GetHashCode() + ": workspace start dying.");
				DestroyImmediate(gameObject);
			}
		}

		private void Update()
		{
			if (Instance != this)
			{
				Debug.Log(gameObject.name + gameObject.GetHashCode() + ": workspace update dying.");
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

		public void SetTransform(Vector3 pos, Quaternion rot, Vector3 scale)
		{
			transform.position = pos;
			transform.rotation = rot;
			transform.localScale = scale;
		}
	}
}