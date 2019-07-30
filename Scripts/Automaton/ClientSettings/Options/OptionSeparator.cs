namespace CryoFall.Automaton.ClientSettings.Options
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using System.Windows;
    using System.Windows.Controls;

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