using KCoreKit;

namespace YatchDungeon
{
    public class DiceKeepPointWidget : WidgetBase
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
    }
}