using System;
using UnityEngine;


namespace Leanity
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	public class LeapTrackingWin : HandTracking
	{
		// Private members
		private Leap.Controller _controller;
		private Leap.Frame _frame;
		private HandData _rightHandData;
		private HandData _leftHandData;
		private long _frameId = 0;
		private static readonly Vector3 LEAP_WORKSPACE = new Vector3(0.6f, 0.5f, 0.4f);
		private static readonly float LEAP_MM_TO_M = 0.001f;

		private LeapTrackingWin()
		{
			_rightHandData = new HandData(120);
			_leftHandData = new HandData(120);
			Options.OnOptionsChange += FilterParameterUpdate;

			Options.Load();
			FilterParameterUpdate();

			try
			{
				if (_controller == null)
				{
					_controller = new Leap.Controller();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
		}

		private void FilterParameterUpdate()
		{
			_rightHandData.SetRotationFilterParams(Options.FilterFrequency, Options.RotFilterMinCutoff, Options.RotFilterBeta, Options.RotFilterDcutoff);
			_rightHandData.SetPositionFilterParams(Options.FilterFrequency, Options.PosFilterMinCutoff, Options.PosFilterBeta, Options.PosFilterDcutoff);

			_leftHandData.SetRotationFilterParams(Options.FilterFrequency, Options.RotFilterMinCutoff, Options.RotFilterBeta, Options.RotFilterDcutoff);
			_leftHandData.SetPositionFilterParams(Options.FilterFrequency, Options.PosFilterMinCutoff, Options.PosFilterBeta, Options.PosFilterDcutoff);
		}

		protected override void UpdateTracking()
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
				}
			}
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

		#region Singleton
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


		private void UpdateHandData(Leap.Hand leapHand, ref HandData hand)
		{
			hand.Rotation = leapToUnityRotation(leapHand.Rotation);
			hand.Position = leapToUnityVector(leapHand.PalmPosition) - new Vector3(0f, LEAP_WORKSPACE.y * 0.5f + 0.1f, 0f);
			hand.GrabValue = leapHand.GrabStrength;
			hand.Detected = true;
			hand.IsRight = leapHand.IsRight;
		}

		private Vector3 leapToUnityVector(Leap.Vector lv)
		{
			return new Vector3(lv.x, lv.y, -lv.z) * LEAP_MM_TO_M;
		}

		private Quaternion leapToUnityRotation(Leap.LeapQuaternion lq)
		{
			return new Quaternion(-lq.x, -lq.y, lq.z, lq.w);
		}
	}
#endif
}
