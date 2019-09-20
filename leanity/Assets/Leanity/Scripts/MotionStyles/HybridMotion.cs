using UnityEngine;

namespace Leanity
{
	public class HybridMotion : IMotionStyle
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public float Scale { get; set; }

		public GestureController LeftGesture
		{
			get { return _oneMotion.LeftGesture; }
			set
			{
				_oneMotion.LeftGesture = value;
				_twoMotion.LeftGesture = value;
			}
		}
		public GestureController RightGesture
		{
			get { return _oneMotion.RightGesture; }
			set
			{
				_oneMotion.RightGesture = value;
				_twoMotion.RightGesture = value;
			}
		}
		public InertialObject InertialData { get; set; }

		public bool RequiresTwoHands { get { return false; } }

		public bool InvertAxis {
			get { return _oneMotion.InvertAxis; }
			set
			{
				_oneMotion.InvertAxis = value;
				_twoMotion.InvertAxis = value;
			}
		}

		private IMotionStyle _oneMotion;
		private IMotionStyle _twoMotion;

		private IMotionStyle _currentMotion;

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
		}

		public bool InertialUpdate()
		{
			return _currentMotion.InertialUpdate();
		}

		private void UpdateData()
		{
			_oneMotion.Position = Position;
			_oneMotion.Rotation = Rotation;
			_oneMotion.Scale = Scale;

			_twoMotion.Position = Position;
			_twoMotion.Rotation = Rotation;
			_twoMotion.Scale = Scale;
		}

		private IMotionStyle ChooseCurrentMotion()
		{
			return LeftGesture.IsHolding && RightGesture.IsHolding ? _twoMotion : _oneMotion;
		}

		public void Start()
		{
			UpdateData();
			_currentMotion = ChooseCurrentMotion();
			_currentMotion.Start();
		}

		public void Update()
		{
			UpdateData();

			var newMotion = ChooseCurrentMotion();
			if (newMotion != _currentMotion)
			{
				newMotion.Start();
				_currentMotion = newMotion;
			}
			else
			{
				_currentMotion.Update();
				Position = _currentMotion.Position;
				Rotation = _currentMotion.Rotation;
				Scale = _currentMotion.Scale;
			}
		}

		public void Stop()
		{
			_currentMotion.Stop();
		}

		public void LateFrameUpdate()
		{
			_oneMotion.LateFrameUpdate();
			_twoMotion.LateFrameUpdate();
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
	}
}
