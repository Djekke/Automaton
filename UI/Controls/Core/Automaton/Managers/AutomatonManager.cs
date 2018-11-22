namespace CryoFall.Automaton.UI.Controls.Core.Managers
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using CryoFall.Automaton.UI.Controls.Core.Data;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public static class AutomatonManager
    {
        private static ObservableCollection<ViewModelFeature> Features;

        private static Dictionary<string, ViewModelFeature> FeaturesDictionary;

        private static IClientStorage settingsStorage;

        private static Settings settingsInstance;

        /// <summary>
        /// List of Features name with description in dictionary form.
        /// </summary>
        private static readonly Dictionary<string, string> FeaturesDescriptions
            = new Dictionary<string, string>()
            {
                { "AutoPickUp", "PickUp items from ground (including twigs, stones, grass)"},
                { "AutoGather", "Gather berry, herbs and other vegetation, harvest corpses, loot radtowns"},
                { "AutoWoodcutting", "Auto-attack near trees if axe in hands"},
                { "AutoMining", "Auto-attack near minerals if pickaxe in hands"},
            };

        /// <summary>
        /// Dictionary of EntityLists for every feature.
        /// </summary>
        private static readonly Dictionary<string, List<IProtoEntity>> FeaturesEntityLists
            = new Dictionary<string, List<IProtoEntity>>()
            {
                { "AutoPickUp", Api.FindProtoEntities<ProtoObjectLoot>().ToList<IProtoEntity>()
                    .Concat(Api.FindProtoEntities<ObjectGroundItemsContainer>()).ToList()},
                { "AutoGather", Api.FindProtoEntities<IProtoObjectGatherable>().ToList<IProtoEntity>()},
                { "AutoWoodcutting", Api.FindProtoEntities<IProtoObjectTree>().ToList<IProtoEntity>()},
                { "AutoMining", Api.FindProtoEntities<IProtoObjectMineral>().ToList<IProtoEntity>()},
            };

        /// <summary>
        /// Init features veiw models.
        /// </summary>
        private static void InitFeatures()
        {
            Features = new ObservableCollection<ViewModelFeature>();
            FeaturesDictionary = new Dictionary<string, ViewModelFeature>();
            foreach (string feature in FeaturesDescriptions.Keys)
            {
                FeaturesDictionary.Add(feature, new ViewModelFeature(
                        feature,
                        FeaturesDescriptions[feature],
                        FeaturesEntityLists[feature],
                        settingsInstance.Features[feature]));
                Features.Add(FeaturesDictionary[feature]);
            }
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
                    = FeaturesDescriptions.Keys.ToDictionary(name => name, name => new List<string>());
            }

            InitFeatures();
        }

        /// <summary>
        /// Save settings in ClientStorage.
        /// </summary>
        public static void SaveSettings()
        {
            bool pendingChanges = false;
            foreach (ViewModelFeature feature in Features)
            {
                var tempList = feature.EntityCollection
                                      .Where(entityViewModel => entityViewModel.IsEnabled)
                                      .Select(entityViewModel => entityViewModel.Id)
                                      .ToList();
                if (!(settingsInstance.Features[feature.Name].Count == tempList.Count &&
                      settingsInstance.Features[feature.Name].All(tempList.Contains)))
                {
                    settingsInstance.Features[feature.Name] = tempList;
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
            return Features;
        }

        /// <summary>
        /// Get Dictionary with view models of all features.
        /// </summary>
        /// <returns>Dictionary with view models of all features.</returns>
        public static Dictionary<string, List<string>> GetFeaturesDictionary()
        {
            return settingsInstance.Features;
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
                // TODO: Add turn off component.
                settingsStorage.Save(settingsInstance);
            }
        }

        /// <summary>
        /// Init on game load.
        /// </summary>
        public static void Init()
        {
            LoadSettings();
        }


        public struct Settings
        {
            public bool IsEnabled;

            public Dictionary<string, List<string>> Features;
        }
    }
}