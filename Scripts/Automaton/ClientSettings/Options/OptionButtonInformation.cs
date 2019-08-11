namespace CryoFall.Automaton.ClientSettings.Options
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using CryoFall.Automaton.UI.Helpers;

    public class OptionButtonInformation : IOption
    {
        public string Text { get; }

        public AutomatonButton ButtonRef { get; }

        public OptionButtonInformation(AutomatonButton buttonRef, string text)
        {
            ButtonRef = buttonRef;
            Text = text;
        }

        public void CreateControl(out FrameworkElement control)
        {
            var buttonReferenceControl = new AutomatonButtonReference() {Button = ButtonRef};
            var labelWithButton = new LabelWithButton() {Button = buttonReferenceControl};
            var textControl = new FormattedTextBlock()
            {
                Content = Text,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 1),
            };
            stackPanel.Children.Add(labelWithButton);
            stackPanel.Children.Add(textControl);

            control = stackPanel;
        }
    }
}