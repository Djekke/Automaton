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
        private static ObservableCollection<ViewModelFeature> ViewModelFeaturesCollection;

        private static IClientStorage settingsStorage;

        private static Settings settingsInstance;

        private static Dictionary<string, ProtoFeature> FeaturesDictionary;

        /// <summary>
        /// Init on game load.
        /// </summary>
        public static void Init()
        {
            FeaturesDictionary = new Dictionary<string, ProtoFeature>()
            {
                {"AutoPickUp", new ProtoFeatureAutoPickUp()},
                {"AutoGather", new ProtoFeatureAutoGather()},
                {"AutoMining", new ProtoFeatureAutoMining()},
                {"AutoWoodcutting", new ProtoFeatureAutoWoodcutting()},
            };

            foreach (ProtoFeature feature in FeaturesDictionary.Values)
            {
                feature.PrepareProto();
            }

            LoadSettings();

            ViewModelFeaturesCollection = new ObservableCollection<ViewModelFeature>(
                FeaturesDictionary.Values.Select(feature => new ViewModelFeature(
                    feature.Name,
                    feature.Description,
                    feature.EntityList,
                    settingsInstance.Features[feature.Name])));
        }

        /// <summary>
        /// Try to load settings from client storage or init deafult one.
        /// </summary>
        private static void LoadSettings()
        {
            settingsStorage = Api.Client.Storage.GetStorage("Mods/Automaton.Settings");
            settingsStorage.RegisterType(typeof(Settings));
            if (!settingsStorage.TryLoad(out settingsInstance))
            {
                // Init default settings. (All disabled by default)
                settingsInstance.IsEnabled = false;
                settingsInstance.Features
                    = FeaturesDictionary.ToDictionary(p => p.Key, p => new List<string>());
                // TODO: May be add default options in ProtoFeature
            }

            foreach (KeyValuePair<string, ProtoFeature> pair in FeaturesDictionary)
            {
                pair.Value.LoadSettings(settingsInstance.Features[pair.Key]);
            }
        }

        /// <summary>
        /// Save settings in ClientStorage.
        /// </summary>
        public static void SaveSettings()
        {
            bool pendingChanges = false;
            foreach (ViewModelFeature feature in ViewModelFeaturesCollection)
            {
                var enabledEntityList = feature.GetEnabledEntityList();

                if(!Enumerable.SequenceEqual(
                        enabledEntityList.OrderBy(e => e.Id),
                        FeaturesDictionary[feature.Name].EnabledEntityList.OrderBy(e => e.Id)))
                {
                    settingsInstance.Features[feature.Name] = enabledEntityList.Select(entity => entity.Id).ToList();
                    FeaturesDictionary[feature.Name].EnabledEntityList = enabledEntityList;
                    pendingChanges = true;
                }
            }
            if (pendingChanges)
            {
                settingsStorage.Save(settingsInstance);
            }
        }

        /// <summary>
        /// Get ObservableCollection with view models of all features.
        /// </summary>
        /// <returns>ObservableCollection with view models of all features.</returns>
        public static ObservableCollection<ViewModelFeature> GetFeatures()
        {
            return ViewModelFeaturesCollection;
        }

        /// <summary>
        /// Get Dictionary of all features by their name.
        /// </summary>
        /// <returns>Dictionary with all features using their names as key.</returns>
        public static Dictionary<string, ProtoFeature> GetFeaturesDictionary()
        {
            return FeaturesDictionary;
        }

        public static bool IsEnabled
        {
            get { return settingsInstance.IsEnabled; }
            set
            {
                if (value == IsEnabled)
                {
                    return;
                }

                settingsInstance.IsEnabled = value;
                IsEnabledChanged?.Invoke();
                settingsStorage.Save(settingsInstance);
            }
        }

        public static event Action IsEnabledChanged;

        public struct Settings
        {
            public bool IsEnabled;

            public Dictionary<string, List<string>> Features;
        }
    }
}