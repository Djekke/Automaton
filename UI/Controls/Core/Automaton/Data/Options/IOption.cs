namespace CryoFall.Automaton.UI.Controls.Core.Data.Options
{
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using System;

    public interface IOption
    {
        bool IsCosmetic { get; }

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