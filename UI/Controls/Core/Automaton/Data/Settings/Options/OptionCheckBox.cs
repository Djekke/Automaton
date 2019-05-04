namespace CryoFall.Automaton.UI.Data.Settings.Options
{
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public class OptionCheckBox : Option<bool>
    {
        public string Label { get; }

        public string ToolTip { get; }

        public OptionCheckBox(
            ProtoSettings parentSettings,
            string id,
            string label,
            bool defaultValue,
            Action<bool> valueChangedCallback,
            string toolTip = "")
            : base(parentSettings, id, defaultValue, valueChangedCallback)
        {
            Label = label;
            ToolTip = toolTip;
        }

        public override void RegisterValueType(IClientStorage storage)
        {
            // bool already registred
        }

        protected override void CreateControlInternal(out FrameworkElement control)
        {
            //<DataTemplate DataType="{x:Type options:OptionCheckBox}">
            //    <CheckBox IsChecked="{Binding CurrentValue}"
            //              Content="{Binding Label}"
            //              base:ToolTipServiceExtend.ToolTip="{Binding ToolTip}" />
            //</DataTemplat>

            var checkbox = new CheckBox()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Content = Label,
                Margin = new Thickness(0, 5, 0, 5)
            };

            SetupOptionToControlValueBinding(checkbox, ToggleButton.IsCheckedProperty);

            if (ToolTip != "")
            {
                ToolTipServiceExtend.SetToolTip(checkbox, ToolTip);
            }

            control = checkbox;
        }
    }
}