namespace CryoFall.Automaton.ClientSettings.Options
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class OptionInformationText : IOption
    {
        public string Text { get; }

        public double FontSize { get; }

        public OptionInformationText(string text, double fontSize = 14)
        {
            Text = text;
            FontSize = fontSize;
        }

        public void CreateControl(out FrameworkElement control)
        {
            var textControl = new FormattedTextBlock()
            {
                Content = Text,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = FontSize,
            };

            control = textControl;
        }
    }
}