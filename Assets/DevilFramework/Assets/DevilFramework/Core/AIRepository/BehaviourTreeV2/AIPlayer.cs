namespace Devil.AI
{
    public abstract class AIPlayer<T> : BehaviourTreeRunner where T : AIPlayer<T>
    {
        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
            OnAIUpdate();
        }

        protected abstract void OnAIStart();

        protected abstract void OnAIUpdate();
    }
}