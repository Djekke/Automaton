namespace CryoFall.Automaton.ClientSettings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using CryoFall.Automaton.ClientSettings.Options;

    public abstract class ProtoSettings
    {
        public List<IOption> Options { get; set; } = new List<IOption>();

        protected List<IOptionWithValue> optionsWithValue;

        public bool HasOptionsWithValue => optionsWithValue.Any();

        public virtual string Id => id;

        public virtual string Name => name;

        public virtual string Description => description;

        protected string id;

        protected string name;

        protected string description;

        public bool IsModified => optionsWithValue.Any(o => o.IsModified);

        public event Action OnIsModifiedChanged;


        protected IClientStorage clientStorage;

        protected string optionsStorageLocalFilePath;

        protected ProtoSettings()
        {
            id = this.GetType().Name;
        }

        public void InitSettings()
        {
            optionsWithValue = Options.OfType<IOptionWithValue>().ToList();

            optionsStorageLocalFilePath = "Mods/Automaton/" + Id;
            RegisterStorage();
            LoadOptionsFromStorage();
        }

        public void ApplyAndSave()
        {
            optionsWithValue.ForEach(o => o.Apply());

            SaveOptionsToStorage();
        }

        public void Cancel()
        {
            optionsWithValue.ForEach(o => o.Cancel());
        }

        public void Reset()
        {
            optionsWithValue.ForEach(o => o.Reset(apply: false));
            ApplyAndSave();
        }

        public void RegisterStorage()
        {
            clientStorage = Api.Client.Storage.GetStorage(optionsStorageLocalFilePath);
            foreach (var option in optionsWithValue)
            {
                option.RegisterValueType(clientStorage);
            }
        }

        public void LoadOptionsFromStorage()
        {
            if (optionsWithValue.Count == 0)
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

            var optionsToProcess = optionsWithValue.ToList();
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
            if (optionsWithValue.Count == 0)
            {
                return;
            }

            var snapshot = new Dictionary<string, object>();
            foreach (var option in optionsWithValue)
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