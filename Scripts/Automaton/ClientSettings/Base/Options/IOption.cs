namespace CryoFall.Automaton.ClientSettings.Options
{
    using System.Windows;

    public interface IOption
    {
        void CreateControl(out FrameworkElement control);
    }
}