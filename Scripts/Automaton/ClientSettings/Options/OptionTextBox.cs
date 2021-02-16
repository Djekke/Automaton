namespace CryoFall.Automaton.ClientSettings.Options
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class OptionTextBox<TValue> : Option<TValue>
    {
        public string Label { get; }

        public string ToolTip { get; }

        private OptionTextBoxValueHolder optionTextBoxValueHolder;

        public override bool IsModified =>
            optionTextBoxValueHolder.StringValue != Convert.ToString(CurrentValue, CultureInfo.CurrentUICulture);

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
            optionTextBoxValueHolder = new OptionTextBoxValueHolder(this, defaultValue);
            optionValueHolder = optionTextBoxValueHolder;
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

        public override void Apply()
        {
            CurrentValue = optionTextBoxValueHolder.Value;
            ResetStringValue();
            onValueChanged?.Invoke(CurrentValue);
            ParentSettings.OnOptionModified(this);
        }

        public override void Cancel()
        {
            optionTextBoxValueHolder.Value = CurrentValue;
            ResetStringValue();
        }

        public override void Reset(bool apply)
        {
            optionTextBoxValueHolder.Value = defaultValue;
            optionTextBoxValueHolder.StringValue = Convert.ToString(defaultValue, CultureInfo.CurrentUICulture);
            if (apply)
            {
                Apply();
            }
        }

        private void ResetStringValue()
        {
            optionTextBoxValueHolder.StringValue = optionTextBoxValueHolder.ResetValue();
        }

        protected override void SetupOptionToControlValueBinding(FrameworkElement control, DependencyProperty valueProperty)
        {
            control.SetBinding(valueProperty, new Binding()
            {
                Path = new PropertyPath(nameof(optionTextBoxValueHolder.StringValue)),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });
            control.DataContext = optionTextBoxValueHolder;
        }

        /// <summary>
        /// Option value holder is used for data binding between UI control and the option.
        /// </summary>
        protected class OptionTextBoxValueHolder : OptionValueHolder
        {
            private string stringValue;

            public OptionTextBoxValueHolder(Option<TValue> owner, TValue initialValue) : base(owner, initialValue)
            {
                stringValue = Convert.ToString(initialValue, CultureInfo.CurrentUICulture);
            }

            public override TValue Value
            {
                get => this.value;
                set
                {
                    if (EqualityComparer<TValue>.Default.Equals(value, Value))
                    {
                        return;
                    }

                    this.value = value;
                    StringValue = ResetValue();

                    owner.ParentSettings.OnOptionModified(owner);

                    // call property changed notification to notify UI about the change
                    NotifyPropertyChanged(nameof(Value));
                }
            }

            public string StringValue
            {
                get => stringValue;
                set
                {
                    if (value == stringValue)
                    {
                        return;
                    }
                    stringValue = value;

                    owner.ParentSettings.OnOptionModified(owner);

                    // call property changed notification to notify UI about the change
                    NotifyPropertyChanged(nameof(StringValue));

                    try
                    {
                        var convertedValue = (TValue)Convert.ChangeType(value, typeof(TValue), CultureInfo.CurrentUICulture);
                        if (!EqualityComparer<TValue>.Default.Equals(convertedValue, Value))
                        {
                            Value = convertedValue;
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
            }

            public string ResetValue()
            {
                return Convert.ToString(Value, CultureInfo.CurrentUICulture);
            }
        }
    }
}