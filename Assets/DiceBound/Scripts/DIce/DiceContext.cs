namespace DiceBound
{
    public class DiceContext
    {
        public int number;

        public DiceContext(DiceWidget dice)
        {
            number = dice.GetNumber();
        }
    }
}