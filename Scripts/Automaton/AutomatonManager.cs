namespace CryoFall.Automaton
{
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using CryoFall.Automaton.ClientSettings;
    using CryoFall.Automaton.Features;
    using System;
    using System.Collections.Generic;

    public static class AutomatonManager
    {
        public const string Notification_ModEnabled = "Automaton is enabled.";

        public const string Notification_ModDisabled = "Automaton is disabled.";

        private static List<ProtoSettings> SettingsList = new List<ProtoSettings>();

        private static IClientStorage versionStorage;

        private static IClientStorage isEnabledStorage;

        private static bool isEnabled;

        private static List<ProtoFeature> FeaturesList;

        public static double UpdateInterval = 0.5d;

        public static Version CurrentVersion => new Version("0.2.3");

        public static Version VersionFromClientStorage = null;

        /// <summary>
        /// Init on game load.
        /// </summary>
        public static void Init()
        {
            LoadVersionFromClientStorage();
            LoadIsEnbledFromClientStorage();

            FeaturesList = new List<ProtoFeature>()
            {
                new FeatureAutoPickUp(),
                new FeatureAutoGather(),
                new FeatureAutoMining(),
                new FeatureAutoWoodcutting(),
                new FeatureAutoFill(),
            };

            foreach (var feature in FeaturesList)
            {
                feature.PrepareProto();
                SettingsList.Add(new SettingsFeature(feature));
            }
            SettingsList.Add(new SettingsGlobal());

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

            // Version changes handeling.
            // if (VersionFromClientStorage.CompareTo(CurrentVersion) > 0)

            versionStorage.Save(CurrentVersion);
        }

        /// <summary>
        /// Try to load IsEnbled from client storage.
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

        public static List<ProtoSettings> GetAllSettings()
        {
            return SettingsList;
        }

        public static List<ProtoFeature> GetFeatures()
        {
            return FeaturesList;
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