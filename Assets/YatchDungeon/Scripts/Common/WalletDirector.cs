using KCoreKit;
using UnityEngine;

namespace YatchDungeon
{
    public class WalletDirector : DirectorBase
    {
        [SerializeField]
        private TextWidget goldTextWidget;
        private int _gold;

        public bool HasGold(int gold)
        {
            return _gold >= gold;
        }

        public void SpendGold(int gold)
        {
            _gold -= gold;
            goldTextWidget.SetText(_gold.ToString());
        }

        public void AddGold(int gold)
        {
            _gold += gold;
            goldTextWidget.SetText(_gold.ToString());
        }
        
    }
}