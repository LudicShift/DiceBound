using System.Collections.Generic;
using System.Linq;

namespace YatchDungeon
{
    public class TwoPairCombination : CombinationBase
    {
        public TwoPairCombination(int priority) : base(priority)
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

        public override string GetName()
        {
            return "TwoPair";
        }
    }
}