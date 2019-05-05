namespace CryoFall.Automaton.ClientComponents.Actions.Features
{
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Axes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using System.Collections.Generic;

    public class FeatureAutoWoodcutting: ProtoFeatureAutoHarvest
    {
        public override string Name => "AutoWoodcutting";

        public override string Description => "Auto-attack near trees if axe in hands.";

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<IProtoObjectTree>());

            requiredItemList.AddRange(Api.FindProtoEntities<IProtoItemToolWoodcutting>());
        }

        protected override double GetCurrentWeaponRange()
        {
            if (SelectedItem.ProtoItem is ProtoItemToolAxe toolAxe)
            {
                return toolAxe.RangeMax;
            }
            else
            {
                return base.GetCurrentWeaponRange();
            }
        }
    }
}
