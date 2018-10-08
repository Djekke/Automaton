namespace CryoFall.Automaton.ClientComponents.Input
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    [NotPersistent]
    public enum AutomatonButton
    {
        [ButtonInfo(
            "Open mod settings",
            "Open Automaton settings menu",
            InputKey.F6,
            Category = "Automaton")]
        OpenSettings,

        [ButtonInfo(
            "Toggle mod features",
            "Turn on or off all features granted by this mod.",
            InputKey.X,
            Category = "Automaton")]
        Toggle,
    }
}
