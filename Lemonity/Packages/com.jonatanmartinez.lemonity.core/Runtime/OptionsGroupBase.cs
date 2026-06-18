using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lemonity.Core
{
	public abstract class OptionsGroupBase
	{
		public event Action Changed;

		public bool IsDirty { get; private set; }

		protected virtual string StoreKey
		{
			get { return GetType().Name; }
		}

		public void Load(IPreferencesStore store)
		{
			string json = store.GetString(StoreKey, string.Empty);
			if (!string.IsNullOrEmpty(json))
			{
				JsonUtility.FromJsonOverwrite(json, this);
			}

			MarkClean();
		}

		public void Save(IPreferencesStore store)
		{
			store.SetString(StoreKey, JsonUtility.ToJson(this));
			MarkClean();
		}

		protected bool SetValue<T>(ref T field, T value)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
			{
				return false;
			}

			field = value;
			MarkDirty();
			return true;
		}

		protected void MarkDirty()
		{
			IsDirty = true;
			Changed.SafeInvoke();
		}

		internal void MarkClean()
		{
			IsDirty = false;
		}
	}
}