namespace CryoFall.Automaton.UI.Controls.Core.Managers
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using CryoFall.Automaton.UI.Controls.Core.Automaton.Features;
    using CryoFall.Automaton.UI.Controls.Core.Data;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public static class AutomatonManager
    {
        private static ObservableCollection<IMainWindowListEntry> ViewModelFeaturesSettings;

        private static IMainWindowListEntry ViewModelSettings;

        private static IClientStorage versionStorage;

        private static IClientStorage isEnabledStorage;

        private static bool isEnabled;

        private static Dictionary<string, ProtoFeature> FeaturesDictionary;

        public static double UpdateInterval = 0.5d;

        public static Version CurrentVersion => new Version("0.2.3");

        public static Version VersionFromClientStorage;

        /// <summary>
        /// Init on game load.
        /// </summary>
        public static void Init()
        {
            FeaturesDictionary = new Dictionary<string, ProtoFeature>()
            {
                {"AutoPickUp", new FeatureAutoPickUp()},
                {"AutoGather", new FeatureAutoGather()},
                {"AutoMining", new FeatureAutoMining()},
                {"AutoWoodcutting", new FeatureAutoWoodcutting()},
                {"AutoFill", new FeatureAutoFill()},
            };

            foreach (ProtoFeature feature in FeaturesDictionary.Values)
            {
                feature.PrepareProto();
            }

            LoadVersionFromClientStorage();
            LoadIsEnbledFromClientStorage();

            ViewModelFeaturesSettings = new ObservableCollection<IMainWindowListEntry>(
                FeaturesDictionary.Select(entry => new ViewModelSettingsFeature(
                    id: entry.Key,
                    feature: entry.Value)));
            ViewModelSettings = new ViewModelSettingsGlobal();
        }

        /// <summary>
        /// Try to load settings from client storage or init deafult one.
        /// </summary>
        private static void LoadVersionFromClientStorage()
        {
            // Load settings.
            versionStorage = Api.Client.Storage.GetStorage("Mods/Automaton/Version");
            versionStorage.RegisterType(typeof(Version));
            if (!versionStorage.TryLoad(out Version VersionFromClientStorage))
            {
                // Init default settings.
                VersionFromClientStorage = CurrentVersion;
            }

            // Version changes handeling.
            // if (VersionFromClientStorage.CompareTo(CurrentVersion) > 0)

            versionStorage.Save(CurrentVersion);
        }

        /// <summary>
        /// Try to load settings from client storage or init deafult one.
        /// </summary>
        private static void LoadIsEnbledFromClientStorage()
        {
            // Load settings.
            isEnabledStorage = Api.Client.Storage.GetStorage("Mods/Automaton/IsEnabled");
            if (!isEnabledStorage.TryLoad(out bool status))
            {
                // Init default settings.
                status = false;
            }

            IsEnabled = status;
        }

        /// <summary>
        /// Get ObservableCollection with view models of all features.
        /// </summary>
        /// <returns>ObservableCollection with view models of all features.</returns>
        public static ObservableCollection<IMainWindowListEntry> GetFeatures()
        {
            return ViewModelFeaturesSettings;
        }

        /// <summary>
        /// Get global settings view model.
        /// </summary>
        /// <returns>ViewModelSettings</returns>
        public static IMainWindowListEntry GetSettingsViewModel()
        {
            return ViewModelSettings;
        }

        /// <summary>
        /// Get Dictionary of all features by their name.
        /// </summary>
        /// <returns>Dictionary with all features using their names as key.</returns>
        public static Dictionary<string, ProtoFeature> GetFeaturesDictionary()
        {
            return FeaturesDictionary;
        }

        /// <summary>
        /// Is Automaton component enabled? (toggled by key press)
        /// </summary>
        public static bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value == isEnabled)
                {
                    return;
                }

                isEnabled = value;
                IsEnabledChanged?.Invoke();
                isEnabledStorage.Save(isEnabled);
            }
        }

        public static event Action IsEnabledChanged;
    }
}