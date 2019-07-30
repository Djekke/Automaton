namespace CryoFall.Automaton.UI
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using CryoFall.Automaton;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ViewModelMainWindow : BaseViewModel
    {
        public ObservableCollection<ViewModelSettings> AllSettings { get; }

        public ViewModelSettings SelectedSettings { get; set; }

        public bool IsEnabled => AutomatonManager.IsEnabled;

        public ViewModelMainWindow()
        {
            AllSettings = new ObservableCollection<ViewModelSettings>(
                AutomatonManager.GetAllSettings().Select(s => new ViewModelSettings(s)));
            AutomatonManager.IsEnabledChanged += OnIsEnabledChanged;
        }

        private void OnIsEnabledChanged()
        {
            NotifyPropertyChanged(nameof(IsEnabled));
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            foreach (var viewModelSettings in AllSettings)
            {
                viewModelSettings.Dispose();
            }

            AutomatonManager.IsEnabledChanged -= OnIsEnabledChanged;
        }
    }
}