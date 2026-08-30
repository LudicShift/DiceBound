using ANU.IngameDebug.Console;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class CheatManager : MonoBehaviour
    {
        private static WalletDirector _walletDirector;
        private static AsyncPvpDirector _asyncPvpDirector;
        private static CheatManager _instance;

        public void Start()
        {
            _instance = this;
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _asyncPvpDirector = DirectorFacade.GetDirector<AsyncPvpDirector>();
        }

        [DebugCommand]
        public static void AddGold(int gold)
        {
            _walletDirector.AddGold(gold);
        }
    }
}
