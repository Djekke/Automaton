namespace CryoFall.Automaton.ClientSettings
{
    using AtomicTorch.GameEngine.Common.Extensions;
    using CryoFall.Automaton.ClientSettings.Options;

    public class SettingsInformation : ProtoSettingsSingleton<SettingsInformation>
    {
        public override string Id => "ModInformation";

        public override string Name => "Information";

        public override string Description => "Automaton Information";

        public override int Order => 20;

        public string AvailableKeybindings => "Available keybindings";

        public string AllKeybindingsConfigurable => "All keybindings is configurable in game settings.";

        public string Links => "Links";

        public string ForumThread => "Forum thread";

        private string forumLink = "http://forums.atomictorch.com/index.php?topic=1097.0";

        public string GithubLink => "Github link";

        private string githubLink = "https://github.com/Djekke/Automaton";

        private SettingsInformation()
        {
            Options.Add(new OptionInformationText("[b]" + AvailableKeybindings + ":[/b]"));
            Options.Add(new OptionSeparator());
            Options.Add(new OptionButtonInformation(AutomatonButton.OpenSettings,
                AutomatonButton.OpenSettings.GetDescription()));
            Options.Add(new OptionButtonInformation(AutomatonButton.Toggle,
                AutomatonButton.Toggle.GetDescription()));
            Options.Add(new OptionSeparator());
            Options.Add(new OptionInformationText(
                text: "[b]" + AllKeybindingsConfigurable + "[/b]",
                fontSize: 12));
            Options.Add(new OptionSeparatorWithTitle("[b]" + Links + ":[b]"));
            Options.Add(new OptionInformationText(
                text: ForumThread + ": [url=" + forumLink + "]" + forumLink + "[/url]",
                fontSize: 12));
            Options.Add(new OptionInformationText(
                text: GithubLink + ": [url=" + githubLink + "]" + githubLink + "[/url]",
                fontSize: 12));
        }
    }
}