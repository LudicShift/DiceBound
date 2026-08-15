using ANU.IngameDebug.Console;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class CheatManager : MonoBehaviour
    {
        private static MasteryManager _masteryManager;
        private static WalletDirector _walletDirector;

        public void Start()
        {
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _masteryManager = MasteryManager.GetInstance();
        }
    
        [DebugCommand]  
        public static void AddGold(int gold)
        {
            _walletDirector.AddGold(gold);
        }
        [DebugCommand]  
        public static void AddDiamonds(int diamonds)
        {
            _masteryManager.AddDiamond(diamonds);
        }
    }
}
