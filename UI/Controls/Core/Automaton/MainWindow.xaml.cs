namespace CryoFall.Automaton.UI.Controls.Core
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using CryoFall.Automaton.UI.Controls.Core.Data;
    using CryoFall.Automaton.UI.Controls.Core.Managers;

    public partial class MainWindow : BaseUserControlWithWindow
    {
        public static MainWindow Instance { get; private set; }

        public ViewModelMainWindow ViewModel { get; set; }

        public static void Toggle()
        {
            if (Instance?.IsOpened == true)
            {
                Instance.CloseWindow();
            }
            else
            {
                if (Instance == null)
                {
                    var instance = new MainWindow();
                    Instance = instance;
                    Api.Client.UI.LayoutRootChildren.Add(instance);
                }
                else
                {
                    Instance.Window.Open();
                }
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            Window.IsCached = true;
            DataContext = ViewModel = new ViewModelMainWindow();
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            DataContext = null;
            ViewModel = null;
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}