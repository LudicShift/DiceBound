using System.Collections.Generic;

namespace DiceBound
{
    public class TwoPairCombination : CombinationBase
    {
        public TwoPairCombination(CombinationDataTableRow priority) : base(priority)
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
                if (count >= 2) // Changed from == 2 to >= 2 to correctly identify pairs even if there's a triple
                {
                    pairCount++;
                }
            }
            return pairCount >= 2; // Changed from == 2 to >= 2 to correctly identify two pairs
        }
    }
}