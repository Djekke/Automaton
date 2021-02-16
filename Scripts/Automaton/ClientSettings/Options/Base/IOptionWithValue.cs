namespace CryoFall.Automaton.ClientSettings.Options
{
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public interface IOptionWithValue : IOption
    {
        bool IsModified { get; }

        string Id { get; }

        ProtoSettings ParentSettings { get; }

        void Apply();

        void Cancel();

        void Reset(bool apply);

        void ApplyAbstractValue(object value);

        object GetAbstractValue();

        void RegisterValueType(IClientStorage storage);
    }
}