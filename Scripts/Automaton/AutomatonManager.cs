namespace CryoFall.Automaton
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using CryoFall.Automaton.ClientSettings;
    using CryoFall.Automaton.Features;

    public static class AutomatonManager
    {
        // ReSharper disable once InconsistentNaming
        public const string Notification_ModEnabled = "Automaton is enabled.";

        // ReSharper disable once InconsistentNaming
        public const string Notification_ModDisabled = "Automaton is disabled.";

        private static readonly List<ProtoSettings> SettingsList = new List<ProtoSettings>();

        private static IClientStorage versionStorage;

        private static IClientStorage isEnabledStorage;

        private static bool isEnabled;

        private static List<IProtoFeature> featuresList = new List<IProtoFeature>();

        public static double UpdateInterval = 0.5d;

        public static Version CurrentVersion => new Version("0.3.4");

        public static Version VersionFromClientStorage = null;

        /// <summary>
        /// Init on game load.
        /// </summary>
        public static void Init()
        {
            LoadVersionFromClientStorage();
            LoadIsEnabledFromClientStorage();

            featuresList = new List<IProtoFeature>()
            {
                FeatureAutoPickUp.Instance,
                FeatureAutoGather.Instance,
                FeatureAutoMining.Instance,
                FeatureAutoWoodcutting.Instance,
                FeatureAutoFill.Instance,
            };

            foreach (var feature in featuresList)
            {
                feature.PrepareProto();
                SettingsList.Add(new SettingsFeature(feature));
            }
            SettingsList.Add(new SettingsGlobal());
            SettingsList.Add(new SettingsInformation());

            foreach (var settings in SettingsList)
            {
                // load settings.
                settings.InitSettings();
            }
        }

        /// <summary>
        /// Try to load mod version from client storage.
        /// </summary>
        private static void LoadVersionFromClientStorage()
        {
            // Load settings.
            versionStorage = Api.Client.Storage.GetStorage("Mods/Automaton/Version");
            versionStorage.RegisterType(typeof(Version));
            versionStorage.TryLoad(out VersionFromClientStorage);

            // Version changes handling.
            // if (VersionFromClientStorage.CompareTo(CurrentVersion) > 0)

            versionStorage.Save(CurrentVersion);
        }

        /// <summary>
        /// Try to load IsEnabled from client storage.
        /// </summary>
        private static void LoadIsEnabledFromClientStorage()
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
        /// Get list of all settings tabs.
        /// </summary>
        /// <returns></returns>
        public static List<ProtoSettings> GetAllSettings()
        {
            return SettingsList;
        }

        /// <summary>
        /// Get list of all features.
        /// </summary>
        /// <returns></returns>
        public static List<IProtoFeature> GetFeatures()
        {
            return featuresList;
        }

        /// <summary>
        /// Is Automaton component enabled? (toggled by key press)
        /// </summary>
        public static bool IsEnabled
        {
            get => isEnabled;
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