namespace AtomicTorch.CBND.Automaton.UI.Controls.Core
{
    using AtomicTorch.CBND.Automaton.ClientComponents.Actions;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using System;

    public class ViewModelAutomatonOverlay : BaseViewModel
    {
        private static ViewModelAutomatonOverlay instance;

        private ViewModelAutomatonOverlay()
        {
            if (instance != null)
            {
                throw new Exception("Instance already created");
            }

            ClientComponentAutomaton.IsAutoPickUpEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsAutoPickUpEnabled));

            ClientComponentAutomaton.IsAutoGatherEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsAutoGatherEnabled));

            ClientComponentAutomaton.IsAutoLootEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsAutoLootEnabled));

            ClientComponentAutomaton.IsAutoWoodcuttingEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsAutoWoodcuttingEnabled));

            ClientComponentAutomaton.IsAutoMiningEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsAutoMiningEnabled));
        }

        public static ViewModelAutomatonOverlay Instance
            => instance ?? (instance = new ViewModelAutomatonOverlay());

        public bool IsAutoPickUpEnabled
        {
            get => ClientComponentAutomaton.IsAutoPickUpEnabled;
            set => ClientComponentAutomaton.IsAutoPickUpEnabled = value;

        }

        public bool IsAutoGatherEnabled
        {
            get => ClientComponentAutomaton.IsAutoGatherEnabled;
            set => ClientComponentAutomaton.IsAutoGatherEnabled = value;

        }

        public bool IsAutoLootEnabled
        {
            get => ClientComponentAutomaton.IsAutoLootEnabled;
            set => ClientComponentAutomaton.IsAutoLootEnabled = value;

        }

        public bool IsAutoWoodcuttingEnabled
        {
            get => ClientComponentAutomaton.IsAutoWoodcuttingEnabled;
            set => ClientComponentAutomaton.IsAutoWoodcuttingEnabled = value;

        }

        public bool IsAutoMiningEnabled
        {
            get => ClientComponentAutomaton.IsAutoMiningEnabled;
            set => ClientComponentAutomaton.IsAutoMiningEnabled = value;

        }
    }
}