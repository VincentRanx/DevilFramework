namespace Devil.AI
{
    public abstract class BTServiceBase
    {
        public int Id { get; private set; }

        public BTServiceBase(int id)
        {
            Id = id;
        }

        public abstract void OnInitData(BehaviourTreeRunner btree, string jsonData);

        public abstract void OnClearData(BehaviourTreeRunner btree);

        public abstract void OnServiceTick(BehaviourTreeRunner btree, float deltaTime);

    }
}