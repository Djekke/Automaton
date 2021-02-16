namespace CryoFall.Automaton.ClientSettings.Options
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;
    using CryoFall.Automaton.UI;
    using CryoFall.Automaton.UI.Data;

    public class OptionEntityList : IOptionWithValue
    {
        protected ProtoSettings parentSettings;

        private readonly OptionValueHolder optionValueHolder;

        public ProtoSettings ParentSettings => parentSettings;

        public bool IsModified => optionValueHolder.EntityCollection.Any(viewModelEntity =>
            EntityDictionary[viewModelEntity.Entity] != viewModelEntity.IsEnabled);

        public Dictionary<IProtoEntity, bool> EntityDictionary { get; }

        public string Id { get; }

        private readonly List<string> defaultEnabledList;

        private event Action<List<IProtoEntity>> OnEnabledListChanged;

        public OptionEntityList(
            ProtoSettings parentSettings,
            string id,
            IEnumerable<IProtoEntity> entityList,
            List<string> defaultEnabledList,
            Action<List<IProtoEntity>> onEnabledListChanged)
        {
            this.parentSettings = parentSettings;
            Id = id;
            this.defaultEnabledList = defaultEnabledList;
            EntityDictionary = new Dictionary<IProtoEntity, bool>();
            foreach (var entity in entityList)
            {
                EntityDictionary[entity] = (defaultEnabledList?.Count > 0) && defaultEnabledList.Contains(entity.Id);
            }
            OnEnabledListChanged = onEnabledListChanged;
            optionValueHolder = new OptionValueHolder(this, EntityDictionary);
        }

        public void Apply()
        {
            // Get data from UI
            foreach (var viewModelEntity in optionValueHolder.EntityCollection)
            {
                EntityDictionary[viewModelEntity.Entity] = viewModelEntity.IsEnabled;
            }
            OnEnabledListChanged?.Invoke(GetEnabledEntityList());
            parentSettings.OnOptionModified(this);
        }

        public void Cancel()
        {
            foreach (var viewModelEntity in optionValueHolder.EntityCollection)
            {
                viewModelEntity.IsEnabled = EntityDictionary[viewModelEntity.Entity];
            }
        }

        private List<IProtoEntity> GetEnabledEntityList()
        {
            return EntityDictionary.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
        }

        private List<string> GetEnabledEntityIdList()
        {
            return EntityDictionary.Where(pair => pair.Value).Select(pair => pair.Key.Id).ToList();
        }

        public void Reset(bool apply)
        {
            ApplyAbstractValue(defaultEnabledList);
        }

        public void ApplyAbstractValue(object value)
        {
            if (value is List<string> enabledList)
            {
                foreach (var viewModelEntity in optionValueHolder.EntityCollection)
                {
                    viewModelEntity.IsEnabled = (enabledList.Count > 0) && enabledList.Contains(viewModelEntity.Id);
                }
                Apply();
                return;
            }

            Api.Logger.Warning(
                $"Automaton: Option {Id} cannot apply abstract value - type mismatch. Will reset option to the default value");
            Reset(apply: true);
        }

        public object GetAbstractValue()
        {
            return GetEnabledEntityIdList();
        }

        public void RegisterValueType(IClientStorage storage)
        {
            // List<string> should be already registered.
            // storage.RegisterType(typeof(List<string>));
        }

        public void CreateControl(out FrameworkElement control)
        {
            //<DataTemplate DataType="{x:Type options:OptionEntityList}">
            //    <Grid>
            //        <Grid.RowDefinitions>
            //            <RowDefinition Height="*" />
            //            <RowDefinition Height="Auto" />
            //        </Grid.RowDefinitions>
            //
            //        <ItemsControl Grid.Row="0"
            //                      ItemsSource="{Binding EntityCollection}"
            //                      HorizontalAlignment="Center">
            //            <ItemsControl.ItemsPanel>
            //                <ItemsPanelTemplate>
            //                    <StackPanel Orientation="Vertical" IsItemsHost="True" />
            //                </ItemsPanelTemplate>
            //            </ItemsControl.ItemsPanel>
            //            <ItemsControl.Template>
            //                <ControlTemplate TargetType="{x:Type ItemsControl}">
            //                    <ScrollViewer>
            //                        <ItemsPresenter />
            //                    </ScrollViewer>
            //                </ControlTemplate>
            //            </ItemsControl.Template>
            //        </ItemsControl>
            //
            //        <Grid Grid.Row="1"
            //              Margin="0,5,0,5">
            //            <Grid.ColumnDefinitions>
            //                <ColumnDefinition Width="*" />
            //                <ColumnDefinition Width="*" />
            //            </Grid.ColumnDefinitions>
            //
            //            <Button Grid.Column="0"
            //                    HorizontalAlignment="Center"
            //                    Content="{x:Static data:AutomatonStrings.Button_SelectAll}"
            //                    IsEnabled="{Binding HasUnselected}"
            //                    Command="{Binding SelectAll}" />
            //
            //            <Button Grid.Column="1"
            //                    HorizontalAlignment="Center"
            //                    Content="{x:Static data:AutomatonStrings.Button_DeselectAll}"
            //                    IsEnabled="{Binding HasSelected}"
            //                    Command="{Binding DeselectAll}" />
            //        </Grid>
            //    </Grid>
            //</DataTemplate>

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            mainGrid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            mainGrid.DataContext = optionValueHolder;

            var itemsControl = new ItemsControl()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, nameof(optionValueHolder.EntityCollection));
            mainGrid.Children.Add(itemsControl);

            var buttonGrid = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(50, GridUnitType.Star)});
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(50, GridUnitType.Star)});

            var selectAllButton = new Button()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = AutomatonStrings.Button_SelectAll,
            };
            selectAllButton.SetBinding(UIElement.IsEnabledProperty, nameof(optionValueHolder.HasUnselected));
            selectAllButton.SetBinding(ButtonBase.CommandProperty, nameof(optionValueHolder.SelectAll));
            buttonGrid.Children.Add(selectAllButton);

            var deselectAllButton = new Button()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = AutomatonStrings.Button_DeselectAll,
            };
            deselectAllButton.SetBinding(UIElement.IsEnabledProperty, nameof(optionValueHolder.HasSelected));
            deselectAllButton.SetBinding(ButtonBase.CommandProperty, nameof(optionValueHolder.DeselectAll));
            buttonGrid.Children.Add(deselectAllButton);
            Grid.SetColumn(deselectAllButton, 1);

            mainGrid.Children.Add(buttonGrid);
            Grid.SetRow(buttonGrid, 1);

            control = mainGrid;
        }

        /// <summary>
        /// Option value holder is used for data binding between UI control and the option.
        /// </summary>
        protected class OptionValueHolder : INotifyPropertyChanged
        {
            private readonly OptionEntityList owner;

            // Suppress all but one OnEntityIsEnabledChanged for foreach cases.
            private bool collectionReset = false;

            public OptionValueHolder(OptionEntityList owner, Dictionary<IProtoEntity, bool> initialValue)
            {
                this.owner = owner;
                EntityCollection =
                    new ObservableCollection<ViewModelEntity>(initialValue.Select(pair =>
                        new ViewModelEntity(pair.Key, pair.Value)));
                foreach (var viewModelEntity in EntityCollection)
                {
                    viewModelEntity.IsEnabledChanged += OnEntityIsEnabledChanged;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public ObservableCollection<ViewModelEntity> EntityCollection { get; }

            public BaseCommand SelectAll => new ActionCommand(() =>
            {
                collectionReset = true;
                EntityCollection.ForEach(v => v.IsEnabled = true);
                collectionReset = false;
                OnEntityIsEnabledChanged();
            });

            public bool HasUnselected => EntityCollection.Any(e => !e.IsEnabled);

            public BaseCommand DeselectAll => new ActionCommand(() =>
            {
                collectionReset = true;
                EntityCollection.ForEach(v => v.IsEnabled = false);
                collectionReset = false;
                OnEntityIsEnabledChanged();
            });

            public bool HasSelected => EntityCollection.Any(e => e.IsEnabled);

            private void OnEntityIsEnabledChanged()
            {
                if (!collectionReset)
                {
                    owner.parentSettings.OnOptionModified(owner);
                    NotifyPropertyChanged(nameof(HasSelected));
                    NotifyPropertyChanged(nameof(HasUnselected));
                }
            }

            private void NotifyPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}