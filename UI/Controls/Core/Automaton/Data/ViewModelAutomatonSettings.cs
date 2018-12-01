namespace CryoFall.Automaton.UI.Controls.Core.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using CryoFall.Automaton.UI.Controls.Core.Managers;
    using System.Collections.ObjectModel;

    public class ViewModelAutomatonSettings : BaseViewModel
    {
        public ObservableCollection<ViewModelFeature> Features { get; }

        public ViewModelFeature SelectedFeature { get; set; }

        public bool IsEnabled => AutomatonManager.IsEnabled;

        public ViewModelAutomatonSettings()
        {
            Features = AutomatonManager.GetFeatures();
            AutomatonManager.IsEnabledChanged +=
                () => NotifyPropertyChanged(nameof(IsEnabled));
        }
    }
}