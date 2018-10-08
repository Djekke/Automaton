namespace CryoFall.Automaton.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
    using CryoFall.Automaton.ClientComponents.Input;
    using CryoFall.Automaton.UI.Controls.Core;

    public class BootstrapperClientAutomaton : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            ClientInputManager.RegisterButtonsEnum<AutomatonButton>();

            ClientInputContext.Start("Automaton options toggle")
                              .HandleButtonDown(AutomatonButton.OpenSettings, AutomatonOverlay.Toggle);
        }
    }
}