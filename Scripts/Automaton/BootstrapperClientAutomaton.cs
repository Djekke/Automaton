namespace CryoFall.Automaton
{
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using CryoFall.Automaton.UI;

    public class BootstrapperClientAutomaton : BaseBootstrapper
    {
        private static ClientInputContext gameplayInputContext;

        public override void ClientInitialize()
        {
            ClientInputManager.RegisterButtonsEnum<AutomatonButton>();

            AutomatonManager.Init();

            BootstrapperClientGame.InitCallback += GameInitHandler;

            BootstrapperClientGame.ResetCallback += ResetHandler;
        }

        private static void GameInitHandler(ICharacter currentCharacter)
        {
            ClientComponentAutomaton.Init();

            gameplayInputContext = ClientInputContext
                .Start("Automaton options toggle")
                .HandleButtonDown(AutomatonButton.OpenSettings, MainWindow.Toggle)
                .HandleButtonDown(AutomatonButton.Toggle, () =>
                { AutomatonManager.IsEnabled = !AutomatonManager.IsEnabled; });
        }

        private static void ResetHandler()
        {
            ClientComponentAutomaton.Instance?.Destroy();

            gameplayInputContext?.Stop();
            gameplayInputContext = null;
        }
    }
}