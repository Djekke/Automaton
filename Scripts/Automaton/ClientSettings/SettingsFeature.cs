namespace CryoFall.Automaton.ClientSettings
{
    using CryoFall.Automaton.Features;
    using System;

    public class SettingsFeature : ProtoSettings
    {
        public bool IsEnabled { get; set; }

        public event Action<bool> IsEnabledChanged; 

        private readonly ProtoFeature Feature;

        public SettingsFeature(ProtoFeature feature)
        {
            Id = feature.Id;
            Feature = feature;
            Name = feature.Name;
            Description = feature.Description;

            feature.PrepareOptions(this);
            Options = feature.Options;
        }

        public void OnIsEnabledChanged(bool value)
        {
            IsEnabledChanged?.Invoke(value);
        }
    }
}