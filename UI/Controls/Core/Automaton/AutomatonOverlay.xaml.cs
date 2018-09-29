namespace AtomicTorch.CBND.Automaton.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.Automaton.ClientComponents.Actions;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.DebugTools;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

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
                if (DebugToolsOverlay.IsInstanceExist)
                {
                    CreateInstance();
                }
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
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            var sceneObject = Api.Client.Scene.GetSceneObject(character);
            sceneObject.AddComponent<ClientComponentAutomaton>();
            this.overlay = this.GetByName<FrameworkElement>("Overlay");
        }

        protected override void OnLoaded()
        {
            this.DataContext = ViewModelAutomatonOverlay.Instance;
            this.overlay.MouseLeftButtonDown += this.OverlayMouseLeftButtonDownHandler;
            this.inputContext = ClientInputContext
                                .Start("Automaton overlay")
                                .HandleButtonDown(GameButton.CancelOrClose, () => Close());
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.overlay.MouseLeftButtonDown -= this.OverlayMouseLeftButtonDownHandler;
            this.inputContext.Stop();
            this.inputContext = null;
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