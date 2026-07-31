using System.Linq;
using System.Collections.Generic;

namespace YatchDungeon
{
    public class SmallStraightCombination : CombinationBase
    {
        public SmallStraightCombination(int priority) : base(priority)
        {
        }

        public override bool Evaluate(CombinationContext context)
        {
            List<int> numbers = context.diceContexts.Select(d => d.number).Distinct().OrderBy(n => n).ToList();

            // Check for 1-2-3-4
            if (numbers.Contains(1) && numbers.Contains(2) && numbers.Contains(3) && numbers.Contains(4))
            {
                return true;
            }
            // Check for 2-3-4-5
            if (numbers.Contains(2) && numbers.Contains(3) && numbers.Contains(4) && numbers.Contains(5))
            {
                return true;
            }
            // Check for 3-4-5-6
            if (numbers.Contains(3) && numbers.Contains(4) && numbers.Contains(5) && numbers.Contains(6))
            {
                return true;
            }
            return false;
        }

        public override string GetName()
        {
            return "Small Straight";
        }
    }
}