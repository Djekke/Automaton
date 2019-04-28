namespace CryoFall.Automaton.UI.Controls.Core.Data.Options
{
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using System;

    public class OptionTextBox<TValue> : Option<TValue>, IOption
    {
        public string Label { get; }

        public string ToolTip { get; }

        public OptionTextBox(string id, string label, TValue defaultValue, Action<TValue> valueChangedCallback,
            string toolTip = "")
        {
            Id = id;
            Label = label;
            ToolTip = toolTip;
            SavedValue = DefaultValue = defaultValue;
            OnValueChanged = valueChangedCallback;
        }

        public override void RegisterValueType(IClientStorage storage)
        {
            // string already registred
        }
    }

    public class OptionTextBoxDouble : OptionTextBox<double>
    {
        public OptionTextBoxDouble(string id, string label, double defaultValue, Action<double> valueChangedCallback,
            string toolTip = "") : base(id, label, defaultValue, valueChangedCallback, toolTip)
        {
        }
    }
}