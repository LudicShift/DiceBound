using KCoreKit;

namespace DiceBound
{
    public enum RoundType
    {
        Creep,
        Pvp
    }

    public class WaveDataTableRow : DataTableRowBase
    {
        public int index;
        public int numberOfEnemy;
        public int waveRewardGold;
        public int waveRewardDiamond;
        public int enemyStarTier;
        public RoundType roundType;
    }
}