using System.Collections.Generic;

namespace DiceBound
{
    public class FourOfKindCombination :CombinationBase
    {
        public FourOfKindCombination(CombinationDataTableRow priority) : base(priority)
        {
        }

        public override bool Evaluate(CombinationContext context)
        {
            var counts = new Dictionary<int, int>();
            foreach (var diceContext in context.diceContexts)
            {
                if (!counts.TryAdd(diceContext.number, 1))
                {
                    counts[diceContext.number]++;
                }
            }

            foreach (var count in counts.Values)
            {
                if (count >= 4)
                {
                    return true;
                }
            }
            return false;
        }

    }
}