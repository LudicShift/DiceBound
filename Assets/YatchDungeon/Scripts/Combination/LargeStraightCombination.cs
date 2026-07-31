using System.Linq;
using System.Collections.Generic;

namespace YatchDungeon
{
    public class LargeStraightCombination : CombinationBase
    {
        public LargeStraightCombination(CombinationDataTableRow priority) : base(priority)
        {
        }

        public override bool Evaluate(CombinationContext context)
        {
            List<int> numbers = context.diceContexts.Select(d => d.number).OrderBy(n => n).ToList();
            
            // Check for 2-3-4-5-6
            if (numbers.SequenceEqual(new List<int> { 2, 3, 4, 5, 6 }))
            {
                return true;
            }
            // Check for 1-2-3-4-5
            if (numbers.SequenceEqual(new List<int> { 1, 2, 3, 4, 5 }))
            {
                return true;
            }
            return false;
        }
        
    }
}