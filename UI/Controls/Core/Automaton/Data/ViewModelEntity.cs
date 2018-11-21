namespace CryoFall.Automaton.UI.Controls.Core.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ViewModelEntity : BaseViewModel
    {
        private static readonly IReadOnlyCollection<Type> IconBlackList = new List<Type>() { 
            typeof(ObjectGroundItemsContainer),
            typeof(ObjectCorpse),
        }.AsReadOnly();

        private ITextureResource iconResource = null;

        private TextureBrush icon;

        private bool isEnabled;

        private readonly IProtoEntity entity;

        private ViewModelFeature parentViewModelFeature = null;

        public ViewModelEntity(IProtoEntity entity, ViewModelFeature parentViewModel)
        {
            this.entity = entity;
            Name = entity.Name;
            Id = entity.Id;
            isEnabled = false;
            parentViewModelFeature = parentViewModel;
        }

        public string Name { get; }

        public string Id { get; }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value == IsEnabled)
                {
                    return;
                }

                isEnabled = value;
                NotifyThisPropertyChanged();
                parentViewModelFeature?.RefreshIsEnabledStatus();
            }
        }

        public TextureBrush Icon
        {
            get
            {
                if (icon == null)
                {
                    if (!IconBlackList.Contains(entity.GetType()))
                    {
                        if (entity is IProtoStaticWorldObject staticWorldObject)
                        {
                            iconResource = staticWorldObject.Icon ?? staticWorldObject.DefaultTexture;
                        }
                    }
                    if (iconResource == null)
                    {
                        // Default icon.
                        iconResource = new TextureResource("Content/Textures/StaticObjects/ObjectUnknown.png");
                    }
                    icon = Api.Client.UI.GetTextureBrush(iconResource);
                }
                return icon;
            }
        }
    }
}