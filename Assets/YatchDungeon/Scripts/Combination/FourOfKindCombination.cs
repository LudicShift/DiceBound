using System.Collections.Generic;
using System.Linq;

namespace YatchDungeon
{
    public class FourOfKindCombination :CombinationBase
    {
        public FourOfKindCombination(CombinationDataTableRow priority) : base(priority)
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