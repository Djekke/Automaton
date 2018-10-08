namespace CryoFall.Automaton.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using CryoFall.Automaton.ClientComponents.Actions;
    using CryoFall.Automaton.ClientComponents.Input;
    using CryoFall.Automaton.UI.Controls.Core;

    public class BootstrapperClientAutomaton : BaseBootstrapper
    {
        private static ClientInputContext gameplayInputContext;

        public override void ClientInitialize()
        {
            ClientInputManager.RegisterButtonsEnum<AutomatonButton>();

            BootstrapperClientGame.InitCallback += GameInitHandler;

            BootstrapperClientGame.ResetCallback += ResetHandler;
        }

        private static void GameInitHandler(ICharacter currentCharacter)
        {
            gameplayInputContext = ClientInputContext
                .Start("Automaton options toggle")
                .HandleButtonDown(AutomatonButton.OpenSettings, AutomatonOverlay.Toggle);

            Api.Client.Scene.GetSceneObject(currentCharacter)
                .AddComponent<ClientComponentAutomaton>();
        }

        private static void ResetHandler()
        {
            gameplayInputContext?.Stop();
            gameplayInputContext = null;
        }
    }
}