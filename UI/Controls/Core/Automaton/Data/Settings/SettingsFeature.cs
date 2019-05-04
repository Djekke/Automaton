namespace CryoFall.Automaton.UI.Data.Settings
{
    using AtomicTorch.CBND.GameApi.Data;
    using CryoFall.Automaton.UI.Data.Settings.Options;
    using CryoFall.Automaton.UI.Features;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SettingsFeature : ProtoSettings
    {
        public bool IsEnabled { get; private set; }

        public event Action<bool> IsEnabledChanged; 

        public string IsEnabledText => "Enable this feature";

        private ProtoFeature Feature;

        public SettingsFeature(string id, ProtoFeature feature)
        {
            Id = id;
            Feature = feature;
            Name = feature.Name;
            Description = feature.Description;

            Options.Add(new OptionCheckBox(
                parentSettings: this,
                id: "IsEnabled",
                label: IsEnabledText,
                defaultValue: false,
                valueChangedCallback: value =>
                {
                    Feature.IsEnabled = IsEnabled = value;
                    IsEnabledChanged?.Invoke(IsEnabled);
                }));
            Options.Add(new OptionSeparator());
            Options.Add(new OptionEntityList(
                id: "EnabledEntityList",
                entityList: feature.EntityList.OrderBy(entity => entity.Id).Select(entity => new ViewModelEntity(entity)),
                defaultEnabledList: new List<string>(),
                onEnabledListChanged: enabledList => Feature.EnabledEntityList = enabledList));
        }

        public List<IProtoEntity> GetEnabledEntityList()
        {
            return Options.OfType<OptionEntityList>().FirstOrDefault()?.SavedEnabledList;
        }
    }
}