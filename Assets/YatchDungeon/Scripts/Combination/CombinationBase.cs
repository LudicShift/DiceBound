namespace YatchDungeon
{
    public abstract class CombinationBase
    {
        public CombinationBase(int priority)
        {
            this._priority =  priority;
        }
        private int _priority;
        public abstract bool Evaluate(CombinationContext context);

        public int GetPriority()
        {
            return _priority;
        }

        public abstract string GetName();
    }
}