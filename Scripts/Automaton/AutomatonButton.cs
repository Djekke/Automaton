﻿namespace CryoFall.Automaton
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    [NotPersistent]
    public enum AutomatonButton
    {
        [Description("Open mod settings")]
        [ButtonInfo(InputKey.F6, Category = "Automaton")]
        OpenSettings,

        [Description("Toggle mod features")]
        [ButtonInfo(InputKey.X, Category = "Automaton")]
        Toggle,
    }
}
