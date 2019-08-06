using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Leanity
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	public class LeapTrackingWin : HandTracking
	{
		// Private members
		private Leap.Controller _controller;
		private Leap.Frame _frame;
		private HandData _mainHandData;
		private HandData _auxHandData;

		#region Singleton
		private LeapTrackingWin()
		{
			_mainHandData = new HandData(120);
			_auxHandData = new HandData(120);

			try
			{
				if (_controller == null)
				{
					_controller = new Leap.Controller();
					//_sensor = _device.Sensor;
				}
				/*if (!_device.IsConnected)
					_device.Connect();*/
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
		}

		protected override void UpdateTracking()
		{
			if (_controller != null && _controller.IsConnected)
			{
				_frame = _controller.Frame();

				// Being pesimistic to avoid some conditionals
				MainHandData.Detected = false;
				AuxHandData.Detected = false;

				// TODO: Check coherence between left/right and hand ids
				if (_frame.Hands.Count > 0)
				{
					Leap.Hand h = _frame.Hands[0];
					UpdateHandData(ref h, ref _mainHandData);
				}

				if (_frame.Hands.Count > 1)
				{
					Leap.Hand h = _frame.Hands[1];
					UpdateHandData(ref h, ref _auxHandData);
				}
			}
		}

		protected override HandData GetMainHandData()
		{
			return _mainHandData;
		}

		protected override HandData GetAuxHandData()
		{
			return _auxHandData;
		}

		public static LeapTrackingWin SubInstance
		{
			get { return _subInstance ?? (_subInstance = new LeapTrackingWin()); }
		}
		private static LeapTrackingWin _subInstance;
		#endregion Singleton

		#region IDisposable
		public override void Dispose()
		{
			try
			{
				if (_controller != null && _controller.IsConnected)
				{
					_controller.StopConnection();
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


		private void UpdateHandData(ref Leap.Hand leapHand, ref HandData hand)
		{
			hand.Rotation = leapToUnityRotation(leapHand.Rotation);
			hand.Position = leapToUnityVector(leapHand.PalmPosition);
			hand.GrabValue = leapHand.GrabStrength;
			hand.Detected = true;
			hand.IsRight = leapHand.IsRight;
		}

		private Vector3 leapToUnityVector(Leap.Vector lv, bool invertAxis = false)
		{
			float invertValue = invertAxis ? -1f : 1f;
			float scaleFactorX = 0.001f * invertValue;
			float scaleFactorY = 0.001f * invertValue;
			float scaleFactorZ = -0.001f * invertValue;
			return new Vector3(lv.x * scaleFactorX, lv.y * scaleFactorY, lv.z * scaleFactorZ);
		}

		private Quaternion leapToUnityRotation(Leap.LeapQuaternion lq, bool invertAxis = false)
		{
			float invertValue = invertAxis ? -1f : 1f;
			return new Quaternion(-lq.x * invertValue, -lq.y * invertValue, lq.z * invertValue, lq.w);
		}
	}
#endif
}
