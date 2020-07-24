namespace CryoFall.Automaton.Features
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using CryoFall.Automaton.ClientSettings;
    using CryoFall.Automaton.ClientSettings.Options;

    public interface IProtoFeature
    {
        /// <summary>
        /// Unique identifier representing this feature.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Feature name.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Feature description.
        /// </summary>
        string Description { get; }
        /// <summary>
        /// List of all options as <see cref="IOption"/> for this feature.
        /// </summary>
        List<IOption> Options { get; }

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        void Execute();
        void PrepareOptions(SettingsFeature settingsFeature);
        void PrepareProto();
        /// <summary>
        /// Init on component enabled.
        /// </summary>
        void Start(ClientComponent parentComponent);
        /// <summary>
        /// Stop everything.
        /// </summary>
        void Stop();
        /// <summary>
        /// Called by client component every tick.
        /// </summary>
        void Update(double deltaTime);
    }
}