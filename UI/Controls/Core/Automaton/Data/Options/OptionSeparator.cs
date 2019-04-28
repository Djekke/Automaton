namespace CryoFall.Automaton.UI.Controls.Core.Data.Options
{
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using System;

    public class OptionSeparator : IOption
    {
        public bool IsCosmetic => true;

        public bool IsModified => false;

        public event Action OnIsModifiedChanged;

        public string Id => "";

        public void Apply()
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void ApplyAbstractValue(object value)
        {
            throw new NotImplementedException();
        }

        public object GetAbstractValue()
        {
            throw new NotImplementedException();
        }

        public void RegisterValueType(IClientStorage storage)
        {
            throw new NotImplementedException();
        }

        public void Reset(bool apply)
        {
            throw new NotImplementedException();
        }
    }
}