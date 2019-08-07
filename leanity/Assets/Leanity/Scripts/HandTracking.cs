using System;

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

		public static void Update()
		{
			Instance.UpdateTracking();
		}

		protected abstract void UpdateTracking();
		protected abstract HandData GetRightHandData();
		protected abstract HandData GetLeftHandData();

		public abstract void Dispose();

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
	}
}
