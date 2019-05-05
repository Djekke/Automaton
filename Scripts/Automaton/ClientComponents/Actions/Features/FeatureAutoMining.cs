namespace CryoFall.Automaton.ClientComponents.Actions.Features
{
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using System.Collections.Generic;

    public class FeatureAutoMining: ProtoFeatureAutoHarvest
    {
        public override string Name => "AutoMining";

        public override string Description => "Auto-attack near minerals if pickaxe in hands.";

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<IProtoObjectMineral>());

            requiredItemList.AddRange(Api.FindProtoEntities<IProtoItemToolMining>());
        }

        protected override double GetCurrentWeaponRange()
        {
            if (SelectedItem.ProtoItem is ProtoItemToolPickaxe toolPickaxe)
            {
                return toolPickaxe.RangeMax;
            }
            else
            {
                return base.GetCurrentWeaponRange();
            }
        }
    }
}
