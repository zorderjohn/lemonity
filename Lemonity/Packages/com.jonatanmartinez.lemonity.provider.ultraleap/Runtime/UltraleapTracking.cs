using System;
using Leap;
using UnityEngine;
using Lemonity.Core;

namespace Lemonity.Provider.Ultraleap
{
	public class UltraLeapTracking : HandTracking
	{
		private static readonly Vector3 LEAP_WORKSPACE = new Vector3(0.6f, 0.5f, 0.4f);
		private static readonly Vector3 LEAP_POSITION_OFFSET = new Vector3(0f, LEAP_WORKSPACE.y * 0.5f + 0.1f, 0f);
		private const float RECOVERY_INTERVAL = 5f;
		private static bool _errorCondition;

		private readonly object _eventStateLock = new object();
		private readonly FilterOptions _filterOptions;
		private readonly InertiaOptions _inertiaOptions;
		private Controller _controller;
		private Frame _frame;
		private HandData _rightHandData;
		private HandData _leftHandData;
		private long _frameId;
		private float _lastConnectionTest;
		private bool _lastLoggedConnected;
		private int _lastLoggedHandCount = -1;
		private bool _controllerEventsAttached;
		private string _lastPolicyState = "policy=unknown";
		private string _lastRuntimeEvent = "event=none";
		private long _lastFrameEventId;
		private long _lastFrameEventTimestamp;

		private UltraLeapTracking(FilterOptions filterOptions, InertiaOptions inertiaOptions)
		{
			_filterOptions = filterOptions;
			_inertiaOptions = inertiaOptions;
			Options.Load();
			_rightHandData = new HandData(_filterOptions.Frequency, _inertiaOptions.VelocityFrames);
			_leftHandData = new HandData(_filterOptions.Frequency, _inertiaOptions.VelocityFrames);
			Options.OnOptionsChange += FilterParameterUpdate;

			FilterParameterUpdate();
			TryEnsureController(); 
		}

		private bool TryEnsureController()
		{
			if (_controller != null)
			{
				return true;
			}

			if (_errorCondition)
			{
				return false;
			}

			try
			{
				_controller = new Controller();
				AttachControllerEvents();
				RequestControllerPolicies();
				_lastConnectionTest = Time.realtimeSinceStartup;

				// After a domain reload the underlying Connection is shared and already running.
				// The Device event will NOT fire again for existing devices, so we must subscribe
				// to them proactively to receive frames in MultipleDevicesAware mode.
				if (_controller.IsServiceConnected && _controller.Devices != null && _controller.Devices.Count > 0)
				{
					foreach (var existingDevice in _controller.Devices)
					{
						_controller.SubscribeToDeviceEvents(existingDevice);
						RequestControllerPolicies(existingDevice);
					}
					
					StatusLogger.Instance.Log("Ultraleap controller created and reconnected to " + _controller.Devices.Count + " existing device(s).", true);
				}
				else
				{
					StatusLogger.Instance.Log("Ultraleap controller created. Requested background frames and waiting for service connection.", true);
				}
				return true;
			}
			catch (Exception ex)
			{
				_errorCondition = true;
				_controller = null;
				StatusLogger.Instance.Error("Ultraleap controller could not be created. " + ex);
				return false;
			}
		}

		private void SetConnectedStatus(string status, int handCount)
		{
			bool forceLog = !_lastLoggedConnected || handCount != _lastLoggedHandCount;
			_lastLoggedConnected = true;
			_lastLoggedHandCount = handCount;
			StatusLogger.Instance.Log(status, forceLog);
		}

		private void SetDisconnectedStatus(string status, bool forceLog = false)
		{
			_lastLoggedConnected = false;
			_lastLoggedHandCount = -1;
			StatusLogger.Instance.Log(status, forceLog);
		}

		private void RequestControllerPolicies(Device device = null)
		{
			if (_controller == null)
			{
				return;
			}

			try
			{
				_controller.SetPolicy(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES, device);
				_controller.SetPolicy(Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME, device);
				_controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP, device);
				_controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD, device);
			}
			catch (Exception ex)
			{
				StatusLogger.Instance.Warning("Could not request Ultraleap policies. " + ex.Message);
			}
		}

		private void AttachControllerEvents()
		{
			if (_controller == null || _controllerEventsAttached)
			{
				return;
			}

			_controller.Connect += HandleControllerConnect;
			_controller.Disconnect += HandleControllerDisconnect;
			_controller.Device += HandleControllerDevice;
			_controller.DeviceLost += HandleControllerDeviceLost;
			_controller.DeviceFailure += HandleControllerDeviceFailure;
			_controller.PolicyChange += HandleControllerPolicyChange;
			_controller.FrameReady += HandleControllerFrameReady;
			_controller.LogMessage += HandleControllerLogMessage;
			_controllerEventsAttached = true;
		}

		private void DetachControllerEvents()
		{
			if (_controller == null || !_controllerEventsAttached)
			{
				return;
			}

			_controller.Connect -= HandleControllerConnect;
			_controller.Disconnect -= HandleControllerDisconnect;
			_controller.Device -= HandleControllerDevice;
			_controller.DeviceLost -= HandleControllerDeviceLost;
			_controller.DeviceFailure -= HandleControllerDeviceFailure;
			_controller.PolicyChange -= HandleControllerPolicyChange;
			_controller.FrameReady -= HandleControllerFrameReady;
			_controller.LogMessage -= HandleControllerLogMessage;
			_controllerEventsAttached = false;
		}

		private void HandleControllerConnect(object sender, ConnectionEventArgs args)
		{
			RememberRuntimeEvent("connect");
			RequestControllerPolicies();
		}

		private void HandleControllerDisconnect(object sender, ConnectionLostEventArgs args)
		{
			RememberRuntimeEvent("disconnect");
		}

		private void HandleControllerDevice(object sender, DeviceEventArgs args)
		{
			RememberRuntimeEvent("device serial=" + args.Device.SerialNumber +
				" type=" + args.Device.Type +
				" streaming=" + args.Device.IsStreaming.ToString().ToLowerInvariant());
			// In multi-device-aware mode (eLeapConnectionFlag_MultipleDevicesAware) the service
			// does NOT deliver tracking events unless the client explicitly subscribes.
			_controller.SubscribeToDeviceEvents(args.Device);
			RequestControllerPolicies(args.Device);
		}

		private void HandleControllerDeviceLost(object sender, DeviceEventArgs args)
		{
			RememberRuntimeEvent("device lost serial=" + args.Device.SerialNumber);
		}

		private void HandleControllerDeviceFailure(object sender, DeviceFailureEventArgs args)
		{
			RememberRuntimeEvent("device failure code=" + args.ErrorCode + " message=" + args.ErrorMessage);
		}

		private void HandleControllerPolicyChange(object sender, PolicyEventArgs args)
		{
			lock (_eventStateLock)
			{
				_lastPolicyState = DescribePolicies(args.currentPolicies);
			}
			RememberRuntimeEvent("policy change");
		}

		private void HandleControllerFrameReady(object sender, FrameEventArgs args)
		{
			lock (_eventStateLock)
			{
				_lastFrameEventId = args.frame == null ? 0 : args.frame.Id;
				_lastFrameEventTimestamp = args.frame == null ? 0 : args.frame.Timestamp;
			}
		}

		private void HandleControllerLogMessage(object sender, LogEventArgs args)
		{
			RememberRuntimeEvent("service log " + args.severity + ": " + args.message);
		}

		private void RememberRuntimeEvent(string value)
		{
			lock (_eventStateLock)
			{
				_lastRuntimeEvent = value;
			}
		}

		private static string DescribePolicies(ulong policies)
		{
			bool background = (policies & (ulong)Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES) != 0;
			bool allowPauseResume = (policies & (ulong)Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME) != 0;
			bool hmd = (policies & (ulong)Controller.PolicyFlag.POLICY_OPTIMIZE_HMD) != 0;
			bool screenTop = (policies & (ulong)Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP) != 0;

			return "policy bg=" + background.ToString().ToLowerInvariant() +
				" resume=" + allowPauseResume.ToString().ToLowerInvariant() +
				" hmd=" + hmd.ToString().ToLowerInvariant() +
				" screentop=" + screenTop.ToString().ToLowerInvariant(); 
		}

		private Device GetPrimaryDevice()
		{
			if (_controller == null || _controller.Devices == null || _controller.Devices.Count == 0)
			{
				return null;
			}

			return _controller.Devices[0];
		}

		private string BuildLatestEventDetails()
		{
			lock (_eventStateLock)
			{
				string frameEvent = _lastFrameEventId > 0
					? "frameEvent=" + _lastFrameEventId + "@" + _lastFrameEventTimestamp
					: "frameEvent=none";

				return frameEvent + " " + _lastPolicyState + " event=" + _lastRuntimeEvent;
			}
		}

		private string BuildConnectionDetails()
		{
			if (_controller == null)
			{
				return "service=false devices=0 background=false resume=false mode=desktop streaming=false version=0.0.0";
			}

			Device device = GetPrimaryDevice();
			bool backgroundFramesEnabled = false;
			bool allowPauseResumeEnabled = false;
			bool screenTopEnabled = false;
			bool hmdEnabled = false;
			try
			{
				backgroundFramesEnabled = _controller.IsPolicySet(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES, device);
				allowPauseResumeEnabled = _controller.IsPolicySet(Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME, device);
				screenTopEnabled = _controller.IsPolicySet(Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP, device);
				hmdEnabled = _controller.IsPolicySet(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD, device);
			}
			catch
			{
			}
			string trackingMode = screenTopEnabled ? "screentop" : (hmdEnabled ? "hmd" : "desktop");
			string streaming = device != null && device.IsStreaming ? "true" : "false";
			string deviceType = device == null ? "none" : device.Type.ToString().ToLowerInvariant();
			var serviceVersion = _controller.ServiceVersion;

			return "service=" + _controller.IsServiceConnected.ToString().ToLowerInvariant() +
				" devices=" + _controller.Devices.Count +
				" background=" + backgroundFramesEnabled.ToString().ToLowerInvariant() +
				" resume=" + allowPauseResumeEnabled.ToString().ToLowerInvariant() +
				" mode=" + trackingMode +
				" device=" + deviceType +
				" streaming=" + streaming +
				" version=" + serviceVersion.major + "." + serviceVersion.minor + "." + serviceVersion.patch;
		}

		private string BuildBackgroundFramesGuidance()
		{
			if (_controller == null)
			{
				return string.Empty;
			}

			Device device = GetPrimaryDevice();
			bool backgroundFramesEnabled;
			try
			{
				backgroundFramesEnabled = _controller.IsPolicySet(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES, device);
			}
			catch
			{
				backgroundFramesEnabled = false;
			}

			if (device != null && !device.IsStreaming)
			{
				return " Device is connected but not streaming.";
			}

			if (!backgroundFramesEnabled)
			{
				return " Background-frame policy is not active on this connection.";
			}

			lock (_eventStateLock)
			{
				if (_lastFrameEventId == 0)
				{
					return " No FrameReady events have been received from the service yet.";
				}
			}

			return string.Empty;
		}

		private static string FormatVector(Vector3 value)
		{
			return string.Format("({0:F3}, {1:F3}, {2:F3})", value.x, value.y, value.z);
		}

		private void FilterParameterUpdate()
		{
			_rightHandData.SetRotationFilterParams(_filterOptions.Frequency, _filterOptions.RotationMinCutoff, _filterOptions.RotationBeta, _filterOptions.RotationDerivativeCutoff);
			_rightHandData.SetPositionFilterParams(_filterOptions.Frequency, _filterOptions.PositionMinCutoff, _filterOptions.PositionBeta, _filterOptions.PositionDerivativeCutoff);

			_leftHandData.SetRotationFilterParams(_filterOptions.Frequency, _filterOptions.RotationMinCutoff, _filterOptions.RotationBeta, _filterOptions.RotationDerivativeCutoff);
			_leftHandData.SetPositionFilterParams(_filterOptions.Frequency, _filterOptions.PositionMinCutoff, _filterOptions.PositionBeta, _filterOptions.PositionDerivativeCutoff);
		}

		protected override bool UpdateTracking()
		{
			if (!TryEnsureController())
			{
				if (_errorCondition)
				{
					StatusLogger.Instance.Log("Ultraleap controller creation failed. See Console for details.");
				}
				return false;
			}

			if (_controller.IsConnected)
			{
				_frame = _controller.Frame();
				if (_frame == null)
				{
					SetConnectedStatus("Ultraleap is connected, but Controller.Frame() returned null.", 0);
					return false;
				}

				if (_frame.Id == _frameId)
				{
					if (Time.realtimeSinceStartup - _lastConnectionTest > RECOVERY_INTERVAL)
					{
						_lastConnectionTest = Time.realtimeSinceStartup;
						Device primaryDevice = GetPrimaryDevice();
						if (primaryDevice != null)
						{
							_controller.SubscribeToDeviceEvents(primaryDevice);
						}
						RequestControllerPolicies(primaryDevice);
					}

					return false;
				}

				_frameId = _frame.Id;
				_rightHandData.Detected = false;
				_leftHandData.Detected = false;

				foreach (var hand in _frame.Hands)
				{
					if (hand.IsRight)
					{
						UpdateHandData(hand, _rightHandData);
					}
					else
					{
						UpdateHandData(hand, _leftHandData);
					}
				}

				int handCount = _frame.Hands == null ? 0 : _frame.Hands.Count;
				if (handCount == 0)
				{
					SetConnectedStatus("Ultraleap is connected, frame " + _frame.Id + " received with 0 hands.", 0);
				}
				else
				{
					SetConnectedStatus(
						"Ultraleap frame " + _frame.Id + " received with " + handCount + " hand(s). Left=" +
						(_leftHandData.Detected ? FormatVector(_leftHandData.Position) : "--") +
						", Right=" + (_rightHandData.Detected ? FormatVector(_rightHandData.Position) : "--") + ".",
						handCount);
				}

				return true;
			}

			if (Time.realtimeSinceStartup - _lastConnectionTest > RECOVERY_INTERVAL)
			{
				_lastConnectionTest = Time.realtimeSinceStartup;
				_controller.StartConnection();
				RequestControllerPolicies();
				SetDisconnectedStatus("Ultraleap service is not connected. Retrying connection (" + BuildConnectionDetails() + ").", true);
			}
			else
			{
				SetDisconnectedStatus("Ultraleap controller exists, but the tracking service is not connected yet (" + BuildConnectionDetails() + ").");
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
			if (!TryEnsureController())
			{
				return false;
			}

			return _controller.IsConnected;
		}

		protected override void ResetDevice()
		{
			_frameId = 0;
			if (_controller != null)
			{
				_controller.StopConnection();
				_controller.StartConnection();
				RequestControllerPolicies();
				_lastConnectionTest = Time.realtimeSinceStartup;
				SetDisconnectedStatus("Ultraleap controller reset requested. Waiting for connection.", true);
			}
		}

		#region Singleton
		public static UltraLeapTracking SubInstance
		{
			get { return _subInstance ?? (_subInstance = new UltraLeapTracking(Options.Filter, Options.Inertia)); }
		}
		private static UltraLeapTracking _subInstance;

		[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void RegisterWithCore()
		{
			HandTracking.RegisterProvider(SubInstance);
		}
		#endregion Singleton

		#region IDisposable
		public override void Dispose()
		{
			try
			{
				Options.OnOptionsChange -= FilterParameterUpdate;

				if (_controller != null)
				{
					DetachControllerEvents();

					if (_controller.IsConnected)
					{
						_controller.StopConnection();
					}
					_controller.Dispose();
				}

				_controller = null;
				_subInstance = null;
				GC.Collect();
			}
			catch (Exception ex)
			{
				StatusLogger.Instance.Error(ex.ToString());
			}
		}
		#endregion

		private void UpdateHandData(Hand leapHand, HandData hand)
		{
			hand.Rotation = leapHand.Rotation;
			hand.Position = leapHand.PalmPosition - LEAP_POSITION_OFFSET;
			hand.GrabValue = leapHand.GrabStrength;
			hand.Detected = true;
			hand.IsRight = leapHand.IsRight;
			hand.PinchDistance = leapHand.PinchDistance;
		}

		protected override bool IsTrackingLibraryLoaded()
		{
			return !_errorCondition;
		}
	}
}
