namespace CryoFall.Automaton.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using CryoFall.Automaton.ClientSettings;
    using CryoFall.Automaton.ClientSettings.Options;

    public abstract class ProtoFeature<T> : IProtoFeature
        where T : class
    {
        /// <summary>
        /// Static instance. Needs to use lambda expression
        /// to construct an instance (since constructor is private).
        /// </summary>
        private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOfT());

        /// <summary>
        /// Gets the instance of this singleton.
        /// </summary>
        public static T Instance { get { return sInstance.Value; } }

        /// <summary>
        /// Creates an instance of T via reflection since T's constructor is expected to be private.
        /// </summary>
        /// <returns></returns>
        private static T CreateInstanceOfT()
        {
            return Activator.CreateInstance(typeof(T), true) as T;
        }

        /// <summary>
        /// List of all enabled Entity as <see cref="IProtoEntity"/> from EntityList that enabled in settings. 
        /// </summary>
        public List<IProtoEntity> EnabledEntityList { get; set; }

        public List<string> DefaultEnabledList { get; protected set; } = new List<string>();

        /// <summary>
        /// List of all options as <see cref="IOption"/> for this feature.
        /// </summary>
        public List<IOption> Options { get; protected set; } = new List<IOption>();

        /// <summary>
        /// Unique identifier representing this feature.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Feature name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Feature description.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// List of items as <see cref="IProtoEntity"/> which activate feature.
        /// </summary>
        public List<IProtoEntity> RequiredItemList;

        /// <summary>
        /// Is this feature enabled.
        /// </summary>
        public virtual bool IsEnabled { get; set; }

        public string IsEnabledText => "Enable this feature";

        /// <summary>
        /// List of all entity as <see cref="IProtoEntity"/> of interest for this feature.
        /// </summary>
        public List<IProtoEntity> EntityList;

        protected IItem SelectedItem => ClientHotbarSelectedItemManager.SelectedItem;

        protected ICharacter CurrentCharacter => Api.Client.Characters.CurrentPlayerCharacter;

        protected PlayerCharacterPrivateState PrivateState => PlayerCharacter.GetPrivateState(CurrentCharacter);

        public void PrepareProto()
        {
            var name = this.GetType().Name;
            Id = name.Replace("Feature", "");

            var entityList = new List<IProtoEntity>();
            var requiredItemList = new List<IProtoEntity>();

            PrepareFeature(entityList, requiredItemList);

            EntityList = entityList;
            RequiredItemList = requiredItemList;
        }

        protected abstract void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList);

        public virtual void PrepareOptions(SettingsFeature settingsFeature)
        {
            AddOptionIsEnabled(settingsFeature);
            Options.Add(new OptionSeparator());
            AddOptionEntityList(settingsFeature);
        }

        protected void AddOptionIsEnabled(SettingsFeature settingsFeature)
        {
            Options.Add(new OptionCheckBox(
                parentSettings: settingsFeature,
                id: "IsEnabled",
                label: IsEnabledText,
                defaultValue: false,
                valueChangedCallback: value =>
                {
                    settingsFeature.IsEnabled = IsEnabled = value;
                    settingsFeature.OnIsEnabledChanged(value);
                }));
        }

        protected void AddOptionEntityList(SettingsFeature settingsFeature)
        {
            Options.Add(new OptionEntityList(
                parentSettings: settingsFeature,
                id: "EnabledEntityList",
                entityList: EntityList.OrderBy(entity => entity.Id),
                defaultEnabledList: DefaultEnabledList,
                onEnabledListChanged: enabledList => EnabledEntityList = enabledList));
        }

        /// <summary>
        /// Called by client component every tick.
        /// </summary>
        public virtual void Update(double deltaTime)
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                return;
            }
            // Check queue and do something.
        }

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        public virtual void Execute()
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                return;
            }
            // Check surrounding
            // Add action to queue
        }

        /// <summary>
        /// Check initial condition (right tool equipped or other checks).
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckPrecondition()
        {
            if (RequiredItemList?.Count == 0)
            {
                return true;
            }
            var selectedProtoItem = SelectedItem?.ProtoItem;
            if (selectedProtoItem != null && RequiredItemList.Contains(selectedProtoItem))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stop everything.
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// Init on component enabled.
        /// </summary>
        public virtual void Start(ClientComponent parentComponent)
        {
            SetupSubscriptions(parentComponent);
        }

        /// <summary>
        /// Setup any of subscriptions
        /// </summary>
        public virtual void SetupSubscriptions(ClientComponent parentComponent)
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
