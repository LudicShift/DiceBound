using KCoreKit;

namespace YatchDungeon
{
    public class WalletDirector : DirectorBase
    {
        private int _gold;

        public bool HasGold(int gold)
        {
            return _gold >= gold;
        }

        public void SpendGold(int gold)
        {
            _gold -= gold;
        }

        public void AddGold(int gold)
        {
            _gold += gold;
        }
        
    }
}