namespace CryoFall.Automaton.UI.Controls.Core.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ViewModelFeature : BaseViewModel
    {
        public ViewModelFeature(string name, string description,
            List<IProtoEntity> entityList, List<string> enabledList)
        {
            Name = name;
            Description = description;
            EntityCollection = new ObservableCollection<ViewModelEntity>(
                entityList.OrderBy(entity => entity.Id)
                          .Select(entity => new ViewModelEntity(entity)));

            foreach (var viewModelEntity in EntityCollection)
            {
                viewModelEntity.IsEnabledChanged +=
                    () => NotifyPropertyChanged(nameof(IsEnabled));

                if (enabledList?.Count > 0 && enabledList.Contains(viewModelEntity.Id))
                {
                    viewModelEntity.IsEnabled = true;
                }
            }

            SelectAll = new ActionCommand(() =>
                {
                    foreach (var viewModelEntity in EntityCollection)
                    {
                        viewModelEntity.IsEnabled = true;
                    }
                    NotifyPropertyChanged(nameof(IsEnabled));
                });
            DeselectAll = new ActionCommand(() =>
                {
                    foreach (var viewModelEntity in EntityCollection)
                    {
                        viewModelEntity.IsEnabled = false;
                    }
                    NotifyPropertyChanged(nameof(IsEnabled));
                });
        }

        public string Name { get; }

        public string Description { get; }

        public bool IsEnabled => EntityCollection.Any(viewModelEntity => viewModelEntity.IsEnabled);

        public BaseCommand SelectAll { get; }

        public BaseCommand DeselectAll { get; }

        public ObservableCollection<ViewModelEntity> EntityCollection { get; set; }

        public List<IProtoEntity> GetEnabledEntityList()
        {
            return EntityCollection.Where(e => e.IsEnabled).Select(e => e.Entity).ToList();
        }
    }
}