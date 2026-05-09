using System;

namespace Lemonity.Core
{
	public sealed class LemonityConfiguration
	{
		private readonly OptionsGroupBase[] _groups;
		private bool _isLoading;

		public event Action Changed;
		public event Action Loaded;

		public GeneralOptions General { get; private set; }
		public TrackingSpaceOptions TrackingSpace { get; private set; }
		public GrabModeOptions GrabMode { get; private set; }
		public OrbitModeOptions OrbitMode { get; private set; }
		public FlyModeOptions FlyMode { get; private set; }
		public CameraOptions Camera { get; private set; }
		public GestureOptions Gestures { get; private set; }
		public InertiaOptions Inertia { get; private set; }
		public FilterOptions Filter { get; private set; }
		public HeuristicOptions Heuristic { get; private set; }

		public IPreferencesStore PreferencesStore { get; set; }

		public LemonityConfiguration(IPreferencesStore preferencesStore)
		{
			PreferencesStore = preferencesStore;
			General = new GeneralOptions();
			TrackingSpace = new TrackingSpaceOptions();
			GrabMode = new GrabModeOptions();
			OrbitMode = new OrbitModeOptions();
			FlyMode = new FlyModeOptions();
			Camera = new CameraOptions();
			Gestures = new GestureOptions();
			Inertia = new InertiaOptions();
			Filter = new FilterOptions();
			Heuristic = new HeuristicOptions();

			_groups = new OptionsGroupBase[]
			{
				General,
				TrackingSpace,
				GrabMode,
				OrbitMode,
				FlyMode,
				Camera,
				Gestures,
				Inertia,
				Filter,
				Heuristic
			};

			for (int i = 0; i < _groups.Length; i++)
			{
				_groups[i].Changed += OnGroupChanged;
			}
		}

		public void Load()
		{
			_isLoading = true;

			for (int i = 0; i < _groups.Length; i++)
			{
				_groups[i].Load(PreferencesStore);
			}

			_isLoading = false;
			Loaded.SafeInvoke();
		}

		public void Save()
		{
			for (int i = 0; i < _groups.Length; i++)
			{
				_groups[i].Save(PreferencesStore);
			}

			PreferencesStore.Save();
		}

		private void OnGroupChanged()
		{
			if (_isLoading)
			{
				return;
			}

			Changed.SafeInvoke();
		}
	}
}