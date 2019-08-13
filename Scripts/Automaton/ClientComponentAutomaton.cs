namespace CryoFall.Automaton
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using CryoFall.Automaton.Features;

    public class ClientComponentAutomaton : ClientComponent
    {
        public static ClientComponentAutomaton Instance { get; set; }

        public static double UpdateInterval => AutomatonManager.UpdateInterval;

        private double accumulatedTime = UpdateInterval;

        private readonly List<ProtoFeature> featuresList;

        public ClientComponentAutomaton() : base(isLateUpdateEnabled: false)
        {
            featuresList = AutomatonManager.GetFeatures();
        }

        public static void Init()
        {
            if (Instance != null)
            {
                Api.Logger.Error("Automaton: Instance already exist.");
            }

            Instance = Client.Scene.CreateSceneObject(nameof(ClientComponentAutomaton))
                .AddComponent<ClientComponentAutomaton>(AutomatonManager.IsEnabled);
        }

        protected override void OnDisable()
        {
            ReleaseSubscriptions();
            featuresList.ForEach(feature => feature.Stop());
        }

        protected override void OnEnable()
        {
            featuresList.ForEach(feature => feature.Start(this));
        }

        public override void Update(double deltaTime)
        {
            featuresList.ForEach(feature => feature.Update(deltaTime));

            accumulatedTime += deltaTime;
            if (accumulatedTime < UpdateInterval)
            {
                return;
            }
            accumulatedTime %= UpdateInterval;


            featuresList.ForEach(feature => feature.Execute());
        }
    }
}