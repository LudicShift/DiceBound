namespace YatchDungeon
{
    public class FiveKindCombination :  CombinationBase
    {
        public FiveKindCombination(CombinationDataTableRow priority) : base(priority)
        {
            
        }

        public override bool Evaluate(CombinationContext context)
        {
            var number = context.diceContexts[0].number;
            foreach (var diceContext in context.diceContexts)
            {
                if (diceContext.number != number)
                {
                    return false;
                }
            }
            return true;
        }

    }
}