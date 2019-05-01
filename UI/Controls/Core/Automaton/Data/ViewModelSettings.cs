namespace CryoFall.Automaton.UI.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using CryoFall.Automaton.UI.Data.Options;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public abstract class ViewModelSettings : BaseViewModel, IMainWindowListEntry
    {
        public ObservableCollection<IOption> Options { get; set; } = new ObservableCollection<IOption>();

        protected List<IOption> OptionsWithValue;

        public virtual string Id { get; protected set; }

        public virtual string Name { get; protected set; }

        public virtual string Description { get; protected set; }

        public bool IsModified => Options.Any(o => o.IsModified);

        protected IClientStorage clientStorage;

        protected string OptionsStorageLocalFilePath;

        public BaseCommand ApplyButton => new ActionCommand(ApplyAndSave);

        public BaseCommand CancelButton => new ActionCommand(Cancel);

        public ViewModelSettings()
        {
            var name = this.GetType().Name;
            Id = name.Replace("ViewModel", string.Empty);
        }

        protected void InitSettings()
        {
            OptionsWithValue = Options.Where(o => !o.IsCosmetic).ToList();
            OptionsWithValue.ForEach(o => o.OnIsModifiedChanged += () => NotifyPropertyChanged(nameof(IsModified)));

            OptionsStorageLocalFilePath = "Mods/Automaton/" + Id;
            RegisterStorage();
            LoadOptionsFromStorage();
        }

        public void Apply()
        {
            OptionsWithValue.ForEach(o => o.Apply());
        }

        public void Cancel()
        {
            OptionsWithValue.ForEach(o => o.Cancel());
        }

        public void ApplyAndSave()
        {
            Apply();

            SaveOptionsToStorage();
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
    }
}