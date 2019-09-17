using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Leanity
{
	public enum WorkingGesture { OneHand, TwoHands}
	public enum WorkingMode { Absolute, Relative}

	[Serializable]
	public static class Options
	{
		private static readonly string _prefix = "Leanity.";
		public static event Action OnOptionsLoad;
		public static event Action OnOptionsChange;

		[SerializeField]
		private static WorkingMode _mode;
		public static WorkingMode Mode
		{
			get { return _mode; }
			set { SetFieldValue(ref _mode, value); }
		}

		[SerializeField]
		private static WorkingGesture _gesture;
		public static WorkingGesture Gesture
		{
			get { return _gesture; }
			set { SetFieldValue(ref _gesture, value); }
		}

		#region Sensitivity
		private static float _posScale;
		public static float PosScale
		{
			get { return _posScale; }
			set
			{
				if (SetFieldValue(ref _posScale, value))
				{
					//_gridFade.FadeIn(.5f);
					//_gridFade.FadeOutAfterTime(1f, 1f);
				}
			}
		}

		private static float _rotScale;
		public static float RotScale
		{
			get { return _rotScale; }
			set { SetFieldValue(ref _rotScale, value); }
		}

		private static float _zoomScale;
		public static float ZoomScale
		{
			get { return _zoomScale; }
			set { SetFieldValue(ref _zoomScale, value); }
		}

		private static Vector3 _axisRotScale;
		public static Vector3 AxisRotScale
		{
			get { return _axisRotScale; }
			set { SetComplexFieldValue(ref _axisRotScale, value); }
		}
		#endregion

		#region Camera
		private static float _pitchLimit;
		public static float PitchLimit
		{
			get { return _pitchLimit; }
			set { SetFieldValue(ref _pitchLimit, value); }
		}

		#endregion

		#region Interaction
		private static float _grabMinThreshold;
		public static float GrabMinThreshold
		{
			get { return _grabMinThreshold; }
			set { SetFieldValue(ref _grabMinThreshold, value); }
		}

		private static float _grabMaxThreshold;
		public static float GrabMaxThreshold
		{
			get { return _grabMaxThreshold; }
			set { SetFieldValue(ref _grabMaxThreshold, value); }
		}

		private static bool _grabEnabled;
		public static bool GrabEnabled
		{
			get { return _grabEnabled; }
			set { SetFieldValue(ref _grabEnabled, value); }
		}

		private static bool _invertAxis;
		public static bool InvertAxis
		{
			get { return _invertAxis; }
			set { SetFieldValue(ref _invertAxis, value); }
		}

		private static bool _pinchEnabled;
		public static bool PinchEnabled
		{
			get { return _pinchEnabled; }
			set { SetFieldValue(ref _pinchEnabled, value); }
		}

		private static float _pinchMinThreshold;
		public static float PinchMinThreshold
		{
			get { return _pinchMinThreshold; }
			set { SetFieldValue(ref _pinchMinThreshold, value); }
		}

		private static float _pinchMaxThreshold;
		public static float PinchMaxThreshold
		{
			get { return _pinchMaxThreshold; }
			set { SetFieldValue(ref _pinchMaxThreshold, value); }
		}

		#endregion

		#region Inertia
		private static bool _enableInertia;
		public static bool EnableInertia
		{
			get { return _enableInertia; }
			set { SetFieldValue(ref _enableInertia, value); }
		}

		private static float _angularDrag;
		public static float AngularDrag
		{
			get { return _angularDrag; }
			set { SetFieldValue(ref _angularDrag, value); }
		}

		private static float _linearDrag;
		public static float LinearDrag
		{
			get { return _linearDrag; }
			set { SetFieldValue(ref _linearDrag, value); }
		}

		private static int _velocityFrames;
		public static int VelocityFrames
		{
			get { return _velocityFrames; }
			set { SetFieldValue(ref _velocityFrames, value); }
		}

		private static int _discardFrames;
		public static int DiscardFrames
		{
			get { return _discardFrames; }
			set { SetFieldValue(ref _discardFrames, value); }
		}
		#endregion

		#region Filter
		private static float _filterFrequency;
		public static float FilterFrequency
		{
			get { return _filterFrequency; }
			set { SetFieldValue(ref _filterFrequency, value); }
		}

		private static float _rotFilterMinCutoff;
		public static float RotFilterMinCutoff
		{
			get { return _rotFilterMinCutoff; }
			set { SetFieldValue(ref _rotFilterMinCutoff, value); }
		}

		private static float _rotFilterBeta;
		public static float RotFilterBeta
		{
			get { return _rotFilterBeta; }
			set { SetFieldValue(ref _rotFilterBeta, value); }
		}

		private static float _rotFilterDcutoff;
		public static float RotFilterDcutoff
		{
			get { return _rotFilterDcutoff; }
			set { SetFieldValue(ref _rotFilterDcutoff, value); }
		}

		private static float _posFilterMinCutoff;
		public static float PosFilterMinCutoff
		{
			get { return _posFilterMinCutoff; }
			set { SetFieldValue(ref _posFilterMinCutoff, value); }
		}

		private static float _posFilterBeta;
		public static float PosFilterBeta
		{
			get { return _posFilterBeta; }
			set { SetFieldValue(ref _posFilterBeta, value); }
		}

		private static float _posFilterDcutoff;
		public static float PosFilterDcutoff
		{
			get { return _posFilterDcutoff; }
			set { SetFieldValue(ref _posFilterDcutoff, value); }
		}


		public static bool Dirty { get; private set; } = false;
		#endregion

		#region Debug

		public static Color GridColor
		{
			get { return new Color(0f, 1f, 0f); }
		}

		public static Color GrabGridColor
		{
			get { return Color.red; }
		}


		private static int _numGridLines;
		public static int NumGridLines
		{
			get { return _numGridLines; }
			set { SetFieldValue(ref _numGridLines, value); }
		}

		private static bool _showGrid;
		public static bool ShowGrid
		{
			get { return _showGrid; }
			set { SetFieldValue(ref _showGrid, value); }
		}

		private static bool _showWorkspace;
		public static bool ShowWorkspace
		{
			get { return _showWorkspace; }
			set { SetFieldValue(ref _showWorkspace, value); }
		}

		private static bool _gestureDebug;
		public static bool GestureDebug
		{
			get { return _gestureDebug; }
			set { SetFieldValue(ref _gestureDebug, value); }
		}

		private static ValueFade _gridFade = new ValueFade();
		public static void GridFadeIn()
		{
			_gridFade.FadeIn();
		}

		public static void GridFadeOut()
		{
			_gridFade.FadeOut();
		}

		public static float GridTransparency
		{
			get { return _gridFade.Value; }
		}

		public static bool GridVisible
		{
			get { return _gridFade.Value > _gridFade.MinValue; }
		}

		public static float MaxGridTransparency
		{
			get { return _gridFade.MaxValue; }
			set { _gridFade.MaxValue = value; }
		}

		private static float _trackingZOffset;
		public static float TrackingZOffset
		{
			get { return _trackingZOffset; }
			set { SetFieldValue(ref _trackingZOffset, value); }
		}

		#endregion

		private static bool _init = false;

		private static bool SetFieldValue<T>(ref T field, T newValue) where T : IComparable
		{
			if (!field.Equals(newValue))
			{
				field = newValue;
				Dirty = true;
				OnOptionsChange?.Invoke();
				return true;
			}
			return false;
		}

		private static bool SetComplexFieldValue<T>(ref T field, T newValue) where T : IEquatable<T>
		{
			if (!field.Equals(newValue))
			{
				field = newValue;
				Dirty = true;
				OnOptionsChange?.Invoke();
				return true;
			}
			return false;
		}

		public static void Save()
		{
			if (Dirty)
			{
				Dirty = false;
				// Working gesture
				PlayerPrefs.SetInt(_prefix + "Gesture", (int)Gesture);

				// Working mode
				PlayerPrefs.SetInt(_prefix + "Mode", (int)Mode);

				// Sensitivity
				PlayerPrefs.SetFloat(_prefix + "PosScale", PosScale);
				PlayerPrefs.SetFloat(_prefix + "RotScale", RotScale);
				PlayerPrefs.SetFloat(_prefix + "ZoomScale", ZoomScale);
				PlayerPrefs.SetFloat(_prefix + "AxisRotScaleX", AxisRotScale.x);
				PlayerPrefs.SetFloat(_prefix + "AxisRotScaleY", AxisRotScale.y);
				PlayerPrefs.SetFloat(_prefix + "AxisRotScaleZ", AxisRotScale.z);

				// Camera
				PlayerPrefs.SetFloat(_prefix + "PitchLimit", PitchLimit);

				// Interaction
				PlayerPrefs.SetInt(_prefix + "GrabEnabled", GrabEnabled ? 1 : 0);
				PlayerPrefs.SetFloat(_prefix + "GrabMinThreshold", GrabMinThreshold);
				PlayerPrefs.SetFloat(_prefix + "GrabMaxThreshold", GrabMaxThreshold);
				PlayerPrefs.SetInt(_prefix + "InvertAxis", InvertAxis ? 1 : 0);
				PlayerPrefs.SetInt(_prefix + "PinchEnabled", PinchEnabled ? 1 : 0);
				PlayerPrefs.SetFloat(_prefix + "PinchMinThreshold", PinchMinThreshold);
				PlayerPrefs.SetFloat(_prefix + "PinchMaxThreshold", PinchMaxThreshold);

				// Inertia
				PlayerPrefs.SetInt(_prefix + "EnableInertia", EnableInertia ? 1 : 0);
				PlayerPrefs.SetFloat(_prefix + "AngularDrag", AngularDrag);
				PlayerPrefs.SetFloat(_prefix + "LinearDrag", LinearDrag);
				PlayerPrefs.SetInt(_prefix + "VelocityFrames", VelocityFrames);
				PlayerPrefs.SetInt(_prefix + "DiscardFrames", DiscardFrames);

				// Filter
				PlayerPrefs.SetFloat(_prefix + "FilterFrequency", FilterFrequency);
				PlayerPrefs.SetFloat(_prefix + "RotFilterMinCutoff", RotFilterMinCutoff);
				PlayerPrefs.SetFloat(_prefix + "RotFilterBeta", RotFilterBeta);
				PlayerPrefs.SetFloat(_prefix + "RotFilterDcutoff", RotFilterDcutoff);
				PlayerPrefs.SetFloat(_prefix + "PosFilterMinCutoff", PosFilterMinCutoff);
				PlayerPrefs.SetFloat(_prefix + "PosFilterBeta", PosFilterBeta);
				PlayerPrefs.SetFloat(_prefix + "PosFilterDcutoff", PosFilterDcutoff);

				// Debug
				PlayerPrefs.SetInt(_prefix + "NumGridLines", NumGridLines);
				PlayerPrefs.SetInt(_prefix + "ShowGrid", ShowGrid ? 1 : 0);
				PlayerPrefs.SetInt(_prefix + "ShowWorkspace", ShowWorkspace ? 1 : 0);
				PlayerPrefs.SetInt(_prefix + "GestureDebug", GestureDebug ? 1 : 0);
				PlayerPrefs.SetFloat(_prefix + "MaxTransparency", _gridFade.MaxValue);
				PlayerPrefs.SetFloat(_prefix + "TrackingZOffset", TrackingZOffset);

			}
		}

		public static void Load()
		{
			if (!_init)
			{
				// Working gesture
				Gesture = (WorkingGesture)PlayerPrefs.GetInt(_prefix + "Gesture", (int)WorkingGesture.OneHand);

				// Working mode
				Mode = (WorkingMode)PlayerPrefs.GetInt(_prefix + "Mode", (int)WorkingMode.Absolute);

				// Sensitivity
				PosScale = PlayerPrefs.GetFloat(_prefix + "PosScale", 1f);
				RotScale = PlayerPrefs.GetFloat(_prefix + "RotScale", 1f);
				ZoomScale = PlayerPrefs.GetFloat(_prefix + "ZoomScale", 1f);
				_axisRotScale.x = PlayerPrefs.GetFloat(_prefix + "AxisRotScaleX", 1f);
				_axisRotScale.y = PlayerPrefs.GetFloat(_prefix + "AxisRotScaleY", 1f);
				_axisRotScale.z = PlayerPrefs.GetFloat(_prefix + "AxisRotScaleZ", 1f);

				// Camera
				PitchLimit = PlayerPrefs.GetFloat(_prefix + "PitchLimit", 75f);

				// Interaction
				GrabEnabled = PlayerPrefs.GetInt(_prefix + "GrabEnabled", 1) == 1;
				GrabMinThreshold = PlayerPrefs.GetFloat(_prefix + "GrabMinThreshold", 0.5f);
				GrabMaxThreshold = PlayerPrefs.GetFloat(_prefix + "GrabMaxThreshold", 0.6f);
				InvertAxis = PlayerPrefs.GetInt(_prefix + "InvertAxis", 1) == 1;

				PinchEnabled = PlayerPrefs.GetInt(_prefix + "PinchEnabled", 1) == 1;
				PinchMinThreshold = PlayerPrefs.GetFloat(_prefix + "PinchMinThreshold", 0.5f);
				PinchMaxThreshold = PlayerPrefs.GetFloat(_prefix + "PinchMaxThreshold", 0.6f);

				// Inertia
				EnableInertia = PlayerPrefs.GetInt(_prefix + "EnableInertia", 1) == 1;
				AngularDrag = PlayerPrefs.GetFloat(_prefix + "AngularDrag", .95f);
				LinearDrag = PlayerPrefs.GetFloat(_prefix + "LinearDrag", .95f);
				VelocityFrames = PlayerPrefs.GetInt(_prefix + "VelocityFrames", 5);
				DiscardFrames = PlayerPrefs.GetInt(_prefix + "DiscardFrames", 5);

				// Filter
				FilterFrequency = PlayerPrefs.GetFloat(_prefix + "FilterFrequency", 120f);
				RotFilterMinCutoff = PlayerPrefs.GetFloat(_prefix + "RotFilterMinCutoff", 1f);
				RotFilterBeta = PlayerPrefs.GetFloat(_prefix + "RotFilterBeta", 0f);
				RotFilterDcutoff = PlayerPrefs.GetFloat(_prefix + "RotFilterDcutoff", 1f);
				PosFilterMinCutoff = PlayerPrefs.GetFloat(_prefix + "PosFilterMinCutoff", 1f);
				PosFilterBeta = PlayerPrefs.GetFloat(_prefix + "PosFilterBeta", 0f);
				PosFilterDcutoff = PlayerPrefs.GetFloat(_prefix + "PosFilterDcutoff", 1f);

				// Debug
				NumGridLines = PlayerPrefs.GetInt(_prefix + "NumGridLines", 6);
				ShowGrid = PlayerPrefs.GetInt(_prefix + "ShowGrid", 1) == 1;
				ShowWorkspace = PlayerPrefs.GetInt(_prefix + "ShowWorkspace", 1) == 1;
				GestureDebug = PlayerPrefs.GetInt(_prefix + "GestureDebug", 1) == 1;
				_gridFade.MaxValue = PlayerPrefs.GetFloat(_prefix + "MaxTransparency", .8f);
				TrackingZOffset = PlayerPrefs.GetFloat(_prefix + "TrackingZOffset", 1f);

				_init = true;
				Dirty = false;

				if (OnOptionsLoad != null)
				{
					OnOptionsLoad.Invoke();
				}
				if (OnOptionsChange != null)
				{
					OnOptionsChange.Invoke();
				}
			}
		}
	}
}
