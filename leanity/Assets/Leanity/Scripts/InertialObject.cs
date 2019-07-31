using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CircularBuffer;

public class InertialObject {
	struct PositionTime
	{
		public Vector3 Position;
		public float Timestamp;
	}

	struct RotationTime
	{
		public Quaternion Rotation;
		public float Timestamp;
	}

	private CircularBuffer<PositionTime> _posBuffer;
	private CircularBuffer<RotationTime> _rotBuffer;
	private readonly int _bufferLength;


	public InertialObject (int bufferLength)
	{
		_bufferLength = bufferLength;
		_posBuffer = new CircularBuffer<PositionTime>(_bufferLength);
		_rotBuffer = new CircularBuffer<RotationTime>(_bufferLength);
	}


	public void SetPosition(Vector3 position, float timestamp)
	{
		_posBuffer.PushBack(new PositionTime() { Position = position, Timestamp = timestamp });
	}

	public void SetRotation(Quaternion rotation, float timestamp)
	{
		_rotBuffer.PushBack(new RotationTime() { Rotation = rotation, Timestamp = timestamp });
	}

	public void Clear()
	{

	}

	public Vector3 GetLinearVelocity()
	{
		if (_posBuffer.IsEmpty || _posBuffer.Size == 1)
		{
			return default(Vector3);
		}
		var lastPos = _posBuffer.Back();
		var oldPos = _posBuffer.Front();

		float deltaTime = lastPos.Timestamp - oldPos.Timestamp;
		return (lastPos.Position - oldPos.Position) / deltaTime;
	}

	public Quaternion GetAngularVelocity()
	{
		if (_rotBuffer.IsEmpty || _rotBuffer.Size == 1)
		{
			return default(Quaternion);
		}
		var lastRot = _rotBuffer.Back();
		var oldRot = _rotBuffer.Front();

		float inverseDeltaTime = 1f/(lastRot.Timestamp - oldRot.Timestamp);
		Quaternion deltaRotation = Quaternion.Inverse(oldRot.Rotation) * lastRot.Rotation;
		Vector3 eulerAngularVelocity = NormalizedEulerAngles(deltaRotation) * inverseDeltaTime;

		return Quaternion.Euler(eulerAngularVelocity);
	}

	private Vector3 NormalizedEulerAngles(Quaternion q)
	{
		Vector3 euler = q.eulerAngles;
		euler.x = NormalizeAngle(euler.x);
		euler.y = NormalizeAngle(euler.y);
		euler.z = NormalizeAngle(euler.z);

		return euler;
	}

	private float NormalizeAngle(float angle)
	{
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}


}
