using System;
using UnityEngine;

namespace Lemonity.Core
{
	[Serializable]
	public sealed class GeneralOptions : OptionsGroupBase
	{
		[SerializeField]
		private WorkingMode _mode = WorkingMode.GrabHybrid;

		public WorkingMode Mode
		{
			get { return _mode; }
			set { SetValue(ref _mode, value); }
		}
	}

	[Serializable]
	public sealed class TrackingSpaceOptions : OptionsGroupBase
	{
		[SerializeField]
		private float _posScale = 1f;
		[SerializeField]
		private bool _autoPosScaleOnLoad = true;
		[SerializeField]
		private Vector3 _axisRotScale = Vector3.one;
		[SerializeField]
		private float _trackingZOffset = 1f;

		public float PosScale
		{
			get { return _posScale; }
			set { SetValue(ref _posScale, value); }
		}

		public bool AutoPosScaleOnLoad
		{
			get { return _autoPosScaleOnLoad; }
			set { SetValue(ref _autoPosScaleOnLoad, value); }
		}

		public Vector3 AxisRotScale
		{
			get { return _axisRotScale; }
			set { SetValue(ref _axisRotScale, value); }
		}

		public float TrackingZOffset
		{
			get { return _trackingZOffset; }
			set { SetValue(ref _trackingZOffset, value); }
		}
	}

	[Serializable]
	public sealed class GrabModeOptions : OptionsGroupBase
	{
		[SerializeField]
		private float _rotScale = 1f;
		[SerializeField]
		private float _zoomScale = 1f;
		[SerializeField]
		private bool _invertAxis;

		public float RotScale
		{
			get { return _rotScale; }
			set { SetValue(ref _rotScale, value); }
		}

		public float ZoomScale
		{
			get { return _zoomScale; }
			set { SetValue(ref _zoomScale, value); }
		}

		public bool InvertAxis
		{
			get { return _invertAxis; }
			set { SetValue(ref _invertAxis, value); }
		}
	}

	[Serializable]
	public sealed class OrbitModeOptions : OptionsGroupBase
	{
		[SerializeField]
		private float _zoomScale = 1f;
		[SerializeField]
		private float _pitchScale = 1f;
		[SerializeField]
		private float _yawScale = 1f;
		[SerializeField]
		private bool _exponentialZoom;
		[SerializeField]
		private bool _invertAxis;

		public float ZoomScale
		{
			get { return _zoomScale; }
			set { SetValue(ref _zoomScale, value); }
		}

		public float PitchScale
		{
			get { return _pitchScale; }
			set { SetValue(ref _pitchScale, value); }
		}

		public float YawScale
		{
			get { return _yawScale; }
			set { SetValue(ref _yawScale, value); }
		}

		public bool ExponentialZoom
		{
			get { return _exponentialZoom; }
			set { SetValue(ref _exponentialZoom, value); }
		}

		public bool InvertAxis
		{
			get { return _invertAxis; }
			set { SetValue(ref _invertAxis, value); }
		}
	}

	[Serializable]
	public sealed class FlyModeOptions : OptionsGroupBase
	{
		[SerializeField]
		private float _posScale = 1f;
		[SerializeField]
		private float _pitchScale = 1f;
		[SerializeField]
		private float _yawScale = 1f;
		[SerializeField]
		private float _exponentialFactor = 1.4f;
		[SerializeField]
		private bool _hover;
		[SerializeField]
		private float _hoverDistance = 2f;
		[SerializeField]
		private bool _invertAxis = true;

		public float PosScale
		{
			get { return _posScale; }
			set { SetValue(ref _posScale, value); }
		}

		public float PitchScale
		{
			get { return _pitchScale; }
			set { SetValue(ref _pitchScale, value); }
		}

		public float YawScale
		{
			get { return _yawScale; }
			set { SetValue(ref _yawScale, value); }
		}

		public float ExponentialFactor
		{
			get { return _exponentialFactor; }
			set { SetValue(ref _exponentialFactor, value); }
		}

		public bool Hover
		{
			get { return _hover; }
			set { SetValue(ref _hover, value); }
		}

		public float HoverDistance
		{
			get { return _hoverDistance; }
			set { SetValue(ref _hoverDistance, value); }
		}

		public bool InvertAxis
		{
			get { return _invertAxis; }
			set { SetValue(ref _invertAxis, value); }
		}
	}

	[Serializable]
	public sealed class CameraOptions : OptionsGroupBase
	{
		[SerializeField]
		private float _pitchMaxAngleLimit = 90f;
		[SerializeField]
		private float _pitchMinAngleLimit;
		[SerializeField]
		private bool _pitchLimit;

		public float PitchMaxAngleLimit
		{
			get { return _pitchMaxAngleLimit; }
			set { SetValue(ref _pitchMaxAngleLimit, value); }
		}

		public float PitchMinAngleLimit
		{
			get { return _pitchMinAngleLimit; }
			set { SetValue(ref _pitchMinAngleLimit, value); }
		}

		public bool PitchLimit
		{
			get { return _pitchLimit; }
			set { SetValue(ref _pitchLimit, value); }
		}

		public bool RollLimit
		{
			get { return true; }
		}
	}

	[Serializable]
	public sealed class GestureOptions : OptionsGroupBase
	{
		[SerializeField]
		private bool _grabEnabled = true;
		[SerializeField]
		private float _grabMinThreshold = 0.5f;
		[SerializeField]
		private float _grabMaxThreshold = 0.7f;
		[SerializeField]
		private bool _pinchEnabled = true;
		[SerializeField]
		private float _pinchMinThreshold = 0.022f;
		[SerializeField]
		private float _pinchMaxThreshold = 0.036f;

		public bool GrabEnabled
		{
			get { return _grabEnabled; }
			set { SetValue(ref _grabEnabled, value); }
		}

		public float GrabMinThreshold
		{
			get { return _grabMinThreshold; }
			set { SetValue(ref _grabMinThreshold, value); }
		}

		public float GrabMaxThreshold
		{
			get { return _grabMaxThreshold; }
			set { SetValue(ref _grabMaxThreshold, value); }
		}

		public bool PinchEnabled
		{
			get { return _pinchEnabled; }
			set { SetValue(ref _pinchEnabled, value); }
		}

		public float PinchMinThreshold
		{
			get { return _pinchMinThreshold; }
			set { SetValue(ref _pinchMinThreshold, value); }
		}

		public float PinchMaxThreshold
		{
			get { return _pinchMaxThreshold; }
			set { SetValue(ref _pinchMaxThreshold, value); }
		}
	}

	[Serializable]
	public sealed class InertiaOptions : OptionsGroupBase
	{
		[SerializeField]
		private bool _enableInertia = false;
		[SerializeField]
		private float _angularDrag = 2.5f;
		[SerializeField]
		private float _linearDrag = 2.5f;
		[SerializeField]
		private int _velocityFrames = 5;
		[SerializeField]
		private int _discardFrames = 1;
		[SerializeField]
		private bool _stopIfNotVisible;

		public bool EnableInertia
		{
			get { return _enableInertia; }
			set { SetValue(ref _enableInertia, value); }
		}

		public float AngularDrag
		{
			get { return _angularDrag; }
			set { SetValue(ref _angularDrag, value); }
		}

		public float LinearDrag
		{
			get { return _linearDrag; }
			set { SetValue(ref _linearDrag, value); }
		}

		public int VelocityFrames
		{
			get { return _velocityFrames; }
			set { SetValue(ref _velocityFrames, value); }
		}

		public int DiscardFrames
		{
			get { return _discardFrames; }
			set { SetValue(ref _discardFrames, value); }
		}

		public bool StopIfNotVisible
		{
			get { return _stopIfNotVisible; }
			set { SetValue(ref _stopIfNotVisible, value); }
		}
	}

	[Serializable]
	public sealed class FilterOptions : OptionsGroupBase
	{
		[SerializeField]
		private float _frequency = 120f;
		[SerializeField]
		private float _rotationMinCutoff = 0.2f;
		[SerializeField]
		private float _rotationBeta = 5f;
		[SerializeField]
		private float _rotationDerivativeCutoff = 1f;
		[SerializeField]
		private float _positionMinCutoff = 0.7f;
		[SerializeField]
		private float _positionBeta = 8f;
		[SerializeField]
		private float _positionDerivativeCutoff = 1f;

		public float Frequency
		{
			get { return _frequency; }
			set { SetValue(ref _frequency, value); }
		}

		public float RotationMinCutoff
		{
			get { return _rotationMinCutoff; }
			set { SetValue(ref _rotationMinCutoff, value); }
		}

		public float RotationBeta
		{
			get { return _rotationBeta; }
			set { SetValue(ref _rotationBeta, value); }
		}

		public float RotationDerivativeCutoff
		{
			get { return _rotationDerivativeCutoff; }
			set { SetValue(ref _rotationDerivativeCutoff, value); }
		}

		public float PositionMinCutoff
		{
			get { return _positionMinCutoff; }
			set { SetValue(ref _positionMinCutoff, value); }
		}

		public float PositionBeta
		{
			get { return _positionBeta; }
			set { SetValue(ref _positionBeta, value); }
		}

		public float PositionDerivativeCutoff
		{
			get { return _positionDerivativeCutoff; }
			set { SetValue(ref _positionDerivativeCutoff, value); }
		}
	}

	[Serializable]
	public sealed class HeuristicOptions : OptionsGroupBase
	{
		[SerializeField]
		private bool _enabled = true;
		[SerializeField]
		private float _radius = 0.2f;

		public bool Enabled
		{
			get { return _enabled; }
			set { SetValue(ref _enabled, value); }
		}

		public float Radius
		{
			get { return _radius; }
			set { SetValue(ref _radius, value); }
		}
	}
}