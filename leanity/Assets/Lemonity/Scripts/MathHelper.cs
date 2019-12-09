using UnityEditor;
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

	public static Vector3 ClampEulerRotationXZ(Vector3 euler, bool pitchLimit, float pitchMinAngle, float pitchMaxAngle, bool rollLimit)
	{
		//Unity order: ZXY
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
				euler.x = normalizedX > 0 ? pitchMaxAngle : pitchMinAngle;
				if (rollLimit)
				{
					euler.z = 0f;
					euler.y -= 180f;
				}
			} else
			{
				euler.x = Mathf.Clamp(normalizedX, pitchMinAngle, pitchMaxAngle);
			}
		}

		return euler;
	}

	public static Quaternion ClampRotationXZ(Quaternion rot, bool pitchLimit, float pitchMinAngle, float pitchMaxAngle, bool rollLimit)
	{
		if (!pitchLimit && !rollLimit)
		{
			return rot;
		}
		return Quaternion.Euler(ClampEulerRotationXZ(rot.eulerAngles, pitchLimit, pitchMinAngle, pitchMaxAngle, rollLimit));
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

	public static float ExponentialScale(float value, float scale, float exponential)
	{
		value *= scale;
		float sign = Mathf.Sign(value);
		float absValue = Mathf.Abs(value);
		return sign * Mathf.Pow(absValue, exponential);
	}

	public static Vector3 ExponentialScale(Vector3 value, float scale, float exponential)
	{
		float magnitude = value.magnitude;
		Vector3 normalized = magnitude > Mathf.Epsilon ? value.normalized : Vector3.zero;
		return normalized * Mathf.Pow(magnitude * scale, exponential);
	}

	public static Vector3 GetOrthogonalAxis(Vector3 value)
	{
		int axis = GetMainAxis(value);
		return GetOrthogonalAxis(value, axis);
	}

	public static Vector3 GetOrthogonalAxis(Vector3 value, int axis)
	{
		switch(axis)
		{
			case 0:
				return new Vector3(value.x, 0f, 0f);
			case 1:
				return new Vector3(0f, value.y, 0f);
			default:
				return new Vector3(0f, 0f, value.z);
		}
	}

	public static int GetMainAxis(Vector3 value)
	{
		float absX = Mathf.Abs(value.x);
		float absY = Mathf.Abs(value.y);
		float absZ = Mathf.Abs(value.z);

		if (absX >= absY && absX >= absZ)
		{
			return 0;
		}
		else if (absY >= absX && absY >= absZ)
		{
			return 1;
		}
		else
		{
			return 2;
		}

	}

	public static float EaseOutQuad(float value)
	{
		return 1f - (1f - value) * (1f - value);
	}

	public static float EaseOutPow(float value, float exponent)
	{
		return 1f - Mathf.Pow(1f - value, exponent);
	}

	public static float EaseInOutSin(float value)
	{
		return 0.5f + 0.5f * Mathf.Cos(value * Mathf.PI + Mathf.PI);
	}
}
