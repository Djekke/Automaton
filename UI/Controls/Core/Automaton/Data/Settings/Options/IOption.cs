namespace CryoFall.Automaton.UI.Data.Settings.Options
{
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using System;
    using System.Windows;

    public interface IOption
    {
        void CreateControl(out FrameworkElement control);
    }

    public interface IOptionWithValue : IOption
    {
        bool IsModified { get; }

        event Action OnIsModifiedChanged;

        string Id { get; }

        void Apply();

        void Cancel();

        void Reset(bool apply);

        void ApplyAbstractValue(object value);

        object GetAbstractValue();

        void RegisterValueType(IClientStorage storage);
    }
}