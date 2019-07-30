namespace CryoFall.Automaton.ClientSettings.Options
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using System.Windows;
    using System.Windows.Controls;

    public class OptionSeparatorWithTitle : IOption
    {
        public string Title { get; }

        public OptionSeparatorWithTitle(string title)
        {
            Title = title;
        }

        public void CreateControl(out FrameworkElement control)
        {
            var separatorControl1 = new Control()
            {
                Style = Api.Client.UI.GetApplicationResource<Style>("ControlHorizontalSeparator")
            };

            var titleControl = new TextBlock()
            {
                Text = Title,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var separatorControl2 = new Control()
            {
                Style = Api.Client.UI.GetApplicationResource<Style>("ControlHorizontalSeparator")
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            mainGrid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            mainGrid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            mainGrid.Children.Add(separatorControl1);
            Grid.SetRow(separatorControl1, 0);
            mainGrid.Children.Add(titleControl);
            Grid.SetRow(titleControl, 1);
            mainGrid.Children.Add(separatorControl2);
            Grid.SetRow(separatorControl2, 2);

            control = mainGrid;
        }
    }
}