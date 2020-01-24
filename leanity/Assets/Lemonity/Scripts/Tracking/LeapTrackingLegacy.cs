using System;
using UnityEngine;


namespace Lemonity
{
#if LEAP_LEGACY
	public class LeapTracking : HandTracking
	{
		static Vector3 Abs(Vector3 p)
		{
			return new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z));
		}
		static Vector3 Max(Vector3 p, float v)
		{
			return new Vector3(Mathf.Max(p.x, v), Mathf.Max(p.y, v), Mathf.Max(p.z, v));
		}
		static Vector3 Min(Vector3 p, float v)
		{
			return new Vector3(Mathf.Min(p.x, v), Mathf.Min(p.y, v), Mathf.Min(p.z, v));
		}
		static Vector2 Abs(Vector2 p)
		{
			return new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y));
		}
		static Vector2 Max(Vector2 p, float v)
		{
			return new Vector3(Mathf.Max(p.x, v), Mathf.Max(p.y, v));
		}
		static Vector2 Min(Vector2 p, float v)
		{
			return new Vector3(Mathf.Min(p.x, v), Mathf.Min(p.y, v));
		}

		float CubeExactSDF(Vector3 p, float side)
		{
			Vector3 q = Abs(p) -	new Vector3(side * 0.5f, side * 0.5f, side * 0.5f);
			return  Max(q, 0.0f).magnitude + Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0.0f);
		}

		public static float CylinderSDF(Vector3 p, float radius, float height)
		{
			float l = (new Vector2(p.x, p.z)).magnitude;
			Vector2 v = new Vector2(l, p.y);
			Vector2 w = new Vector2(radius, height * .5f);
			Vector2 d = Abs(v) - w;

			return Mathf.Min(Mathf.Max(d.x,d.y),0.0f) + Max(d,0.0f).magnitude;
		}


		float Union (float a, float b)
		{
			return Mathf.Max(a, b);
		}

		float Difference (float a, float b)
		{
			return Mathf.Min(-a, b);
		}

		float Intersection (float a, float b)
		{
			return Mathf.Min(a, b);
		}

float sdCappedCone(Vector3 p, float h, float r)
{
	Vector2 q = new Vector2(new Vector2(p.x, p.z).magnitude, p.y);
	Vector2 k1 = new Vector2(0f, h);
	Vector2 k2 = new Vector2(-r, 2f * h);
	Vector2 ca = new Vector2(q.x - Mathf.Min(q.x, q.y < 0f ? r : 0f), Mathf.Abs(q.y) - h);
	Vector2 cb = q - k1 + k2 * Mathf.Clamp01(Vector2.Dot(k1 - q, k2) / k2.sqrMagnitude);

	float s = (cb.x < 0f && ca.y < 0f) ? -1f : 1f;
	return s * Mathf.Sqrt(Mathf.Min(ca.sqrMagnitude, cb.sqrMagnitude));
}


		// Private members
		private Leap.Controller _controller;
		private Leap.Frame _frame;
		private HandData _rightHandData;
		private HandData _leftHandData;
		private long _frameId = 0;
		private float _lastConnectionTest = 0;
		private static readonly Vector3 LEAP_WORKSPACE = new Vector3(0.6f, 0.5f, 0.4f);
		private static readonly float LEAP_MM_TO_M = 0.001f;
		private static bool _errorCondition = false;

		private LeapTracking()
		{
			Options.Load();
			_rightHandData = new HandData(Options.FilterFrequency, Options.VelocityFrames);
			_leftHandData = new HandData(Options.FilterFrequency, Options.VelocityFrames);
			Options.OnOptionsChange += FilterParameterUpdate;

			FilterParameterUpdate();

			if (_controller == null && !_errorCondition)
			{
				try
				{
					_controller = new Leap.Controller();
				}
				catch(Exception)
				{
					_controller = null;
					_errorCondition = true;
					Debug.LogError("[Lemonity] LeapMotion DLL is not loaded. Please restart Unity.");
				}

				_lastConnectionTest = Time.realtimeSinceStartup;
			}
		}

		private void FilterParameterUpdate()
		{
			_rightHandData.SetRotationFilterParams(Options.FilterFrequency, Options.RotFilterMinCutoff, Options.RotFilterBeta, Options.RotFilterDcutoff);
			_rightHandData.SetPositionFilterParams(Options.FilterFrequency, Options.PosFilterMinCutoff, Options.PosFilterBeta, Options.PosFilterDcutoff);

			_leftHandData.SetRotationFilterParams(Options.FilterFrequency, Options.RotFilterMinCutoff, Options.RotFilterBeta, Options.RotFilterDcutoff);
			_leftHandData.SetPositionFilterParams(Options.FilterFrequency, Options.PosFilterMinCutoff, Options.PosFilterBeta, Options.PosFilterDcutoff);
		}

		protected override bool UpdateTracking()
		{
			if (_controller != null && _controller.IsConnected)
			{
				_frame = _controller.Frame();
				if (_frame.Id != _frameId)
				{
					_frameId = _frame.Id;
					// Being pesimistic to avoid some conditionals
					RightHandData.Detected = false;
					LeftHandData.Detected = false;

					foreach (var hand in _frame.Hands)
					{
						if (hand.IsRight)
						{
							UpdateHandData(hand, ref _rightHandData);
						}
						else
						{
							UpdateHandData(hand, ref _leftHandData);
						}
					}
					return true;
				}
			}
			else if (Time.realtimeSinceStartup - _lastConnectionTest > 5f && _controller != null && !_controller.IsConnected)
			{
				_lastConnectionTest = Time.realtimeSinceStartup;
				//_controller. StartConnection();
			}

			return false;
		}

		protected override HandData GetRightHandData()
		{
			return _rightHandData;
		}

		protected override HandData GetLeftHandData()
		{
			return _leftHandData;
		}

		protected override Vector3 GetWorkspace()
		{
			return LEAP_WORKSPACE;
		}

		protected override bool IsDeviceConnected()
		{
			return _controller != null && _controller.IsConnected;
		}

		protected override void ResetDevice()
		{
			_controller.Dispose();
			_controller = new Leap.Controller();
			/*
            if (_controller != null)
			{
				_controller.StopConnection();
				_controller.StartConnection();
			}
            */
		}

		#region Singleton
		public static LeapTracking SubInstance
		{
			get { return _subInstance ?? (_subInstance = new LeapTracking()); }
		}
		private static LeapTracking _subInstance;
		#endregion Singleton

		#region IDisposable
		public override void Dispose()
		{
			try
			{
				if (_controller != null && _controller.IsConnected)
				{
					_controller.Dispose();
					_subInstance = null;
					GC.Collect();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
		}
		#endregion


		private void UpdateHandData(Leap.Hand leapHand, ref HandData hand)
		{
			hand.Rotation = leapToUnityRotation(leapHand.Direction, leapHand.PalmNormal);
			hand.Position = leapToUnityVector(leapHand.PalmPosition) - new Vector3(0f, LEAP_WORKSPACE.y * 0.5f + 0.1f, 0f);
			hand.GrabValue = leapHand.GrabStrength;
			hand.Detected = true;
			hand.IsRight = leapHand.IsRight;
			hand.PinchDistance = (1f - leapHand.PinchStrength) * 50f;
		}

		private Vector3 leapToUnityVector(Leap.Vector lv)
		{
			return new Vector3(lv.x, lv.y, -lv.z) * LEAP_MM_TO_M;
		}

		private Quaternion leapToUnityRotation(Leap.Vector dir, Leap.Vector norm)
		{
			return Quaternion.LookRotation(leapToUnityVector(dir), -leapToUnityVector(norm));
		}

		protected override bool IsTrackingLibraryLoaded()
		{
			return !_errorCondition;
		}
	}
#endif
}
