namespace CryoFall.Automaton.UI.Data.Options
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using System;

    public abstract class Option<TValue> : BaseViewModel, IOption
    {
        private TValue currentValue;

        protected TValue SavedValue;

        // Shouild be init in constructor
        protected TValue DefaultValue;

        protected Action<TValue> OnValueChanged;

        private bool isCurrentValueInitialized = false;

        public bool IsModified => !CurrentValue.Equals(SavedValue);

        public event Action OnIsModifiedChanged;

        public virtual bool IsCosmetic => false;

        public string Id { get; protected set; }

        public TValue CurrentValue
        {
            get { return currentValue; }
            set
            {
                if (value.Equals(currentValue) && isCurrentValueInitialized)
                {
                    return;
                }

                currentValue = value;
                isCurrentValueInitialized = true;
                NotifyThisPropertyChanged();
                OnIsModifiedChanged?.Invoke();
                NotifyPropertyChanged(nameof(IsModified));
            }
        }

        public virtual void Apply()
        {
            SavedValue = CurrentValue;
            OnValueChanged?.Invoke(CurrentValue);
            OnIsModifiedChanged?.Invoke();
            NotifyPropertyChanged(nameof(IsModified));
        }

        public virtual void Cancel()
        {
            CurrentValue = SavedValue;
        }

        public virtual void Reset(bool apply)
        {
            CurrentValue = DefaultValue;
            OnIsModifiedChanged?.Invoke();
            NotifyPropertyChanged(nameof(IsModified));
            if (apply)
            {
                Apply();
            }
        }

        public abstract void RegisterValueType(IClientStorage storage);

        public virtual void ApplyAbstractValue(object value)
        {
            if (value is TValue casted)
            {
                CurrentValue = casted;
                Apply();
                return;
            }

            Api.Logger.Warning(
                $"Option {Id} cannot apply abstract value - type mismatch. Will reset option to the default value");
            Reset(apply: true);
        }

        public virtual object GetAbstractValue()
        {
            return SavedValue;
        }
    }
}