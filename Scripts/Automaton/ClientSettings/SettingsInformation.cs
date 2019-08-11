namespace CryoFall.Automaton.ClientSettings
{
    using AtomicTorch.GameEngine.Common.Extensions;
    using CryoFall.Automaton.ClientSettings.Options;

    public class SettingsInformation : ProtoSettings
    {
        public override string Id => "ModInformation";

        public override string Name => "Information";

        public override string Description => "Automaton Information";

        public string AvaiableKeybindings => "Avaiable keybindings";

        public string AllKeybindingsConfigurable => "All keybindings is configurable in game settings.";

        public string Links => "Links";

        public string ForumThread => "Forum thread";

        private string forumLink = "http://forums.atomictorch.com/index.php?topic=1097.0";

        public string GithubLink => "Github link";

        private string githubLink = "https://github.com/Djekke/Automaton";

        public SettingsInformation()
        {
            Options.Add(new OptionInformationText("[b]" + AvaiableKeybindings + ":[/b]"));
            Options.Add(new OptionSeparator());
            Options.Add(new OptionButtonInformation(AutomatonButton.OpenSettings,
                AutomatonButton.OpenSettings.GetDescription()));
            Options.Add(new OptionButtonInformation(AutomatonButton.Toggle,
                AutomatonButton.Toggle.GetDescription()));
            Options.Add(new OptionSeparator());
            Options.Add(new OptionInformationText(
                "[b]" + AllKeybindingsConfigurable + "[/b]",
                12));
            Options.Add(new OptionSeparatorWithTitle("[b]" + Links + ":[b]"));
            Options.Add(new OptionInformationText(
                ForumThread + ": [url=" + forumLink + "]" + forumLink + "[/url]",
                12));
            Options.Add(new OptionInformationText(
                GithubLink + ": [url=" + githubLink + "]" + githubLink + "[/url]",
                12));
        }
    }
}