namespace CryoFall.Automaton.UI.Controls.Core.Managers
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using CryoFall.Automaton.UI.Controls.Core.Data;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;

    public static class AutomatonManager
    {
        private static ObservableCollection<ViewModelFeature> Features;

        private static IClientStorage settingsStorage;

        private static Settings settingsInstance;

        /// <summary>
        /// List of Features name with description in dictionary form.
        /// </summary>
        private static readonly Dictionary<string, string> FeaturesDescriptions = new Dictionary<string, string>()
        {
            { "AutoPickUp", "PickUp items from ground (including twigs, stones, grass)"},
            { "AutoGather", "Gather berry, herbs and other vegetation, harvest corpses, loot radtowns"},
            { "AutoWoodcutting", "Auto-attack near trees if axe in hands"},
            { "AutoMining", "Auto-attack near minerals if pickaxe in hands"},
        };

        /// <summary>
        /// Init features veiw models.
        /// </summary>
        private static void InitFeatures()
        {
            Features = new ObservableCollection<ViewModelFeature>();
            //List<string> autoloot = new List<string>() { "ObjectGroundItemsContainer" };
            //autoloot.AddRange(Api.FindProtoEntities<ProtoObjectLoot>()
            //                     .Select(entity => entity.ShortId));
            //Features.Add(new ViewModelAutomatonFeature("AutoPickUp",
            //    FeaturesDescriptions["AutoPickUp"],
            //    autoloot));
            //Features.Add(new ViewModelAutomatonFeature("AutoGather",
            //    FeaturesDescriptions["AutoGather"],
            //    Api.FindProtoEntities<IProtoObjectGatherable>().Select(entity => entity.ShortId).ToList()));
            //Features.Add(new ViewModelAutomatonFeature("AutoLoot",
            //    FeaturesDescriptions["AutoLoot"],
            //    Api.FindProtoEntities<ProtoObjectLootContainer>().Select(entity => entity.ShortId).ToList()));
            //Features.Add(new ViewModelAutomatonFeature("AutoWoodcutting",
            //    FeaturesDescriptions["AutoWoodcutting"],
            //    Api.FindProtoEntities<IProtoObjectTree>().Select(entity => entity.ShortId).ToList()));
            //Features.Add(new ViewModelAutomatonFeature("AutoMining",
            //    FeaturesDescriptions["AutoMining"],
            //    Api.FindProtoEntities<IProtoObjectMineral>().Select(entity => entity.ShortId).ToList()));
            List<IProtoEntity> autoloot = new List<IProtoEntity>(Api.FindProtoEntities<ObjectGroundItemsContainer>());
            autoloot.AddRange(Api.FindProtoEntities<ProtoObjectLoot>());
            Features.Add(new ViewModelFeature("AutoPickUp",
                FeaturesDescriptions["AutoPickUp"],
                autoloot,
                null));
            Features.Add(new ViewModelFeature("AutoGather",
                FeaturesDescriptions["AutoGather"],
                Api.FindProtoEntities<IProtoObjectGatherable>().ToList<IProtoEntity>(),
                null));
            Features.Add(new ViewModelFeature("AutoWoodcutting",
                FeaturesDescriptions["AutoWoodcutting"],
                Api.FindProtoEntities<IProtoObjectTree>().ToList<IProtoEntity>(),
                null));
            Features.Add(new ViewModelFeature("AutoMining",
                FeaturesDescriptions["AutoMining"],
                Api.FindProtoEntities<IProtoObjectMineral>().ToList<IProtoEntity>(),
                null));
            // TODO: Add proper loading.
        }

        /// <summary>
        /// Try to load settings from client storage or init deafult one.
        /// </summary>
        private static void LoadSettings()
        {
            settingsStorage = Api.Client.Storage.GetStorage("Mods/Automaton.Settings");
            settingsStorage.RegisterType(typeof(Feature));
            settingsStorage.RegisterType(typeof(Settings));

            if (!settingsStorage.TryLoad(out settingsInstance))
            {
                // Init default settings. (All disabled by default)
                settingsInstance.IsEnabled = false;
                settingsInstance.Features = FeaturesDescriptions.Keys.ToDictionary(name => name, name => new Feature());
            }

            InitFeatures();
        }

        /// <summary>
        /// Save settings in ClientStorage.
        /// </summary>
        public static void SaveSettings()
        {
            // TODO: Actualliy save? Convert Dictionary<string, bool> -> List<string>.
            // TODO: check for changes?
            //settingsStorage.Save(settingsInstance);
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
        /// Init on game load.
        /// </summary>
        public static void Init()
        {
            //LoadSettings();
            InitFeatures();
        }


        public struct Settings
        {
            public bool IsEnabled;

            public Dictionary<string, Feature> Features;
        }

        public struct Feature
        {
            //public string Name;

            public List<string> EntityTypeNamesList;
        }
    }
}