using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Leanity
{
	public enum WorkingGesture { Disabled = 0, OneHand, TwoHands, Hybrid, Orbit}
	public enum WorkingMode    { Absolute = 0, Relative}

	[Serializable]
	public static class Options
	{
		private static readonly string _prefix = "Leanity.";
		public static event Action OnOptionsLoad;
		public static event Action OnOptionsChange;

		#region General
		public static WorkingMode    Mode    { get; set; }
		public static WorkingGesture Gesture { get; set; }
		#endregion

		#region Sensitivity
		public static float   PosScale           { get; set; }
		public static bool    AutoPosScaleOnLoad { get; set; }
		public static float   RotScale           { get; set; }
		public static float   ZoomScale          { get; set; }
		public static Vector3 AxisRotScale       { get; set; }
		#endregion

		#region Camera
		public static float PitchLimitAngle { get; set; }
		public static bool  PitchLimit      { get; set; }
		public static bool  RollLimit       { get; set; }
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
		public static bool  EnableInertia     { get; set; }
		public static float AngularDrag       { get; set; }
		public static float LinearDrag        { get; set; }
		public static int   VelocityFrames    { get; set; }
		public static int   DiscardFrames     { get; set; }
		public static bool  StopIfNotVisible  { get; set; }
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

		#region Visuals
		public static Color GridColor     { get { return Color.green; } }
		public static Color GrabGridColor {	get { return Color.red;   } }
		public static int   NumGridLines  { get; set; }
		public static bool  ShowGrid      { get; set; }
		public static bool  ShowWorkspace { get; set; }
		public static bool  GestureDebug  { get; set; }

		public static float MaxGridTransparency { get; set; }

		public static float TrackingZOffset { get; set; }
		public static float HandScale       { get; set; }
		public static bool  ShowHandGuides  { get; set; }
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
				SaveValue("Gesture", (int)Gesture);
				SaveValue("Mode", (int)Mode);

				// Sensitivity
				SaveValue("PosScale", PosScale);
				SaveValue("RotScale", RotScale);
				SaveValue("ZoomScale", ZoomScale);
				SaveValue("AxisRotScaleX", AxisRotScale.x);
				SaveValue("AxisRotScaleY", AxisRotScale.y);
				SaveValue("AxisRotScaleZ", AxisRotScale.z);
				SaveValue("AutoPosScaleOnLoad", AutoPosScaleOnLoad);

				// Camera
				SaveValue("PitchLimitAngle", PitchLimitAngle);
				SaveValue("PitchLimit", PitchLimit);
				SaveValue("RollLimit", RollLimit);

				// Interaction
				SaveValue("GrabEnabled", GrabEnabled);
				SaveValue("GrabMinThreshold", GrabMinThreshold);
				SaveValue("GrabMaxThreshold", GrabMaxThreshold);
				SaveValue("InvertAxis", InvertAxis);
				SaveValue("PinchEnabled", PinchEnabled);
				SaveValue("PinchMinThreshold", PinchMinThreshold);
				SaveValue("PinchMaxThreshold", PinchMaxThreshold);

				// Inertia
				SaveValue("EnableInertia", EnableInertia);
				SaveValue("AngularDrag", AngularDrag);
				SaveValue("LinearDrag", LinearDrag);
				SaveValue("VelocityFrames", VelocityFrames);
				SaveValue("DiscardFrames", DiscardFrames);

				// Filter
				SaveValue("FilterFrequency", FilterFrequency);
				SaveValue("RotFilterMinCutoff", RotFilterMinCutoff);
				SaveValue("RotFilterBeta", RotFilterBeta);
				SaveValue("RotFilterDcutoff", RotFilterDcutoff);
				SaveValue("PosFilterMinCutoff", PosFilterMinCutoff);
				SaveValue("PosFilterBeta", PosFilterBeta);
				SaveValue("PosFilterDcutoff", PosFilterDcutoff);

				// Visuals
				SaveValue("NumGridLines", NumGridLines);
				SaveValue("ShowGrid", ShowGrid);
				SaveValue("ShowWorkspace", ShowWorkspace);
				SaveValue("GestureDebug", GestureDebug);
				SaveValue("MaxTransparency", MaxGridTransparency);
				SaveValue("TrackingZOffset", TrackingZOffset);
				SaveValue("HandScale", HandScale);
				SaveValue("ShowHandGuides", ShowHandGuides);
				SaveValue("StopIfNotVisible", StopIfNotVisible);

				// Freeze Heuristic
				SaveValue("HeuristicEnable", HeuristicEnabled);
				SaveValue("HeuristicRadius", HeuristicRadius);
			}
		}

		public static void Load()
		{
			if (!_init)
			{
				// General
				Gesture = (WorkingGesture)Load("Gesture", (int)WorkingGesture.TwoHands);
				Mode = (WorkingMode)Load("Mode", (int)WorkingMode.Absolute);

				// Sensitivity
				PosScale = Load("PosScale", 1f);
				RotScale = Load("RotScale", 1f);
				ZoomScale = Load("ZoomScale", 1f);

				Vector3 axisRotScale = new Vector3();
				axisRotScale.x = Load("AxisRotScaleX", 1f);
				axisRotScale.y = Load("AxisRotScaleY", 1f);
				axisRotScale.z = Load("AxisRotScaleZ", 1f);
				AxisRotScale = axisRotScale;

				AutoPosScaleOnLoad = Load("AutoPosScaleOnLoad", true);

				// Camera
				PitchLimitAngle = Load("PitchLimitAngle", 75f);
				PitchLimit = Load("PitchLimit", true);
				RollLimit = Load("RollLimit", true);

				// Interaction
				GrabEnabled = Load("GrabEnabled", true);
				GrabMinThreshold = Load("GrabMinThreshold", 0.5f);
				GrabMaxThreshold = Load("GrabMaxThreshold", 0.6f);
				InvertAxis = Load("InvertAxis", false);

				PinchEnabled = Load("PinchEnabled", true);
				PinchMinThreshold = Load("PinchMinThreshold", 22f);
				PinchMaxThreshold = Load("PinchMaxThreshold", 40f);

				// Inertia
				EnableInertia = Load("EnableInertia", true);
				AngularDrag = Load("AngularDrag", .95f);
				LinearDrag = Load("LinearDrag", .95f);
				VelocityFrames = Load("VelocityFrames", 5);
				DiscardFrames = Load("DiscardFrames", 1);
				StopIfNotVisible = Load("StopIfNotVisible", true);

				// Filter
				FilterFrequency = Load("FilterFrequency", 120f);

				RotFilterMinCutoff = Load("RotFilterMinCutoff", 0.2f);
				RotFilterBeta = Load("RotFilterBeta", 5f);
				RotFilterDcutoff = Load("RotFilterDcutoff", 1f);

				PosFilterMinCutoff = Load("PosFilterMinCutoff", 0.7f);
				PosFilterBeta = Load("PosFilterBeta", 8f);
				PosFilterDcutoff = Load("PosFilterDcutoff", 1f);

				// Visuals
				NumGridLines = Load("NumGridLines", 6);
				ShowGrid = Load("ShowGrid", true);
				ShowWorkspace = Load("ShowWorkspace", true);
				GestureDebug = Load("GestureDebug", true);
				MaxGridTransparency = Load("MaxTransparency", .8f);
				TrackingZOffset = Load("TrackingZOffset", 1f);
				HandScale = Load("HandScale", 1f);
				ShowHandGuides = Load("ShowHandGuides", true);

				// Freeze Heuristic
				HeuristicEnabled = Load("HeuristicEnabled", true);
				HeuristicRadius = Load("HeuristicRadius", 0.2f);

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

		private static bool Load(string name, bool defaultValue)
		{
			return PlayerPrefs.GetInt(_prefix + name, defaultValue ? 1 : 0) == 1;
		}

		private static int Load(string name, int defaultValue)
		{
			return PlayerPrefs.GetInt(_prefix + name, defaultValue);
		}

		private static float Load(string name, float defaultValue)
		{
			return PlayerPrefs.GetFloat(_prefix + name, defaultValue);
		}

		private static void SaveValue(string name, bool value)
		{
			PlayerPrefs.SetInt(_prefix + name, value ? 1 : 0);
		}

		private static void SaveValue(string name, int value)
		{
			PlayerPrefs.SetInt(_prefix + name, value);
		}

		private static void SaveValue(string name, float value)
		{
			PlayerPrefs.SetFloat(_prefix + name, value);
		}
	}
}
