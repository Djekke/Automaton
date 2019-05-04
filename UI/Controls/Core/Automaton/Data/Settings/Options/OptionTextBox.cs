namespace CryoFall.Automaton.UI.Data.Settings.Options
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class OptionTextBox<TValue> : Option<TValue>, IOptionWithValue
    {
        public string Label { get; }

        public string ToolTip { get; }

        public OptionTextBox(
            ProtoSettings parentSettings,
            string id,
            string label,
            TValue defaultValue,
            Action<TValue> valueChangedCallback,
            string toolTip = "")
            : base(parentSettings, id, defaultValue, valueChangedCallback)
        {
            Label = label;
            ToolTip = toolTip;
        }

        public override void RegisterValueType(IClientStorage storage)
        {
            // string already registred
        }

        protected override void CreateControlInternal(out FrameworkElement control)
        {
            //<DataTemplate DataType="{x:Type options:OptionTextBoxDouble}">
            //    <StackPanel Orientation="Horizontal">
            //    <TextBlock Text="{Binding Label}"
            //               Margin="0,0,10,0"
            //               base:ToolTipServiceExtend.ToolTip="{Binding Tooltip}" />
            //    <TextBox Text="{Binding CurrentValue, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
            //             Width="75" />
            //    </StackPanel>
            //</DataTemplate>

            var label = new FormattedTextBlock()
            {
                Content = Label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            if (ToolTip != "")
            {
                ToolTipServiceExtend.SetToolTip(label, ToolTip);
            }

            var textbox = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Width = 75
            };
            SetupOptionToControlValueBinding(textbox, TextBox.TextProperty);

            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(textbox);

            control = stackPanel;
        }
    }
}