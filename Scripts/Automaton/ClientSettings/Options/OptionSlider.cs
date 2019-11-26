namespace CryoFall.Automaton.ClientSettings.Options
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class OptionSlider : Option<double>
    {
        public string Label { get; }

        public string ToolTip { get; }

        public double MinValue { get; }

        public double MaxValue { get; }

        public double StepSize { get; }

        public OptionSlider(
            ProtoSettings parentSettings,
            string id,
            string label,
            double defaultValue,
            Action<double> valueChangedCallback,
            double minValue = 0.0,
            double stepSize = 0.05,
            double maxValue = 1.0,
            string toolTip = "")
            : base(parentSettings, id, defaultValue, valueChangedCallback)
        {
            Label = label;
            ToolTip = toolTip;
            MinValue = minValue;
            MaxValue = maxValue;
            StepSize = stepSize;
        }

        public override void RegisterValueType(IClientStorage storage)
        {
            // double already registered.
        }

        protected override void CreateControlInternal(out FrameworkElement control)
        {
            var labelControl = new TextBlock()
            {
                Text = Label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 7, 0, 0)
            };

            var largeStep = StepSize * 2.0;
            var sliderControl = new Slider
            {
                Minimum = MinValue,
                Maximum = MaxValue,
                SmallChange = StepSize,
                LargeChange = largeStep,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 200,
                Margin = new Thickness(0, 2, 0, -3),
                IsSnapToTickEnabled = false,
                TickFrequency = largeStep,
            };
            var valueToolTip = new TextBlock();
            valueToolTip.SetBinding(TextBlock.TextProperty, new Binding("Value") { StringFormat = "N2"});
            valueToolTip.DataContext = sliderControl;
            ToolTipServiceExtend.SetToolTip(sliderControl, valueToolTip);

            if (ToolTip != "")
            {
                ToolTipServiceExtend.SetToolTip(labelControl, ToolTip);
            }

            SetupOptionToControlValueBinding(sliderControl, RangeBase.ValueProperty);

            var mainGrid = new Grid()
            {
                Margin = new Thickness(0, 3, 0, 3)
            };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(100, GridUnitType.Star)});
            mainGrid.Children.Add(labelControl);
            Grid.SetColumn(labelControl, 0);
            mainGrid.Children.Add(sliderControl);
            Grid.SetColumn(sliderControl, 1);

            control = mainGrid;
        }
    }
}