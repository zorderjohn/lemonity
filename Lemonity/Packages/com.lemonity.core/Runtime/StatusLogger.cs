using UnityEngine;

namespace Lemonity.Core
{
	public class StatusLogger
	{
		private const float LOG_INTERVAL = 2f;

		public static StatusLogger Instance { get; } = new StatusLogger();

		public string Status { get; private set; } = string.Empty;
		public bool ConsoleLoggingEnabled { get; set; }

		private float _lastLogTime;

		private StatusLogger() {}

		public void Log(string message, bool forceLog = false)
		{
			Status = message;

			if (!ConsoleLoggingEnabled)
			{
				return;
			}

			if (!forceLog && Time.realtimeSinceStartup - _lastLogTime < LOG_INTERVAL)
			{
				return;
			}

			_lastLogTime = Time.realtimeSinceStartup;
			Debug.Log("[Lemonity] " + message);
		}

		public void Warning(string message)
		{
			Status = message;
			Debug.LogWarning("[Lemonity] " + message);
		}

		public void Error(string message)
		{
			Status = message;
			Debug.LogError("[Lemonity] " + message);
		}
	}
}
