using UnityEngine;

namespace Leanity
{
	public class HandData
	{
		public bool IsRight { get; set; }
		public Vector3 InitialPosition { get; private set; }
		public Quaternion InitialRotation { get; private set; }
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

		public Vector3 DeltaPosition
		{
			get { return Position - InitialPosition; }
		}

		public Quaternion DeltaRotation
		{
			get { return Quaternion.Inverse(InitialRotation) * Rotation; }
		}


		private OneEuroFilter<Quaternion> _rotationFilter;
		private OneEuroFilter<Vector3> _positionFilter;

		public HandData(float filterFrequency)
		{
			_rotationFilter = new OneEuroFilter<Quaternion>(filterFrequency);
			_positionFilter = new OneEuroFilter<Vector3>(filterFrequency);
		}

		public void CaptureInitialPose()
		{
			InitialPosition = Position;
			InitialRotation = Rotation;
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