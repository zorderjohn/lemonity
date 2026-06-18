using System;
using Lemonity.Core;
using UnityEngine;

namespace Lemonity.Editor
{
	public static class EditorOptions
	{
		private static bool _dirty;
		private static bool _init;
		private static bool _suppressChangeNotifications;

		static EditorOptions()
		{
			PreferencesStore = new EditorPrefsStore("Lemonity.");
			Visuals = new VisualOptions();
			Visuals.Changed += OnVisualsChanged;
		}

		public static event Action OnOptionsLoad;
		public static event Action OnOptionsChange;

		public static VisualOptions Visuals { get; private set; }

		public static IPreferencesStore PreferencesStore { get; private set; }

		public static bool Dirty
		{
			get { return _dirty; }
			set
			{
				if (_dirty == value)
				{
					return;
				}

				_dirty = value;
				if (_dirty && !_suppressChangeNotifications)
				{
					OnOptionsChange.SafeInvoke();
				}
			}
		}

		public static void Save()
		{
			if (!Dirty)
			{
				return;
			}

			Visuals.Save(PreferencesStore);
			PreferencesStore.Save();
			Dirty = false;
		}

		public static void Load()
		{
			if (_init)
			{
				return;
			}

			_suppressChangeNotifications = true;
			Visuals.Load(PreferencesStore);
			StatusLogger.Instance.ConsoleLoggingEnabled = Visuals.DebugConsoleLog;
			_init = true;
			Dirty = false;
			_suppressChangeNotifications = false;

			OnOptionsLoad.SafeInvoke();
			OnOptionsChange.SafeInvoke();
		}

		private static void OnVisualsChanged()
		{
			StatusLogger.Instance.ConsoleLoggingEnabled = Visuals.DebugConsoleLog;
			Dirty = true;
		}
	}

	[Serializable]
	public sealed class VisualOptions : OptionsGroupBase
	{
		[SerializeField]
		private int _numGridLines = 6;
		[SerializeField]
		private bool _showGrid = true;
		[SerializeField]
		private bool _showWorkspace = true;
		[SerializeField]
		private bool _gestureDebug;
		[SerializeField]
		private float _maxGridTransparency = 0.8f;
		[SerializeField]
		private float _handScale = 1f;
		[SerializeField]
		private bool _showHandGuides = true;
		[SerializeField]
		private bool _debugConsoleLog;

		public int NumGridLines
		{
			get { return _numGridLines; }
			set { SetValue(ref _numGridLines, value); }
		}

		public bool ShowGrid
		{
			get { return _showGrid; }
			set { SetValue(ref _showGrid, value); }
		}

		public bool ShowWorkspace
		{
			get { return _showWorkspace; }
			set { SetValue(ref _showWorkspace, value); }
		}

		public bool GestureDebug
		{
			get { return _gestureDebug; }
			set { SetValue(ref _gestureDebug, value); }
		}

		public float MaxGridTransparency
		{
			get { return _maxGridTransparency; }
			set { SetValue(ref _maxGridTransparency, value); }
		}

		public float HandScale
		{
			get { return _handScale; }
			set { SetValue(ref _handScale, value); }
		}

		public bool ShowHandGuides
		{
			get { return _showHandGuides; }
			set { SetValue(ref _showHandGuides, value); }
		}

		public bool DebugConsoleLog
		{
			get { return _debugConsoleLog; }
			set { SetValue(ref _debugConsoleLog, value); }
		}
	}
}