using UnityEngine;

[ExecuteInEditMode]
public class LeanityWorkspace : MonoBehaviour {

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
