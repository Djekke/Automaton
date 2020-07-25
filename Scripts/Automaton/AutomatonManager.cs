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

        public const string ModId = "Automaton";

        public static Version CurrentVersion => new Version("0.3.5");

        public const string RssFeed = //"http://github.com/Djekke/Automaton/releases.atom";
            "https://feedmix.novaclic.com/atom2rss.php?source=https%3A%2F%2Fgithub.com%2FDjekke%2FAutomaton%2Freleases.atom";

        public static Version VersionFromClientStorage = null;

        /// <summary>
        /// Init on game load.
        /// </summary>
        public static void Init()
        {
            LoadVersionFromClientStorage();
            LoadIsEnabledFromClientStorage();

            AddFeature(FeatureAutoPickUp.Instance);
            AddFeature(FeatureAutoGather.Instance);
            AddFeature(FeatureAutoMining.Instance);
            AddFeature(FeatureAutoWoodcutting.Instance);
            AddFeature(FeatureAutoFill.Instance);

            AddAndInitCustomSettingsTab(SettingsGlobal.Instance);
            AddAndInitCustomSettingsTab(SettingsInformation.Instance);
        }

        /// <summary>
        /// Try to load mod version from client storage.
        /// </summary>
        private static void LoadVersionFromClientStorage()
        {
            // Load settings.
            versionStorage = Api.Client.Storage.GetStorage("Mods/" + ModId + "/ Version");
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
            isEnabledStorage = Api.Client.Storage.GetStorage("Mods/" + ModId + "/IsEnabled");
            if (!isEnabledStorage.TryLoad(out bool status))
            {
                // Init default settings.
                status = false;
            }

            isEnabled = status;
        }

        /// <summary>
        /// Add custom settings passed as parameter to SettingsList and init this settings.
        /// </summary>
        /// <param name="customSettings"></param>
        public static void AddAndInitCustomSettingsTab(ProtoSettings customSettings)
        {
            SettingsList.Add(customSettings);
            SettingsList.Sort((s1, s2) =>
                s1.Order == s2.Order
                ? s1.Name.CompareTo(s2.Name)
                : s1.Order.CompareTo(s2.Order));
            customSettings.InitSettings();
        }

        /// <summary>
        /// Add feature to feature list.
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static bool AddFeature(IProtoFeature feature)
        {
            if (featuresList.Contains(feature))
            {
                Api.Logger.Error("Automaton: This feature already added: '" + feature + "'");
                return false;
            }

            featuresList.Add(feature);

            feature.PrepareProto();
            AddAndInitCustomSettingsTab(new SettingsFeature(feature));
            return true;
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