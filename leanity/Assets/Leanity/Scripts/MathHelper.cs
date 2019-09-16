using UnityEngine;

public static class MathHelper
{
	public static Vector3 NormalizedEulerAngles(Quaternion q)
	{
		Vector3 euler = q.eulerAngles;
		euler.x = NormalizeAngle(euler.x);
		euler.y = NormalizeAngle(euler.y);
		euler.z = NormalizeAngle(euler.z);

		return euler;
	}

	public static float NormalizeAngle(float angle)
	{
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}

	public static Vector3 ClampEulerRotationXZ(Vector3 euler, float xMin, float xMax, float zMin, float zMax)
	{
		euler.x = Mathf.Clamp(NormalizeAngle(euler.x), xMin, xMax);
		euler.z = Mathf.Clamp(NormalizeAngle(euler.z), zMin, zMax);
		return euler;
	}

	public static Quaternion ClampRotationXZ(Quaternion rot, float xMin, float xMax, float zMin, float zMax)
	{
		return Quaternion.Euler(ClampEulerRotationXZ(rot.eulerAngles, xMin, xMax, zMin, zMax));
	}


	public static Vector3 CameraPosition(Vector3 pivot, Quaternion rotation, float distance)
	{
		return pivot - (rotation * (Vector3.forward * distance));
	}

	public static Vector3 CameraPivot(Vector3 position, Quaternion rotation, float distance)
	{
		return rotation * (Vector3.forward * distance) + position;
	}


	// x = 0, y = 0
	// x = 59048, y = 10
	public static float LinearToLogScale(float value)
	{
		return Mathf.Log(value + 1f, 3f);
	}

	// x = 0, y = 0
	// x = 10, y = 59048
	public static float LogToLinearScale(float value)
	{
		return Mathf.Pow(3f, value) - 1f;
	}
}
