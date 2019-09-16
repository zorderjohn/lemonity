using UnityEngine;

namespace Leanity
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
			}
		}

		public Quaternion _rotation;
		public Quaternion Rotation
		{
			get { return _rotation; }
			set
			{
				_rotation = _rotationFilter.Filter(value);
			}
		}
		public float PinchDistance { get; set; }

		private OneEuroFilter<Quaternion> _rotationFilter;
		private OneEuroFilter<Vector3> _positionFilter;

		public HandData(float filterFrequency)
		{
			_rotationFilter = new OneEuroFilter<Quaternion>(filterFrequency);
			_positionFilter = new OneEuroFilter<Vector3>(filterFrequency);
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