using System.Collections.Generic;

namespace DiceBound
{
    public class TripleCombination : CombinationBase
    {
        public TripleCombination(CombinationDataTableRow priority) : base(priority)
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

            foreach (var count in counts.Values)
            {
                if (count >= 3)
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}