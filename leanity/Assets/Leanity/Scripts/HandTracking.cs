using System;
using UnityEngine;

namespace Leanity
{
	public abstract class HandTracking : IDisposable
	{
		#region Public Interface
		public static HandData RightHandData
		{
			get { return Instance.GetRightHandData(); }
		}
		public static HandData LeftHandData
		{
			get { return Instance.GetLeftHandData(); }
		}
		public static Vector3 Workspace
		{
			get { return Instance.GetWorkspace(); }
		}

		public static void Update()
		{
			Instance.UpdateTracking();
		}

		public static Vector3 TransformPosition { get; set; }
		public static Quaternion TransformRotation { get; set; }
		public static float TransformScale { get; set; }

		public static Vector3 ToWorldCoordinates(Vector3 position)
		{
			return TransformPosition + TransformRotation * (position * TransformScale);
		}

		public static Quaternion ToWorldCoordinates(Quaternion rotation)
		{
			return Quaternion.Normalize(TransformRotation * rotation);
		}

		public abstract void Dispose();
		#endregion

		#region Singleton
		public static HandTracking _instance;
		public static HandTracking Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = LeapTrackingWin.SubInstance;
				}
				return _instance;
			}
		}
		#endregion

		protected abstract void UpdateTracking();
		protected abstract HandData GetRightHandData();
		protected abstract HandData GetLeftHandData();
		protected abstract Vector3 GetWorkspace();

	}
}
