namespace CryoFall.Automaton.ClientSettings.Options
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class OptionSeparator : IOption
    {
        public void CreateControl(out FrameworkElement control)
        {
            //<DataTemplate DataType="{x:Type options:OptionSeparator}">
            //    <Control Style="{StaticResource ControlHorizontalSeparator}" />
            //</DataTemplate>

            var separatorControl = new Control()
            {
                Style = Api.Client.UI.GetApplicationResource<Style>("ControlHorizontalSeparator")
            };

            control = separatorControl;
        }
    }
}