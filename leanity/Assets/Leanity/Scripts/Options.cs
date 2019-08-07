using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Leanity
{
	public enum WorkingMode { Absolute, Relative}

	[Serializable]
	public static class Options
	{
		public static event Action OnOptionsChange;

		[SerializeField]
		public static WorkingMode Mode;

		#region Sensitivity
		private static float _posScale;
		public static float PosScale
		{
			get { return _posScale; }
			set { _posScale = value; SetDirty(); }
		}

		private static float _rotScale;
		public static float RotScale
		{
			get { return _rotScale; }
			set { _rotScale = value; SetDirty(); }
		}

		private static Vector3 _axisRotScale;
		public static Vector3 AxisRotScale
		{
			get { return _axisRotScale; }
			set { _axisRotScale = value; SetDirty(); }
		}
		#endregion

		#region Camera
		private static float _pitchLimit;
		public static float PitchLimit
		{
			get { return _pitchLimit; }
			set { _pitchLimit = value; SetDirty(); }
		}
		#endregion

		#region Interaction
		private static float _grabThreshold;
		public static float GrabThreshold
		{
			get { return _grabThreshold; }
			set { _grabThreshold = value; SetDirty(); }
		}

		private static bool _grabEnabled;
		public static bool GrabEnabled
		{
			get { return _grabEnabled; }
			set { _grabEnabled = value; SetDirty(); }
		}
		#endregion

		#region Inertia
		private static bool _enableInertia;
		public static bool EnableInertia
		{
			get { return _enableInertia; }
			set { _enableInertia = value; SetDirty(); }
		}

		private static float _angularDrag;
		public static float AngularDrag
		{
			get { return _angularDrag; }
			set { _angularDrag = value; SetDirty(); }
		}

		private static float _linearDrag;
		public static float LinearDrag
		{
			get { return _linearDrag; }
			set { _linearDrag = value; SetDirty(); }
		}

		private static int _velocityFrames;
		public static int VelocityFrames
		{
			get { return _velocityFrames; }
			set { _velocityFrames = value; SetDirty(); }
		}

		private static int _discardFrames;
		public static int DiscardFrames
		{
			get { return _discardFrames; }
			set { _discardFrames = value; SetDirty(); }
		}
		#endregion

		#region Filter
		private static float _filterFrequency;
		public static float FilterFrequency
		{
			get { return _filterFrequency; }
			set { _filterFrequency = value; SetDirty(); }
		}

		private static float _rotFilterMinCutoff;
		public static float RotFilterMinCutoff
		{
			get { return _rotFilterMinCutoff; }
			set { _rotFilterMinCutoff = value; SetDirty(); }
		}

		private static float _rotFilterBeta;
		public static float RotFilterBeta
		{
			get { return _rotFilterBeta; }
			set { _rotFilterBeta = value; SetDirty(); }
		}

		private static float _rotFilterDcutoff;
		public static float RotFilterDcutoff
		{
			get { return _rotFilterDcutoff; }
			set { _rotFilterDcutoff = value; SetDirty(); }
		}

		private static float _posFilterMinCutoff;
		public static float PosFilterMinCutoff
		{
			get { return _posFilterMinCutoff; }
			set { _posFilterMinCutoff = value; SetDirty(); }
		}

		private static float _posFilterBeta;
		public static float PosFilterBeta
		{
			get { return _posFilterBeta; }
			set { _posFilterBeta = value; SetDirty(); }
		}

		private static float _posFilterDcutoff;
		public static float PosFilterDcutoff
		{
			get { return _posFilterDcutoff; }
			set { _posFilterDcutoff = value; SetDirty(); }
		}
		#endregion

		private static bool _dirty = false;
		private static bool _init = false;

		private static void SetDirty()
		{
			_dirty = true;
			if (OnOptionsChange != null)
			{
				OnOptionsChange.Invoke();
			}
		}

		public static void Save()
		{
			if (_dirty)
			{
				_dirty = false;
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
			}
		}

		public static void Load()
		{
			if (!_init)
			{
				// Working mode
				Mode = (WorkingMode)PlayerPrefs.GetInt("Mode", (int)WorkingMode.Absolute);

				// Sensitivity
				PosScale = PlayerPrefs.GetFloat("PosScale", 1f);
				RotScale = PlayerPrefs.GetFloat("RotScale", 1f);
				_axisRotScale.x = PlayerPrefs.GetFloat("AxisRotScaleX", 1f);
				_axisRotScale.y = PlayerPrefs.GetFloat("AxisRotScaleY", 1f);
				_axisRotScale.z = PlayerPrefs.GetFloat("AxisRotScaleZ", 1f);

				// Camera
				PitchLimit = PlayerPrefs.GetFloat("PitchLimit", PitchLimit);

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

				_init = true;
				_dirty = false;

				if (OnOptionsChange != null)
				{
					OnOptionsChange.Invoke();
				}
			}
		}
	}
}
