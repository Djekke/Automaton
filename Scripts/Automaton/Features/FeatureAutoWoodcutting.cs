namespace CryoFall.Automaton.Features
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Axes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using CryoFall.Automaton.ClientSettings;
    using CryoFall.Automaton.ClientSettings.Options;

    public class FeatureAutoWoodcutting: ProtoFeatureAutoHarvest<FeatureAutoWoodcutting>
    {
        private FeatureAutoWoodcutting() { }

        public override string Name => "AutoWoodcutting";

        public override string Description => "Auto-attack near trees if axe in hands.";

        public string ChopOnlyFullyGrownText => "Chop only fully grown trees";

        public bool ChopOnlyFullyGrown { get; set; }

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<IProtoObjectTree>());

            requiredItemList.AddRange(Api.FindProtoEntities<IProtoItemToolWoodcutting>());
        }

        public override void PrepareOptions(SettingsFeature settingsFeature)
        {
            // Full override of default settings because we need to change order of some options.
            AddOptionIsEnabled(settingsFeature);
            Options.Add(new OptionSeparator());
            Options.Add(new OptionCheckBox(
                parentSettings: settingsFeature,
                id: "ChopOnlyFullyGrown",
                label: ChopOnlyFullyGrownText,
                defaultValue: true,
                valueChangedCallback: value =>
                {
                    ChopOnlyFullyGrown = value;
                }));
            Options.Add(new OptionSeparator());
            AddOptionEntityList(settingsFeature);
        }

        protected override bool AdditionalValidation(IStaticWorldObject testWorldObject)
        {
            return !ChopOnlyFullyGrown ||
                   testWorldObject.GetPublicState<VegetationPublicState>()
                    .IsFullGrown((IProtoObjectTree) testWorldObject.ProtoStaticWorldObject);
        }

        protected override double GetCurrentWeaponRange()
        {
            if (SelectedItem.ProtoItem is ProtoItemToolAxe toolAxe)
            {
                return toolAxe.RangeMax;
            }
            return base.GetCurrentWeaponRange();
        }
    }
}
