using ANU.IngameDebug.Console;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class CheatManager : MonoBehaviour
    {
        private static WalletDirector _walletDirector;

        public void Start()
        {
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
        }
    
        [DebugCommand]  
        public static void AddGold(int gold)
        {
            _walletDirector.AddGold(gold);
        }
    }
}
