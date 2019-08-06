using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leanity
{
	public abstract class HandTracking : IDisposable
	{
		#region Public Interface
		public static HandData MainHandData
		{
			get { return Instance.GetMainHandData(); }
		}
		public static HandData AuxHandData
		{
			get { return Instance.GetAuxHandData(); }
		}

		public static void Update()
		{
			Instance.UpdateTracking();
		}

		protected abstract void UpdateTracking();
		protected abstract HandData GetMainHandData();
		protected abstract HandData GetAuxHandData();

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
