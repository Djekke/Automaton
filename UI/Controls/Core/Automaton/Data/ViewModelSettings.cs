namespace CryoFall.Automaton.UI.Controls.Core.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using CryoFall.Automaton.UI.Controls.Core.Managers;

    public class ViewModelSettings : BaseViewModel, IMainWindowListEntry
    {
        private double updateInterval;

        public double UpdateInterval
        {
            get { return updateInterval; }
            set
            {
                if (value == updateInterval)
                {
                    return;
                }

                updateInterval = value;
                NotifyThisPropertyChanged();
            }
        }

        public string UpdateIntervalText => "Update interval (s)";

        public string UpdateIntervalToolTip => "How often mod will attempt to do something.";

        public string Name => "Settings";

        public string Description => "Global mod settings.";

        public BaseCommand Apply { get; }

        public BaseCommand Cancel { get; }

        public ViewModelSettings()
        {
            updateInterval = AutomatonManager.UpdateInterval;
            Apply = new ActionCommand(() =>
            {
                AutomatonManager.UpdateInterval = updateInterval;
                NotifyPropertyChanged(nameof(UpdateInterval));
                // settings saved on window close, no need to call SaveSettings here.
            });
            Cancel = new ActionCommand(() =>
            {
                // reset any changed settings
                updateInterval = AutomatonManager.UpdateInterval;
                NotifyPropertyChanged(nameof(UpdateInterval));
                // close window?
            });
        }
    }
}