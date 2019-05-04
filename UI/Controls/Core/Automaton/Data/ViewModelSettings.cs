namespace CryoFall.Automaton.UI.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using CryoFall.Automaton.UI.Data.Settings;
    using System.Collections.ObjectModel;
    using System.Windows;

    public class ViewModelSettings : BaseViewModel
    {
        private readonly ProtoSettings owner;

        public ObservableCollection<FrameworkElement> OptionsControls { get; }

        public bool? IsEnabled { get; private set; }

        public string Name { get; }

        public string Description { get; }

        public bool IsModified => owner.IsModified;

        public BaseCommand ApplyButton => new ActionCommand(owner.ApplyAndSave);

        public BaseCommand CancelButton => new ActionCommand(owner.Cancel);

        public ViewModelSettings(ProtoSettings owner)
        {
            this.owner = owner;
            Name = owner.Name;
            Description = owner.Description;
            owner.OnIsModifiedChanged += OnIsModifiedChanged;
            if (owner is SettingsFeature settingsFeature)
            {
                IsEnabled = settingsFeature.IsEnabled;
                settingsFeature.IsEnabledChanged += OnIsEnabledChanged;
            }

            OptionsControls = new ObservableCollection<FrameworkElement>();
            foreach (var option in owner.Options)
            {
                option.CreateControl(out var optionControl);
                OptionsControls.Add(optionControl);
            }
        }

        private void OnIsEnabledChanged(bool value)
        {
            IsEnabled = value;
            //Api.Logger.Dev("IsEnabled = " + value + " settings = " + owner.Name);
            NotifyPropertyChanged(nameof(IsEnabled));
        }

        private void OnIsModifiedChanged()
        {
            NotifyPropertyChanged(nameof(IsModified));
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            owner.OnIsModifiedChanged -= OnIsModifiedChanged;
            if (owner is SettingsFeature settingsFeature)
            {
                settingsFeature.IsEnabledChanged -= OnIsEnabledChanged;
            }
        }
    }
}