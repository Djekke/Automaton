namespace CryoFall.Automaton.ClientSettings
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using CryoFall.Automaton.ClientSettings.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ProtoSettings
    {
        public List<IOption> Options { get; set; } = new List<IOption>();

        protected List<IOptionWithValue> OptionsWithValue;

        public virtual string Id { get; protected set; }

        public virtual string Name { get; protected set; }

        public virtual string Description { get; protected set; }

        public bool IsModified => OptionsWithValue.Any(o => o.IsModified);

        public event Action OnIsModifiedChanged;

        protected IClientStorage clientStorage;

        protected string OptionsStorageLocalFilePath;

        public ProtoSettings()
        {
            Id = this.GetType().Name;
        }

        public void InitSettings()
        {
            OptionsWithValue = Options.OfType<IOptionWithValue>().ToList();

            OptionsStorageLocalFilePath = "Mods/Automaton/" + Id;
            RegisterStorage();
            LoadOptionsFromStorage();
        }

        public void ApplyAndSave()
        {
            OptionsWithValue.ForEach(o => o.Apply());

            SaveOptionsToStorage();
        }

        public void Cancel()
        {
            OptionsWithValue.ForEach(o => o.Cancel());
        }

        public void Reset()
        {
            OptionsWithValue.ForEach(o => o.Reset(apply: false));
            ApplyAndSave();
        }

        public void RegisterStorage()
        {
            clientStorage = Api.Client.Storage.GetStorage(OptionsStorageLocalFilePath);
            foreach (var option in OptionsWithValue)
            {
                option.RegisterValueType(clientStorage);
            }
        }

        public void LoadOptionsFromStorage()
        {
            if (OptionsWithValue.Count == 0)
            {
                return;
            }

            if (!clientStorage.TryLoad<Dictionary<string, object>>(out var snapshot))
            {
                Api.Logger.Important(
                    $"There are no options snapshot for {Id} or it cannot be deserialized - applying default values");
                Reset();
                return;
            }

            var optionsToProcess = OptionsWithValue.ToList();
            foreach (var pair in snapshot)
            {
                for (var index = 0; index < optionsToProcess.Count; index++)
                {
                    var option = optionsToProcess[index];
                    if (option.Id != pair.Key)
                    {
                        continue;
                    }

                    // found a value of option
                    option.ApplyAbstractValue(pair.Value);
                    optionsToProcess.RemoveAt(index);
                    index--;
                }
            }

            // reset all the remaining options (don't found values from them in snapshot)
            foreach (var option in optionsToProcess)
            {
                option.Reset(apply: true);
            }
        }

        private void SaveOptionsToStorage()
        {
            if (OptionsWithValue.Count == 0)
            {
                return;
            }

            var snapshot = new Dictionary<string, object>();
            foreach (var option in OptionsWithValue)
            {
                snapshot[option.Id] = option.GetAbstractValue();
            }

            clientStorage.Save(snapshot);
        }

        private void NotifyModified()
        {
            OnIsModifiedChanged?.Invoke();
        }

        public void OnOptionModified(IOptionWithValue option)
        {
            NotifyModified();
        }
    }
}