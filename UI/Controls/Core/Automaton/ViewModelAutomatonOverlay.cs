namespace CryoFall.Automaton.UI.Controls.Core
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using CryoFall.Automaton.ClientComponents.Actions;
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
                += () => NotifyPropertyChanged(nameof(IsAutoPickUpEnabled));

            ClientComponentAutomaton.IsAutoGatherEnabledChanged
                += () => NotifyPropertyChanged(nameof(IsAutoGatherEnabled));

            ClientComponentAutomaton.IsAutoLootEnabledChanged
                += () => NotifyPropertyChanged(nameof(IsAutoLootEnabled));

            ClientComponentAutomaton.IsAutoWoodcuttingEnabledChanged
                += () => NotifyPropertyChanged(nameof(IsAutoWoodcuttingEnabled));

            ClientComponentAutomaton.IsAutoMiningEnabledChanged
                += () => NotifyPropertyChanged(nameof(IsAutoMiningEnabled));
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