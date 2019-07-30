namespace CryoFall.Automaton.ClientSettings
{
    using CryoFall.Automaton;
    using CryoFall.Automaton.ClientSettings.Options;

    public class SettingsGlobal : ProtoSettings
    {
        public string UpdateIntervalText => "Update interval (s)";

        public string UpdateIntervalToolTip => "How often mod will attempt to do something.";

        public override string Id => "GlobalSettings";

        public override string Name => "Settings";

        public override string Description => "Global mod settings.";

        public SettingsGlobal()
        {
            Options.Add(new OptionTextBox<double>(
                parentSettings: this,
                id: "UpdateInterval",
                label: UpdateIntervalText,
                defaultValue: 0.5d,
                valueChangedCallback: (val) => AutomatonManager.UpdateInterval = val,
                toolTip: UpdateIntervalToolTip));
        }
    }
}