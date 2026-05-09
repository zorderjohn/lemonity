using System;
using UnityEngine;

namespace Lemonity.Core
{
	public abstract class HandTracking : IDisposable
	{
		#region Public Interface
		public static event Action OnConnect;
		public static event Action OnDisconnect;

		public static HandData RightHandData {	get { return Instance.GetRightHandData(); }}
		public static HandData LeftHandData { get { return Instance.GetLeftHandData(); }}
		public static Vector3 Workspace { get { return Instance.GetWorkspace(); }}

		public static Vector3 TransformPosition { get; set; }
		public static Quaternion TransformRotation { get; set; }
		public static float TransformScale { get; set; }


		public static bool Update()
		{
			Instance.UpdateDeviceState();
			return Instance.UpdateTracking();
		}

		public static bool IsConnected()
		{
			return Instance.UpdateDeviceState();
		}

		public static bool IsLibraryLoaded()
		{
			return Instance.IsTrackingLibraryLoaded();
		}

		public static void Reset()
		{
			Instance.ResetDevice();
		}

		public static Vector3 CamToHandOffset(TrackingSpaceOptions trackingSpaceOptions)
		{
			return Vector3.forward * trackingSpaceOptions.PosScale * Workspace.z * trackingSpaceOptions.TrackingZOffset;
		}

		public static Vector3 CamToHandOffset(float scale, TrackingSpaceOptions trackingSpaceOptions)
		{
			return Vector3.forward * scale * Workspace.z * trackingSpaceOptions.TrackingZOffset;
		}

		public static Vector3 HandToCamCoordinates(Vector3 position, TrackingSpaceOptions trackingSpaceOptions)
		{
			return position * trackingSpaceOptions.PosScale + CamToHandOffset(trackingSpaceOptions);
		}

		public static Vector3 ToWorldCoordinates(Vector3 position)
		{
			return TransformPosition + TransformRotation * (position * TransformScale);
		}

		public static Quaternion ToWorldCoordinates(Quaternion rotation)
		{
#if UNITY_2018_2_OR_NEWER
			return Quaternion.Normalize(TransformRotation * rotation);
#else
			return TransformRotation * rotation;
#endif
		}

		protected bool _isConnected;
		protected bool UpdateDeviceState()
		{
			bool connectedUpdate = IsDeviceConnected();
			if (_isConnected && !connectedUpdate)
			{
				OnDisconnect.SafeInvoke();
			}
			else if (!_isConnected && connectedUpdate)
			{
				OnConnect.SafeInvoke();
			}

			_isConnected = connectedUpdate;
			return _isConnected;
		}

#endregion

#region Singleton
		private static HandTracking _instance;

		public static bool HasProvider => _instance != null;

		public static HandTracking Instance
		{
			get
			{
				if (_instance == null)
				{
					throw new InvalidOperationException(
						"[Lemonity] No HandTracking provider has been registered. " +
						"Install a provider package (e.g. com.lemonity.provider.ultraleap) " +
						"and ensure it is included in the project.");
				}
				return _instance;
			}
		}

		public static void RegisterProvider(HandTracking provider)
		{
			_instance = provider;
		}
#endregion

		public abstract void Dispose();
		protected abstract bool UpdateTracking();
		protected abstract HandData GetRightHandData();
		protected abstract HandData GetLeftHandData();
		protected abstract Vector3 GetWorkspace();
		protected abstract bool IsDeviceConnected();
		protected abstract bool IsTrackingLibraryLoaded();
		protected abstract void ResetDevice();
	}
}
