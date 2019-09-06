using UnityEngine;
using CircularBuffer;

namespace Leanity
{
	public class InertialObject
	{
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

		public Vector3 LinearVelocity { get; set; }
		public Vector3 AngularVelocityEuler { get; set; }

		private readonly int MIN_BUFFER_SIZE = 2;
		private readonly int MAX_BUFFER_SIZE = 1000;

		private CircularBuffer<PositionTime> _posBuffer;
		private CircularBuffer<RotationTime> _rotBuffer;

		private int _bufferLength = -1;
		public int BufferLength
		{
			get
			{
				return _bufferLength;
			}
			set
			{
				int newLength = Mathf.Clamp(value, MIN_BUFFER_SIZE, MAX_BUFFER_SIZE);

				if (_bufferLength != newLength)
				{
					_bufferLength = newLength;
					_posBuffer = new CircularBuffer<PositionTime>(_bufferLength);
					_rotBuffer = new CircularBuffer<RotationTime>(_bufferLength);
				}
			}
		}

		public InertialObject(int bufferLength)
		{
			BufferLength = Mathf.Clamp(bufferLength, 2, 1000);
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
			_posBuffer.Clear();
			_rotBuffer.Clear();
		}

		public void DiscardFrames(int frames)
		{
			for (int i = 0; i < frames; i++)
			{
				if (!_rotBuffer.IsEmpty)
					_rotBuffer.PopBack();

				if (!_posBuffer.IsEmpty)
					_posBuffer.PopBack();
			}
		}

		public void CalculateLinearVelocity()
		{
			if (_posBuffer.IsEmpty || _posBuffer.Size == 1)
			{
				LinearVelocity = Vector3.zero;
			}
			else
			{
				var lastPos = _posBuffer.Back();
				var oldPos = _posBuffer.Front();

				float deltaTime = lastPos.Timestamp - oldPos.Timestamp;

				LinearVelocity = deltaTime <= 0f ? Vector3.zero : (lastPos.Position - oldPos.Position) / deltaTime;
			}
		}

		public void CalculateAngularVelocity()
		{
			if (_rotBuffer.IsEmpty || _rotBuffer.Size == 1)
			{
				AngularVelocityEuler = Vector3.zero;
			}
			else
			{
				var lastRot = _rotBuffer.Back();
				var oldRot = _rotBuffer.Front();
				float inverseDeltaTime = 1f / (lastRot.Timestamp - oldRot.Timestamp);
				Quaternion deltaRotation = Quaternion.Inverse(oldRot.Rotation) * lastRot.Rotation;
				AngularVelocityEuler = NormalizedEulerAngles(deltaRotation) * inverseDeltaTime;
			}
		}

		public void DragLinearVelocity(float drag)
		{
			var newAngularVelocity = AngularVelocityEuler * drag;
			newAngularVelocity.z = 0f;
			AngularVelocityEuler = newAngularVelocity;
		}

		public void DragAngularVelocity(float drag)
		{
			LinearVelocity *= drag;
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
}