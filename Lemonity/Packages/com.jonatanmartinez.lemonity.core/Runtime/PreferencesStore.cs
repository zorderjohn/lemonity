using UnityEngine;

namespace Lemonity.Core
{
	public interface IPreferencesStore
	{
		string GetString(string key, string defaultValue);
		void SetString(string key, string value);
		void Save();
	}

	public sealed class PlayerPrefsStore : IPreferencesStore
	{
		private readonly string _prefix;

		public PlayerPrefsStore(string prefix)
		{
			_prefix = prefix;
		}

		public string GetString(string key, string defaultValue)
		{
			return PlayerPrefs.GetString(_prefix + key, defaultValue);
		}

		public void SetString(string key, string value)
		{
			PlayerPrefs.SetString(_prefix + key, value);
		}

		public void Save()
		{
			PlayerPrefs.Save();
		}
	}
}