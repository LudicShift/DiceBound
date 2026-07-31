using System.Collections.Generic;
using System.Linq;

namespace YatchDungeon
{
    public class FullHouseCombination : CombinationBase
    {
        public FullHouseCombination(CombinationDataTableRow priority) : base(priority)
        {
        }

        public override bool Evaluate(CombinationContext context)
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            foreach (var diceContext in context.diceContexts)
            {
                if (counts.ContainsKey(diceContext.number))
                {
                    counts[diceContext.number]++;
                }
                else
                {
                    counts.Add(diceContext.number, 1);
                }
            }

            bool hasThreeOfKind = false;
            bool hasPair = false;

            foreach (var count in counts.Values)
            {
                if (count == 3)
                {
                    hasThreeOfKind = true;
                }
                else if (count == 2)
                {
                    hasPair = true;
                }
            }
            return hasThreeOfKind && hasPair;
        }

    }
}