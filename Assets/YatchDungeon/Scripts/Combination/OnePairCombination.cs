using System.Collections.Generic;
using System.Linq;

namespace YatchDungeon
{
    public class OnePairCombination:CombinationBase
    {
        public OnePairCombination(int priority) : base(priority)
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
                if (count == 2)
                {
                    pairCount++;
                }
            }
            return pairCount == 1;
        }

        public override string GetName()
        {
            return "One Pair";
        }
    }
}