using System;

namespace Lemonity.Core
{
	public enum WorkingMode { Disabled = 0, GrabOneHand, GrabTwoHands, GrabHybrid, Orbit, FlyOneHand, FlyTwoHands }

	public static class Options
	{
		private const string Prefix = "Lemonity.";
		private static bool _dirty;
		private static bool _init;
		private static bool _suppressChangeNotifications;

		static Options()
		{
			Configuration = new LemonityConfiguration(new PlayerPrefsStore(Prefix));
			Configuration.Changed += OnConfigurationChanged;
		}

		public static event Action OnOptionsLoad;
		public static event Action OnOptionsChange;

		public static LemonityConfiguration Configuration { get; private set; }

		public static GeneralOptions General
		{
			get { return Configuration.General; }
		}

		public static TrackingSpaceOptions TrackingSpace
		{
			get { return Configuration.TrackingSpace; }
		}

		public static GrabModeOptions GrabMode
		{
			get { return Configuration.GrabMode; }
		}

		public static OrbitModeOptions OrbitMode
		{
			get { return Configuration.OrbitMode; }
		}

		public static FlyModeOptions FlyMode
		{
			get { return Configuration.FlyMode; }
		}

		public static CameraOptions Camera
		{
			get { return Configuration.Camera; }
		}

		public static GestureOptions Gestures
		{
			get { return Configuration.Gestures; }
		}

		public static InertiaOptions Inertia
		{
			get { return Configuration.Inertia; }
		}

		public static FilterOptions Filter
		{
			get { return Configuration.Filter; }
		}

		public static HeuristicOptions Heuristic
		{
			get { return Configuration.Heuristic; }
		}

		public static IPreferencesStore PreferencesStore
		{
			get { return Configuration.PreferencesStore; }
			set
			{
				if (value == null || ReferenceEquals(Configuration.PreferencesStore, value))
				{
					return;
				}

				Configuration.PreferencesStore = value;
				_init = false;
				_dirty = false;
			}
		}

		public static WorkingMode Mode
		{
			get { return General.Mode; }
			set { General.Mode = value; }
		}

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

			Configuration.Save();
			Dirty = false;
		}

		public static void Load()
		{
			if (_init)
			{
				return;
			}

			_suppressChangeNotifications = true;
			Configuration.Load();
			_init = true;
			Dirty = false;
			_suppressChangeNotifications = false;

			OnOptionsLoad.SafeInvoke();
			OnOptionsChange.SafeInvoke();
		}

		private static void OnConfigurationChanged()
		{
			Dirty = true;
		}
	}
}