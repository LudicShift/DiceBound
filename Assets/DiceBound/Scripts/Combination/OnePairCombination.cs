using System.Collections.Generic;

namespace DiceBound
{
    public class OnePairCombination:CombinationBase
    {
        public OnePairCombination(CombinationDataTableRow priority) : base(priority)
        {
        }

        public override bool Evaluate(CombinationContext context)
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            foreach (var diceContext in context.diceContexts)
            {
                if (!counts.TryAdd(diceContext.number, 1))
                {
                    counts[diceContext.number]++;
                }
            }

            int pairCount = 0;
            foreach (var count in counts.Values)
            {
                if (count == 2)
                {
                    pairCount++;
                }
            }
            return pairCount >= 1;
        }
        
    }
}