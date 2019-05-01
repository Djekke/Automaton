namespace CryoFall.Automaton.UI.Data.Options
{
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using System;

    public class OptionCheckBox : Option<bool>
    {
        public string Label { get; }

        public string ToolTip { get; }

        public OptionCheckBox(string id, string label, bool defaultValue, Action<bool> valueChangedCallback,
            string toolTip = "")
        {
            Id = id;
            Label = label;
            SavedValue = DefaultValue = defaultValue;
            OnValueChanged = valueChangedCallback;
        }

        public override void RegisterValueType(IClientStorage storage)
        {
            // bool already registred
        }
    }
}