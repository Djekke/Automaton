namespace CryoFall.Automaton.UI.Data.Settings.Options
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Data;

    public abstract class Option<TValue> : IOptionWithValue
    {
        private OptionValueHolder optionValueHolder;

        protected TValue SavedValue { get; private set; }

        // Shouild be init in constructor
        protected TValue DefaultValue;

        protected Action<TValue> OnValueChanged;

        protected ProtoSettings ParentSettings;

        private bool isCurrentValueInitialized = false;

        public bool IsModified => !CurrentValue.Equals(SavedValue);

        public event Action OnIsModifiedChanged;

        public virtual bool IsCosmetic => false;

        public string Id { get; protected set; }

        public TValue CurrentValue
        {
            get { return optionValueHolder.Value; }
            set
            {
                if (value.Equals(CurrentValue) && isCurrentValueInitialized)
                {
                    return;
                }

                isCurrentValueInitialized = true;
                optionValueHolder.Value = value;
                //OnIsModifiedChanged?.Invoke();
            }
        }

        protected virtual void OnCurrentValueChanged(bool fromUi)
        {
        }

        public Option(ProtoSettings parentSettings)
        {
            ParentSettings = parentSettings;
            optionValueHolder = new OptionValueHolder(this, DefaultValue);
        }

        public virtual void Apply()
        {
            SavedValue = CurrentValue;
            OnValueChanged?.Invoke(CurrentValue);
            OnIsModifiedChanged?.Invoke();
        }

        public virtual void Cancel()
        {
            CurrentValue = SavedValue;
        }

        public virtual void Reset(bool apply)
        {
            CurrentValue = DefaultValue;
            OnIsModifiedChanged?.Invoke();
            if (apply)
            {
                Apply();
            }
        }

        public abstract void RegisterValueType(IClientStorage storage);

        public virtual void ApplyAbstractValue(object value)
        {
            //Api.Logger.Dev("Set value = " + value + " for option = " + Id + " for settings = " + ParentSettings.Name);
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

        public void CreateControl(out FrameworkElement control)
        {
            CreateControlInternal(out control);
        }

        protected abstract void CreateControlInternal(out FrameworkElement control);

        protected void SetupOptionToControlValueBinding(FrameworkElement control, DependencyProperty valueProperty)
        {
            //control.SetBinding(valueProperty, "Value");
            control.SetBinding(valueProperty, new Binding()
            {
                Path = new PropertyPath("Value"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            control.DataContext = optionValueHolder;
        }

        /// <summary>
        /// Option value holder is used for data binding between UI control and the option.
        /// </summary>
        protected class OptionValueHolder : INotifyPropertyChanged
        {
            private readonly Option<TValue> owner;

            private TValue value;

            public OptionValueHolder(Option<TValue> owner, TValue initialValue)
            {
                this.owner = owner;
                value = initialValue;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public TValue Value
            {
                get => value;
                set
                {
                    if (EqualityComparer<TValue>.Default.Equals(value, this.value))
                    {
                        return;
                    }

                    this.value = value;

                    // call property changed notification to notify UI about the change
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

                    owner.ParentSettings.OnOptionModified(owner);
                }
            }
        }
    }
}