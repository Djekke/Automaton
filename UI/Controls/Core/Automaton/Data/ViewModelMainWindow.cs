namespace CryoFall.Automaton.UI.Controls.Core.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using CryoFall.Automaton.UI.Controls.Core.Managers;
    using System.Collections.ObjectModel;

    public class ViewModelMainWindow : BaseViewModel
    {
        public ObservableCollection<IMainWindowListEntry> ListEntries { get; }

        public IMainWindowListEntry SelectedListEntry { get; set; }

        public bool IsEnabled => AutomatonManager.IsEnabled;

        public ViewModelMainWindow()
        {
            ListEntries = AutomatonManager.GetFeatures();
            ListEntries.Add(AutomatonManager.GetSettingsViewModel());
            AutomatonManager.IsEnabledChanged +=
                () => NotifyPropertyChanged(nameof(IsEnabled));
        }
    }
}