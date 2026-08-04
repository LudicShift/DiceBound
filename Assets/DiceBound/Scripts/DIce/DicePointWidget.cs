using KCoreKit;

namespace DiceBound
{
    public class DicePointWidget : WidgetBase
    {
        private DiceWidget _dice;

        public void SetDice(DiceWidget dice)
        {
            _dice = dice;
        }

        public bool IsEmpty()
        {
            return !_dice;
        }

        public bool IsLinked(DiceWidget dice)
        {
            return dice == _dice;
        }
    }
}