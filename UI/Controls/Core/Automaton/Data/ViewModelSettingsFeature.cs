namespace CryoFall.Automaton.UI.Data
{
    using AtomicTorch.CBND.GameApi.Data;
    using CryoFall.Automaton.UI.Data.Options;
    using CryoFall.Automaton.UI.Features;
    using System.Collections.Generic;
    using System.Linq;

    public class ViewModelSettingsFeature : ViewModelSettings
    {
        public bool IsEnabled { get; private set; }

        public string IsEnabledText => "Enable this feature";

        private ProtoFeature Feature;

        public ViewModelSettingsFeature(string id, ProtoFeature feature)
        {
            Id = id;
            Feature = feature;
            Name = feature.Name;
            Description = feature.Description;

            Options.Add(new OptionCheckBox(
                id: "IsEnabled",
                label: IsEnabledText,
                defaultValue: false,
                valueChangedCallback: value =>
                {
                    IsEnabled = value;
                    Feature.IsEnabled = value;
                    NotifyPropertyChanged(nameof(IsEnabled));
                }));
            Options.Add(new OptionSeparator());
            Options.Add(new OptionEntityList(
                id: "EnabledEntityList",
                entityList: feature.EntityList.OrderBy(entity => entity.Id).Select(entity => new ViewModelEntity(entity)),
                defaultEnabledList: new List<string>(),
                onEnabledListChanged: enabledList => Feature.EnabledEntityList = enabledList));

            InitSettings();
        }

        public List<IProtoEntity> GetEnabledEntityList()
        {
            return Options.OfType<OptionEntityList>().FirstOrDefault()?.SavedEnabledList;
        }
    }
}