namespace CryoFall.Automaton.UI.Controls.Core
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using CryoFall.Automaton.UI.Controls.Core.Data;

    public partial class AutomatonOverlay : BaseUserControlWithWindow
    {
        public static AutomatonOverlay Instance { get; private set; }

        public ViewModelAutomatonSettings ViewModel { get; set; }

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
                    var instance = new AutomatonOverlay();
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
            DataContext = ViewModel = new ViewModelAutomatonSettings();
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