namespace CryoFall.Automaton.Managers
{
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using CryoFall.Automaton.ClientComponents.Actions;
    using CryoFall.Automaton.ClientComponents.Actions.Features;
    using CryoFall.Automaton.ClientSettings;
    using System;
    using System.Collections.Generic;

    public static class AutomatonManager
    {
        public const string Notification_ModEnabled = "Automaton is enabled.";

        public const string Notification_ModDisabled = "Automaton is disabled.";

        private static List<ProtoSettings> SettingsList;

        private static IClientStorage versionStorage;

        private static IClientStorage isEnabledStorage;

        private static bool isEnabled;

        private static Dictionary<string, ProtoFeature> FeaturesDictionary;

        public static double UpdateInterval = 0.5d;

        public static Version CurrentVersion => new Version("0.2.3");

        public static Version VersionFromClientStorage = null;

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

            SettingsList = new List<ProtoSettings>();
            foreach (var pair in FeaturesDictionary)
            {
                SettingsList.Add(new SettingsFeature(pair.Key, pair.Value));
            }
            SettingsList.Add(new SettingsGlobal());

            foreach (var settings in SettingsList)
            {
                // load settings.
                settings.InitSettings();
            }
        }

        /// <summary>
        /// Try to load settings from client storage or init deafult one.
        /// </summary>
        private static void LoadVersionFromClientStorage()
        {
            // Load settings.
            versionStorage = Api.Client.Storage.GetStorage("Mods/Automaton/Version");
            versionStorage.RegisterType(typeof(Version));
            versionStorage.TryLoad(out VersionFromClientStorage);

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

            isEnabled = status;
        }

        /// <summary>
        /// Get ObservableCollection with view models of all features.
        /// </summary>
        /// <returns>ObservableCollection with view models of all features.</returns>
        public static List<ProtoSettings> GetAllSettings()
        {
            return SettingsList;
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
                if (ClientComponentAutomaton.Instance != null)
                {
                    ClientComponentAutomaton.Instance.IsEnabled = value;
                    NotificationSystem.ClientShowNotification(
                        value ? Notification_ModEnabled : Notification_ModDisabled);
                }

                IsEnabledChanged?.Invoke();
                isEnabledStorage.Save(isEnabled);
            }
        }

        public static event Action IsEnabledChanged;
    }
}