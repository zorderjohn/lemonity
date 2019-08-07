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
}
