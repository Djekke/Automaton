namespace CryoFall.Automaton.ClientComponents.Actions
{
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using CryoFall.Automaton.UI.Features;
    using CryoFall.Automaton.UI.Managers;
    using System;
    using System.Collections.Generic;

    public class ClientComponentAutomaton : ClientComponent
    {
        public static ClientComponentAutomaton Instance { get; private set; }

        public static double UpdateInterval => AutomatonManager.UpdateInterval;

        private double accumulatedTime = UpdateInterval;

        private Dictionary<string, ProtoFeature> featuresDictionary;

        public ClientComponentAutomaton() : base(isLateUpdateEnabled: false)
        {
            if (Instance != null)
            {
                throw new Exception("Instance already exist");
            }

            featuresDictionary = AutomatonManager.GetFeaturesDictionary();
        }

        public static void Init()
        {
            Instance = Client.Scene.CreateSceneObject(nameof(ClientComponentAutomaton))
                .AddComponent<ClientComponentAutomaton>(AutomatonManager.IsEnabled);
        }

        protected override void OnDisable()
        {
            ReleaseSubscriptions();
            foreach (var feature in featuresDictionary.Values)
            {
                feature.Stop();
            }
        }

        protected override void OnEnable()
        {
            foreach (var feature in featuresDictionary.Values)
            {
                feature.Start(this);
            }
        }

        public override void Update(double deltaTime)
        {
            foreach (var feature in featuresDictionary.Values)
            {
                feature.Update(deltaTime);
            }

            accumulatedTime += deltaTime;
            if (accumulatedTime < UpdateInterval)
            {
                return;
            }
            accumulatedTime %= UpdateInterval;


            foreach (var feature in featuresDictionary.Values)
            {
                feature.Execute();
            }
        }
    }
}