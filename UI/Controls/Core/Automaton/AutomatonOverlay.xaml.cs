namespace CryoFall.Automaton.UI.Controls.Core
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using CryoFall.Automaton.ClientComponents.Actions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class AutomatonOverlay : BaseUserControl
    {
        private static AutomatonOverlay instance;

        private ClientInputContext inputContext;

        private FrameworkElement overlay;

        public static bool IsInstanceExist => instance != null;

        public static void Toggle()
        {
            if (IsInstanceExist)
            {
                DestroyInstance();
            }
            else
            {
                CreateInstance();
            }
        }

        public static void Close()
        {
            if (IsInstanceExist)
            {
                DestroyInstance();
            }
        }

        protected override void InitControl()
        {
            overlay = this.GetByName<FrameworkElement>("Overlay");
        }

        protected override void OnLoaded()
        {
            DataContext = ViewModelAutomatonOverlay.Instance;
            overlay.MouseLeftButtonDown += OverlayMouseLeftButtonDownHandler;
            inputContext = ClientInputContext
                                .Start("Automaton overlay")
                                .HandleButtonDown(GameButton.CancelOrClose, Close);
        }

        protected override void OnUnloaded()
        {
            ClientComponentAutomaton.SaveSettings();
            DataContext = null;
            overlay.MouseLeftButtonDown -= OverlayMouseLeftButtonDownHandler;
            inputContext.Stop();
            inputContext = null;
        }

        private static void CreateInstance()
        {
            if (instance != null)
            {
                return;
            }

            instance = new AutomatonOverlay();
            Api.Client.UI.LayoutRootChildren.Add(instance);
            Panel.SetZIndex(instance, 1001);
        }

        private static void DestroyInstance()
        {
            if (instance == null)
            {
                return;
            }

            ((Panel)instance.Parent).Children.Remove(instance);
            instance = null;
        }

        private void OverlayMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            DestroyInstance();
        }
    }
}