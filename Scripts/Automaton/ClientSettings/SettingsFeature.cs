namespace CryoFall.Automaton.ClientSettings
{
    using System;
    using CryoFall.Automaton.Features;

    public class SettingsFeature : ProtoSettings
    {
        public bool IsEnabled { get; set; }

        public event Action<bool> IsEnabledChanged;

        public SettingsFeature(IProtoFeature feature)
        {
            id = feature.Id;
            name = feature.Name;
            description = feature.Description;

            feature.PrepareOptions(this);
            Options = feature.Options;
        }

        public void OnIsEnabledChanged(bool value)
        {
            IsEnabledChanged?.Invoke(value);
        }
    }
}