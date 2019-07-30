namespace CryoFall.Automaton
{
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using CryoFall.Automaton.Features;
    using System;
    using System.Collections.Generic;

    public class ClientComponentAutomaton : ClientComponent
    {
        public static ClientComponentAutomaton Instance { get; private set; }

        public static double UpdateInterval => AutomatonManager.UpdateInterval;

        private double accumulatedTime = UpdateInterval;

        private List<ProtoFeature> featuresList;

        public ClientComponentAutomaton() : base(isLateUpdateEnabled: false)
        {
            if (Instance != null)
            {
                throw new Exception("Instance already exist");
            }

            featuresList = AutomatonManager.GetFeatures();
        }

        public static void Init()
        {
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