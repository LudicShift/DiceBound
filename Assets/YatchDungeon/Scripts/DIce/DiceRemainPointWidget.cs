using KCoreKit;

namespace YatchDungeon
{
    public class DiceRemainPointWidget : WidgetBase
    {
        private DiceWidget _dice;

        public void SetDice(DiceWidget dice)
        {
            _dice = dice;
        }

        public bool IsLinked(DiceWidget dice)
        {
            return dice == _dice;
        }
    }
}