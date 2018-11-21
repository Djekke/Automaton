namespace CryoFall.Automaton.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using CryoFall.Automaton.ClientComponents.Actions;
    using CryoFall.Automaton.ClientComponents.Input;
    using CryoFall.Automaton.UI.Controls.Core;
    using CryoFall.Automaton.UI.Controls.Core.Managers;

    public class BootstrapperClientAutomaton : BaseBootstrapper
    {
        private static ClientInputContext gameplayInputContext;

        private static ClientComponentAutomaton clientComponentAutomaton;

        private static bool IsAutomatonEnabled = true;

        public override void ClientInitialize()
        {
            ClientInputManager.RegisterButtonsEnum<AutomatonButton>();

            AutomatonManager.Init();

            BootstrapperClientGame.InitCallback += GameInitHandler;

            BootstrapperClientGame.ResetCallback += ResetHandler;
        }

        private static void GameInitHandler(ICharacter currentCharacter)
        {
            clientComponentAutomaton = Api.Client.Scene.GetSceneObject(currentCharacter)
                .AddComponent<ClientComponentAutomaton>(IsAutomatonEnabled);

            gameplayInputContext = ClientInputContext
                .Start("Automaton options toggle")
                .HandleButtonDown(AutomatonButton.OpenSettings, AutomatonOverlay.Toggle)
                .HandleButtonDown(AutomatonButton.Toggle, () =>
                {
                    if (clientComponentAutomaton == null)
                    {
                        return;
                    }
                    IsAutomatonEnabled = !clientComponentAutomaton.IsEnabled;
                    clientComponentAutomaton.IsEnabled = IsAutomatonEnabled;
                    NotificationSystem.ClientShowNotification(
                        "Automaton is " + (IsAutomatonEnabled ? "enabled." : "disabled."));
                });
        }

        private static void ResetHandler()
        {
            clientComponentAutomaton?.Destroy();
            clientComponentAutomaton = null;

            gameplayInputContext?.Stop();
            gameplayInputContext = null;
        }
    }
}