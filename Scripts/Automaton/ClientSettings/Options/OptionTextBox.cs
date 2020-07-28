namespace CryoFall.Automaton.ClientSettings.Options
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class OptionTextBox<TValue> : Option<TValue>
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
            // string already registered
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
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 100
            };
            SetupOptionToControlValueBinding(textbox, TextBox.TextProperty);


            var mainGrid = new Grid()
            {
                Margin = new Thickness(0, 3, 0, 3)
            };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Star) });
            mainGrid.Children.Add(label);
            Grid.SetColumn(label, 0);
            mainGrid.Children.Add(textbox);
            Grid.SetColumn(textbox, 1);

            control = mainGrid;
        }
    }
}