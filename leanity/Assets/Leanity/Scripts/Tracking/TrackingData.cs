using UnityEngine;

namespace Lemonity
{
	public class HandData
	{
		public bool IsRight { get; set; }
		public float GrabValue { get; set; }
		public bool Detected { get; set; }

		private Vector3 _position;
		public Vector3 Position
		{
			get { return _position; }
			set
			{
				_position = _positionFilter.Filter(value);
				_inertialData.SetPosition(_position, Time.realtimeSinceStartup);
				_inertialData.CalculateLinearVelocity();
			}
		}

		public Vector3 LinearVelocity
		{
			get { return _inertialData.LinearVelocity; }
		}

		public Vector3 AngularVelocity
		{
			get { return _inertialData.AngularVelocityEuler; }
		}

		public Quaternion _rotation;
		public Quaternion Rotation
		{
			get { return _rotation; }
			set
			{
				_rotation = _rotationFilter.Filter(value);
				_inertialData.SetRotation(_rotation, Time.realtimeSinceStartup);
				_inertialData.CalculateAngularVelocity();
			}
		}
		public float PinchDistance { get; set; }

		private OneEuroFilter<Quaternion> _rotationFilter;
		private OneEuroFilter<Vector3> _positionFilter;
		private InertialObject _inertialData;

		public HandData(float filterFrequency, int inertialFrames)
		{
			_rotationFilter = new OneEuroFilter<Quaternion>(filterFrequency);
			_positionFilter = new OneEuroFilter<Vector3>(filterFrequency);
			_inertialData = new InertialObject(inertialFrames);
		}

		public void SetRotationFilterParams(float frequency, float minCutOff, float beta, float dCutOff)
		{
			_rotationFilter.UpdateParams(frequency, minCutOff, beta, dCutOff);
		}

		public void SetPositionFilterParams(float frequency, float minCutOff, float beta, float dCutOff)
		{
			_positionFilter.UpdateParams(frequency, minCutOff, beta, dCutOff);
		}
	}
}