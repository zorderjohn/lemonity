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
		public static event Action OnOptionsLoad;
		public static event Action OnOptionsChange;

		[SerializeField]
		private static WorkingMode _mode;
		public static WorkingMode Mode
		{
			get { return _mode; }
			set { SetFieldValue(ref _mode, value);}
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
			set { SetFieldValue(ref _posScale, value); }
		}

		private static float _rotScale;
		public static float RotScale
		{
			get { return _rotScale; }
			set { SetFieldValue(ref _rotScale, value); }
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

		private static bool _gestureDebug;
		public static bool GestureDebug
		{
			get { return _gestureDebug; }
			set { SetFieldValue(ref _gestureDebug, value); }
		}
		#endregion

		private static bool _init = false;

		private static void SetFieldValue<T>(ref T field, T newValue) where T : IComparable
		{
			if (!field.Equals(newValue))
			{
				field = newValue;
				Dirty = true;
				OnOptionsChange?.Invoke();
			}
		}

		private static void SetComplexFieldValue<T>(ref T field, T newValue) where T : IEquatable<T>
		{
			if (!field.Equals(newValue))
			{
				field = newValue;
				Dirty = true;
				OnOptionsChange?.Invoke();
			}
		}

		public static void Save()
		{
			if (Dirty)
			{
				Dirty = false;
				// Working gesture
				PlayerPrefs.SetInt("Gesture", (int)Gesture);

				// Working mode
				PlayerPrefs.SetInt("Mode", (int)Mode);

				// Sensitivity
				PlayerPrefs.SetFloat("PosScale", PosScale);
				PlayerPrefs.SetFloat("RotScale", RotScale);
				PlayerPrefs.SetFloat("AxisRotScaleX", AxisRotScale.x);
				PlayerPrefs.SetFloat("AxisRotScaleY", AxisRotScale.y);
				PlayerPrefs.SetFloat("AxisRotScaleZ", AxisRotScale.z);

				// Camera
				PlayerPrefs.SetFloat("PitchLimit", PitchLimit);

				// Interaction
				PlayerPrefs.SetInt("GrabEnabled", GrabEnabled ? 1 : 0);
				PlayerPrefs.SetFloat("GrabMinThreshold", GrabMinThreshold);
				PlayerPrefs.SetFloat("GrabMaxThreshold", GrabMaxThreshold);
				PlayerPrefs.SetInt("InvertAxis", InvertAxis ? 1 : 0);

				// Inertia
				PlayerPrefs.SetInt("EnableInertia", EnableInertia ? 1 : 0);
				PlayerPrefs.SetFloat("AngularDrag", AngularDrag);
				PlayerPrefs.SetFloat("LinearDrag", LinearDrag);
				PlayerPrefs.SetInt("VelocityFrames", VelocityFrames);
				PlayerPrefs.SetInt("DiscardFrames", DiscardFrames);

				// Filter
				PlayerPrefs.SetFloat("FilterFrequency", FilterFrequency);
				PlayerPrefs.SetFloat("RotFilterMinCutoff", RotFilterMinCutoff);
				PlayerPrefs.SetFloat("RotFilterBeta", RotFilterBeta);
				PlayerPrefs.SetFloat("RotFilterDcutoff", RotFilterDcutoff);
				PlayerPrefs.SetFloat("PosFilterMinCutoff", PosFilterMinCutoff);
				PlayerPrefs.SetFloat("PosFilterBeta", PosFilterBeta);
				PlayerPrefs.SetFloat("PosFilterDcutoff", PosFilterDcutoff);

				// Debug
				PlayerPrefs.SetInt("NumGridLines", NumGridLines);
				PlayerPrefs.SetInt("ShowGrid", ShowGrid ? 1 : 0);
				PlayerPrefs.SetInt("GestureDebug", GestureDebug ? 1 : 0);
			}
		}

		public static void Load()
		{
			if (!_init)
			{
				// Working gesture
				Gesture = (WorkingGesture)PlayerPrefs.GetInt("Gesture", (int)WorkingGesture.OneHand);

				// Working mode
				Mode = (WorkingMode)PlayerPrefs.GetInt("Mode", (int)WorkingMode.Absolute);

				// Sensitivity
				PosScale = PlayerPrefs.GetFloat("PosScale", 1f);
				RotScale = PlayerPrefs.GetFloat("RotScale", 1f);
				_axisRotScale.x = PlayerPrefs.GetFloat("AxisRotScaleX", 1f);
				_axisRotScale.y = PlayerPrefs.GetFloat("AxisRotScaleY", 1f);
				_axisRotScale.z = PlayerPrefs.GetFloat("AxisRotScaleZ", 1f);

				// Camera
				PitchLimit = PlayerPrefs.GetFloat("PitchLimit", 75f);

				// Interaction
				GrabEnabled = PlayerPrefs.GetInt("GrabEnabled", 1) == 1;
				GrabMinThreshold = PlayerPrefs.GetFloat("GrabMinThreshold", 0.5f);
				GrabMaxThreshold = PlayerPrefs.GetFloat("GrabMaxThreshold", 0.6f);
				InvertAxis = PlayerPrefs.GetInt("InvertAxis", 1) == 1;

				// Inertia
				EnableInertia = PlayerPrefs.GetInt("EnableInertia", 1) == 1;
				AngularDrag = PlayerPrefs.GetFloat("AngularDrag", .95f);
				LinearDrag = PlayerPrefs.GetFloat("LinearDrag", .95f);
				VelocityFrames = PlayerPrefs.GetInt("VelocityFrames", 5);
				DiscardFrames = PlayerPrefs.GetInt("DiscardFrames", 5);

				// Filter
				FilterFrequency = PlayerPrefs.GetFloat("FilterFrequency", 120f);
				RotFilterMinCutoff = PlayerPrefs.GetFloat("RotFilterMinCutoff", 1f);
				RotFilterBeta = PlayerPrefs.GetFloat("RotFilterBeta", 0f);
				RotFilterDcutoff = PlayerPrefs.GetFloat("RotFilterDcutoff", 1f);
				PosFilterMinCutoff = PlayerPrefs.GetFloat("PosFilterMinCutoff", 1f);
				PosFilterBeta = PlayerPrefs.GetFloat("PosFilterBeta", 0f);
				PosFilterDcutoff = PlayerPrefs.GetFloat("PosFilterDcutoff", 1f);

				// Debug
				NumGridLines = PlayerPrefs.GetInt("NumGridLines", 6);
				ShowGrid = PlayerPrefs.GetInt("ShowGrid", 1) == 1;
				GestureDebug = PlayerPrefs.GetInt("GestureDebug", 1) == 1;

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
