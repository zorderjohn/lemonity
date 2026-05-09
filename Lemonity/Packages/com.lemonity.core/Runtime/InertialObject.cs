using UnityEngine;

namespace Lemonity.Core
{
	public class InertialObject
	{
		struct PositionFrame
		{
			public Vector3 Position;
			public float Timestamp;
		}

		struct RotationFrame
		{
			public Quaternion Rotation;
			public float Timestamp;
		}

		public Vector3 LinearVelocity { get; set; }
		public Vector3 AngularVelocityEuler { get; set; }

		public Vector3 Position
		{
			get { return _lastPositionFrame.Position; }
		}

		public Quaternion Rotation
		{
			get { return _lastRotationFrame.Rotation; }
		}


		public bool IsMoving { get; private set; }

		private readonly int MIN_BUFFER_SIZE = 2;
		private readonly int MAX_BUFFER_SIZE = 1000;

		private CircularBuffer<PositionFrame> _posBuffer;
		private CircularBuffer<RotationFrame> _rotBuffer;

		private PositionFrame _lastPositionFrame;
		private RotationFrame _lastRotationFrame;

		protected readonly float TERMINAL_SQR_VELOCITY = 0.01f;
		private readonly CameraOptions _cameraOptions;


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
					_posBuffer = new CircularBuffer<PositionFrame>(_bufferLength);
					_rotBuffer = new CircularBuffer<RotationFrame>(_bufferLength);
				}
			}
		}

		public InertialObject(int bufferLength, CameraOptions cameraOptions = null)
		{
			BufferLength = Mathf.Clamp(bufferLength, 2, 1000);
			_cameraOptions = cameraOptions;
		}

		public void SetPosition(Vector3 position, float timestamp)
		{
			_lastPositionFrame = new PositionFrame() { Position = position, Timestamp = timestamp };
			_posBuffer.PushBack(_lastPositionFrame);
		}

		public void SetRotation(Quaternion rotation, float timestamp)
		{
			_lastRotationFrame = new RotationFrame() { Rotation = rotation, Timestamp = timestamp };
			_rotBuffer.PushBack(_lastRotationFrame);
		}

		public void Clear()
		{
			_posBuffer.Clear();
			_rotBuffer.Clear();
			LinearVelocity = Vector3.zero;
			AngularVelocityEuler = Vector3.zero;
			IsMoving = false;
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
			UpdateMovementDetection();
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
			UpdateMovementDetection();
		}

		public void DragAngularVelocity(float drag, float time)
		{
			float rotDeltaTime = time - _lastRotationFrame.Timestamp;
			var newAngularVelocity = AngularVelocityEuler * Mathf.Clamp01(1f - rotDeltaTime * drag);
			newAngularVelocity.z = 0f;
			AngularVelocityEuler = newAngularVelocity;
			UpdateMovementDetection();
		}

		public void DragLinearVelocity(float drag, float time)
		{
			float posDeltaTime = time - _lastPositionFrame.Timestamp;
			LinearVelocity = LinearVelocity * Mathf.Clamp01(1f - posDeltaTime * drag);
			UpdateMovementDetection();
		}

		public bool Update(float time)
		{
			if (IsMoving)
			{
				float posDeltaTime = time - _lastPositionFrame.Timestamp;
				_lastPositionFrame.Position += LinearVelocity * posDeltaTime;
				_lastPositionFrame.Timestamp = time;

				float rotDeltaTime = time - _lastRotationFrame.Timestamp;
				Quaternion deltaRotation = Quaternion.Euler(AngularVelocityEuler * rotDeltaTime);
				Quaternion newOrientation = _lastRotationFrame.Rotation * deltaRotation;
				_lastRotationFrame.Rotation = _cameraOptions == null
					? newOrientation
					: MathHelper.ClampRotationXZ(newOrientation, _cameraOptions.PitchLimit, _cameraOptions.PitchMinAngleLimit, _cameraOptions.PitchMaxAngleLimit, _cameraOptions.RollLimit);
				_lastRotationFrame.Timestamp = time;
			}
			return IsMoving;
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

		private void UpdateMovementDetection()
		{
			IsMoving = LinearVelocity.sqrMagnitude + AngularVelocityEuler.sqrMagnitude >= TERMINAL_SQR_VELOCITY;
		}

	}
}
