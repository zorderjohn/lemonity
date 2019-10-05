using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Leanity
{
	public enum WorkingGesture { OneHand = 0, TwoHands, Hybrid}
	public enum WorkingMode { Absolute = 0, Relative}

	[Serializable]
	public static class Options
	{
		private static readonly string _prefix = "Leanity.";
		public static event Action OnOptionsLoad;
		public static event Action OnOptionsChange;

		#region General
		public static bool Enabled { get; set; }
		public static WorkingMode Mode { get; set; }
		public static WorkingGesture Gesture { get; set; }
		#endregion

		#region Sensitivity
		public static float PosScale { get; set; }
		public static bool AutoPosScaleOnLoad { get; set; }
		public static float RotScale { get; set; }
		public static float ZoomScale { get; set; }
		public static Vector3 AxisRotScale { get; set; }
		#endregion

		#region Camera
		public static float PitchLimitAngle { get; set; }
		public static bool PitchLimit { get; set; }
		public static bool RollLimit  { get; set; }
		#endregion

		#region Interaction
		public static float GrabMinThreshold  { get; set; }
		public static float GrabMaxThreshold  { get; set; }
		public static bool  GrabEnabled       { get; set; }
		public static bool  InvertAxis        { get; set; }
		public static bool  PinchEnabled      { get; set; }
		public static float PinchMinThreshold { get; set; }
		public static float PinchMaxThreshold { get; set; }
		#endregion

		#region Inertia
		public static bool EnableInertia { get; set; }
		public static float AngularDrag { get; set; }
		public static float LinearDrag { get; set; }
		public static int VelocityFrames { get; set; }
		public static int DiscardFrames { get; set; }
		#endregion

		#region Filter
		public static float FilterFrequency    { get; set; }
		public static float RotFilterMinCutoff { get; set; }
		public static float RotFilterBeta      { get; set; }
		public static float RotFilterDcutoff   { get; set; }
		public static float PosFilterMinCutoff { get; set; }
		public static float PosFilterBeta      { get; set; }
		public static float PosFilterDcutoff   { get; set; }
		#endregion

		#region Debug
		public static Color GridColor     { get { return Color.green; } }
		public static Color GrabGridColor {	get { return Color.red;   } }
		public static int NumGridLines    { get; set; }
		public static bool ShowGrid       { get; set; }
		public static bool ShowWorkspace  { get; set; }
		public static bool GestureDebug   { get; set; }

		public static float MaxGridTransparency { get; set; }

		public static float TrackingZOffset { get; set; }
		public static float HandScale { get; set; }
		public static bool ShowHandGuides { get; set; }
		#endregion

		#region FreezeHeuristic
		public static bool HeuristicEnabled { get; set; }
		public static float HeuristicRadius { get; set; }
		#endregion

		private static bool _dirty = false;
		public static bool Dirty
		{
			get { return _dirty; }
			set {
				_dirty = value;
				if (_dirty) { OnOptionsChange?.Invoke(); }
			}
		}

		private static bool _init = false;

		public static void Save()
		{
			if (Dirty)
			{
				Dirty = false;

				// General
				PlayerPrefs.SetInt(_prefix + "Enabled", Enabled ? 1 : 0);
				PlayerPrefs.SetInt(_prefix + "Gesture", (int)Gesture);
				PlayerPrefs.SetInt(_prefix + "Mode", (int)Mode);

				// Sensitivity
				PlayerPrefs.SetFloat(_prefix + "PosScale", PosScale);
				PlayerPrefs.SetFloat(_prefix + "RotScale", RotScale);
				PlayerPrefs.SetFloat(_prefix + "ZoomScale", ZoomScale);
				PlayerPrefs.SetFloat(_prefix + "AxisRotScaleX", AxisRotScale.x);
				PlayerPrefs.SetFloat(_prefix + "AxisRotScaleY", AxisRotScale.y);
				PlayerPrefs.SetFloat(_prefix + "AxisRotScaleZ", AxisRotScale.z);
				PlayerPrefs.SetInt(_prefix + "AutoPosScaleOnLoad", AutoPosScaleOnLoad ? 1 : 0);

				// Camera
				PlayerPrefs.SetFloat(_prefix + "PitchLimitAngle", PitchLimitAngle);
				PlayerPrefs.SetInt(_prefix + "PitchLimit", PitchLimit ? 1 : 0);
				PlayerPrefs.SetInt(_prefix + "RollLimit", RollLimit ? 1 : 0);

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
				PlayerPrefs.SetFloat(_prefix + "MaxTransparency", MaxGridTransparency);
				PlayerPrefs.SetFloat(_prefix + "TrackingZOffset", TrackingZOffset);
				PlayerPrefs.SetFloat(_prefix + "HandScale", HandScale);
				PlayerPrefs.SetInt(_prefix + "ShowHandGuides", ShowHandGuides ? 1 : 0);

				// Freeze Heuristic
				PlayerPrefs.SetInt(_prefix + "HeuristicEnable", HeuristicEnabled ? 1 : 0);
				PlayerPrefs.SetFloat(_prefix + "HeuristicRadius", HeuristicRadius);
			}
		}

		public static void Load()
		{
			if (!_init)
			{
				// General
				Enabled = PlayerPrefs.GetInt(_prefix + "Enabled", 1) == 1;
				Gesture = (WorkingGesture)PlayerPrefs.GetInt(_prefix + "Gesture", (int)WorkingGesture.TwoHands);
				Mode = (WorkingMode)PlayerPrefs.GetInt(_prefix + "Mode", (int)WorkingMode.Absolute);

				// Sensitivity
				PosScale = PlayerPrefs.GetFloat(_prefix + "PosScale", 1f);
				RotScale = PlayerPrefs.GetFloat(_prefix + "RotScale", 1f);
				ZoomScale = PlayerPrefs.GetFloat(_prefix + "ZoomScale", 1f);

				Vector3 axisRotScale = new Vector3();
				axisRotScale.x = PlayerPrefs.GetFloat(_prefix + "AxisRotScaleX", 1f);
				axisRotScale.y = PlayerPrefs.GetFloat(_prefix + "AxisRotScaleY", 1f);
				axisRotScale.z = PlayerPrefs.GetFloat(_prefix + "AxisRotScaleZ", 1f);
				AxisRotScale = axisRotScale;

				AutoPosScaleOnLoad = PlayerPrefs.GetInt(_prefix + "AutoPosScaleOnLoad", 1) == 1;

				// Camera
				PitchLimitAngle = PlayerPrefs.GetFloat(_prefix + "PitchLimitAngle", 75f);
				PitchLimit = PlayerPrefs.GetInt(_prefix + "PitchLimit", 1) == 1;
				RollLimit = PlayerPrefs.GetInt(_prefix + "RollLimit", 1) == 1;

				// Interaction
				GrabEnabled = PlayerPrefs.GetInt(_prefix + "GrabEnabled", 1) == 1;
				GrabMinThreshold = PlayerPrefs.GetFloat(_prefix + "GrabMinThreshold", 0.5f);
				GrabMaxThreshold = PlayerPrefs.GetFloat(_prefix + "GrabMaxThreshold", 0.6f);
				InvertAxis = PlayerPrefs.GetInt(_prefix + "InvertAxis", 0) == 1;

				PinchEnabled = PlayerPrefs.GetInt(_prefix + "PinchEnabled", 1) == 1;
				PinchMinThreshold = PlayerPrefs.GetFloat(_prefix + "PinchMinThreshold", 22f);
				PinchMaxThreshold = PlayerPrefs.GetFloat(_prefix + "PinchMaxThreshold", 40f);

				// Inertia
				EnableInertia = PlayerPrefs.GetInt(_prefix + "EnableInertia", 1) == 1;
				AngularDrag = PlayerPrefs.GetFloat(_prefix + "AngularDrag", .95f);
				LinearDrag = PlayerPrefs.GetFloat(_prefix + "LinearDrag", .95f);
				VelocityFrames = PlayerPrefs.GetInt(_prefix + "VelocityFrames", 5);
				DiscardFrames = PlayerPrefs.GetInt(_prefix + "DiscardFrames", 1);

				// Filter
				FilterFrequency = PlayerPrefs.GetFloat(_prefix + "FilterFrequency", 120f);

				RotFilterMinCutoff = PlayerPrefs.GetFloat(_prefix + "RotFilterMinCutoff", 0.2f);
				RotFilterBeta = PlayerPrefs.GetFloat(_prefix + "RotFilterBeta", 5f);
				RotFilterDcutoff = PlayerPrefs.GetFloat(_prefix + "RotFilterDcutoff", 1f);

				PosFilterMinCutoff = PlayerPrefs.GetFloat(_prefix + "PosFilterMinCutoff", 0.7f);
				PosFilterBeta = PlayerPrefs.GetFloat(_prefix + "PosFilterBeta", 8f);
				PosFilterDcutoff = PlayerPrefs.GetFloat(_prefix + "PosFilterDcutoff", 1f);

				// Debug
				NumGridLines = PlayerPrefs.GetInt(_prefix + "NumGridLines", 6);
				ShowGrid = PlayerPrefs.GetInt(_prefix + "ShowGrid", 1) == 1;
				ShowWorkspace = PlayerPrefs.GetInt(_prefix + "ShowWorkspace", 1) == 1;
				GestureDebug = PlayerPrefs.GetInt(_prefix + "GestureDebug", 1) == 1;
				MaxGridTransparency = PlayerPrefs.GetFloat(_prefix + "MaxTransparency", .8f);
				TrackingZOffset = PlayerPrefs.GetFloat(_prefix + "TrackingZOffset", 1f);
				HandScale = PlayerPrefs.GetFloat(_prefix + "HandScale", 1f);
				ShowHandGuides = PlayerPrefs.GetInt(_prefix + "ShowHandGuides", 1) == 1;

				// Freeze Heuristic
				HeuristicEnabled = PlayerPrefs.GetInt(_prefix + "HeuristicEnabled", 1) == 1;
				HeuristicRadius = PlayerPrefs.GetFloat(_prefix + "HeuristicRadius", 0.2f);

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
