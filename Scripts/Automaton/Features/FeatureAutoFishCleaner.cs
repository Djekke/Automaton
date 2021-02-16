namespace CryoFall.Automaton.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class FeatureAutoFishCleaner : ProtoFeature<FeatureAutoFishCleaner>
    {
        private FeatureAutoFishCleaner() { }

        public override string Name => "AutoFishCleaner";

        public override string Description => "Auto use fish to cut them to pieces.";

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<IProtoItemFish>());
        }

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        public override void Execute()
        {
            if (IsEnabled
                && EnabledEntityList.Any())
            {
                foreach (var fish in GetAllFish())
                {
                    fish.ProtoItem.ClientItemUseStart(fish);
                    fish.ProtoItem.ClientItemUseFinish(fish);
                }
            }
        }

        private IEnumerable<IItem> GetAllFish()
        {
            return CurrentCharacter.ProtoCharacter
                                   .SharedEnumerateAllContainers(CurrentCharacter, includeEquipmentContainer: false)
                                   .SelectMany(c => c.Items)
                                   .Where(i => EnabledEntityList.Contains(i.ProtoItem));
        }
    }
}
