using UnityEngine;

namespace Lemonity
{
	public class HybridMotion : IMotionStyle
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public float Scale { get; set; }

		public IGestureController LeftGesture
		{
			get { return _oneMotion.LeftGesture; }
			set
			{
				_oneMotion.LeftGesture = value;
				_twoMotion.LeftGesture = value;
			}
		}
		public IGestureController RightGesture
		{
			get { return _oneMotion.RightGesture; }
			set
			{
				_oneMotion.RightGesture = value;
				_twoMotion.RightGesture = value;
			}
		}

		public bool RequiresTwoHands { get { return false; } }

		public bool HasInertia
		{
			get
			{
				return _currentMotion.HasInertia;
			}
		}
		private IMotionStyle _oneMotion;
		private IMotionStyle _twoMotion;

		private IMotionStyle _lastMotion;
		private IMotionStyle _currentMotion;
		private float _motionChangeTimestamp = 0f;

		public HybridMotion()
		{
			_oneMotion = new OneHandMotion();
			_oneMotion.LeftGesture = LeftGesture;
			_oneMotion.RightGesture = RightGesture;
			_oneMotion.OptionsChange();

			_twoMotion = new TwoHandsMotion();
			_twoMotion.LeftGesture = LeftGesture;
			_twoMotion.RightGesture = RightGesture;
			_twoMotion.OptionsChange();

			_currentMotion = _oneMotion;
			_lastMotion = _oneMotion;
		}

		public bool InertialMovement()
		{
			if (_currentMotion.InertialMovement())
			{
				GetMotionData();
				return true;
			}
			return false;
		}

		private void SetMotionData()
		{
			_oneMotion.Position = Position;
			_oneMotion.Rotation = Rotation;
			_oneMotion.Scale = Scale;

			_twoMotion.Position = Position;
			_twoMotion.Rotation = Rotation;
			_twoMotion.Scale = Scale;
		}

		private void GetMotionData()
		{
			Position = _currentMotion.Position;
			Rotation = _currentMotion.Rotation;
			Scale = _currentMotion.Scale;
		}

		private IMotionStyle ChooseCurrentMotion()
		{
			return LeftGesture.IsHolding && RightGesture.IsHolding ? _twoMotion : _oneMotion;
		}

		public void Start()
		{
			SetMotionData();
			_currentMotion = ChooseCurrentMotion();
			_currentMotion.Start();
		}

		public void Update()
		{
			SetMotionData();

			var newMotion = ChooseCurrentMotion();
			if (newMotion != _currentMotion)
			{
				_currentMotion.Stop();
				newMotion.Start();
				SetNewMotion(newMotion);
			}
			else
			{
				_currentMotion.Update();
				GetMotionData();
			}
		}

		public void Stop()
		{
			_currentMotion.Stop();
			if (GetTime() - _motionChangeTimestamp < 0.5f)
			{
				_currentMotion = _lastMotion;
				_currentMotion.Update();
				_currentMotion.Stop();
			}
		}

		private void SetNewMotion(IMotionStyle _motion)
		{
			_motionChangeTimestamp = GetTime();
			_lastMotion = _currentMotion;
			_currentMotion = _motion;
		}

		public void StopInertia()
		{
			_currentMotion.StopInertia();
		}

		public void OptionsChange()
		{
			_oneMotion.OptionsChange();
			_twoMotion.OptionsChange();
		}

		public void DebugDraw()
		{
			_currentMotion.DebugDraw();
		}

		protected float GetTime()
		{
			return Time.realtimeSinceStartup;
		}
	}
}
