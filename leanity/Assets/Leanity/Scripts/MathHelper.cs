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

	/// <summary>
	/// Return angle (in degrees) between -180 and 180
	/// </summary>
	/// <param name="angle">Angle in degrees</param>
	/// <returns></returns>
	public static float NormalizeAngle(float angle)
	{
		return angle <= 180f ? angle: angle - 360f;
	}

	public static Vector3 ClampEulerRotationXZ(Vector3 euler, bool pitchLimit, float pitchMaxAngle, bool rollLimit)
	{
		//Unity order: ZXY
		string dbgStr = euler.ToString();
		float normalizedZ = NormalizeAngle(euler.z);
		float normalizedX = NormalizeAngle(euler.x);
		bool flippedZ = Mathf.Abs(normalizedZ) > 90f;

		if (rollLimit)
		{
			euler.z = flippedZ ? 180f : 0f;
		}
		if (pitchLimit)
		{
			if (flippedZ)
			{
				euler.x = normalizedX > 0 ? pitchMaxAngle : -pitchMaxAngle;
				if (rollLimit)
				{
					euler.z = 0f;
					euler.y -= 180f;
				}
			} else
			{
				euler.x = Mathf.Clamp(normalizedX, -pitchMaxAngle, pitchMaxAngle);
			}
		}
		//	euler.x = Mathf.Clamp(NormalizeAngle(euler.x), xMin, xMax);
		//	euler.z = Mathf.Clamp(NormalizeAngle(euler.z), zMin, zMax);

		dbgStr += " --> " + euler.ToString();
		Debug.Log(dbgStr);
		return euler;
	}

	public static Quaternion ClampRotationXZ(Quaternion rot, bool pitchLimit, float pitchMaxAngle, bool rollLimit)
	{
		if (!pitchLimit && !rollLimit)
		{
			return rot;
		}
		return Quaternion.Euler(ClampEulerRotationXZ(rot.eulerAngles, pitchLimit, pitchMaxAngle, rollLimit));
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
