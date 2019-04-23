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

        public const string Notification_ModEnabled = "Automaton is enabled.";

        public const string Notification_ModDisabled = "Automaton is disabled.";

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
                .AddComponent<ClientComponentAutomaton>(AutomatonManager.IsEnabled);

            gameplayInputContext = ClientInputContext
                .Start("Automaton options toggle")
                .HandleButtonDown(AutomatonButton.OpenSettings, MainWindow.Toggle)
                .HandleButtonDown(AutomatonButton.Toggle, () =>
                {
                    if (clientComponentAutomaton == null)
                    {
                        return;
                    }
                    AutomatonManager.IsEnabled = !clientComponentAutomaton.IsEnabled;
                    clientComponentAutomaton.IsEnabled = AutomatonManager.IsEnabled;
                    NotificationSystem.ClientShowNotification(
                        AutomatonManager.IsEnabled ? Notification_ModEnabled : Notification_ModDisabled);
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