using System;
using System.Collections.Generic;

namespace YatchDungeon
{
    public class CombinationContext
    {
        public List<DiceContext> diceContexts;

        public CombinationContext(List<DiceWidget> allDices)
        {
            diceContexts = new List<DiceContext>();
            foreach (var dice in allDices)
            {
                diceContexts.Add(new DiceContext(dice));
            }
        }
    }
}