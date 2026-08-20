using ANU.IngameDebug.Console;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class CheatManager : MonoBehaviour
    {
        private static MasteryManager _masteryManager;
        private static WalletDirector _walletDirector;
        private static AsyncPvpDirector _asyncPvpDirector;

        public void Start()
        {
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _masteryManager = MasteryManager.GetInstance();
            _asyncPvpDirector = DirectorFacade.GetDirector<AsyncPvpDirector>();
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

        [DebugCommand]
        public static void CaptureAsyncPvpSnapshot(int waveIndex)
        {
            var snapshot = _asyncPvpDirector.CaptureOwnBoardSnapshot(waveIndex);
            _asyncPvpDirector.SaveOwnSnapshot(snapshot);
        }

        [DebugCommand]
        public static void LoadAsyncPvpSnapshotAsEnemy(int waveIndex)
        {
            if (_asyncPvpDirector.TryGetRandomSnapshot(waveIndex, out var snapshot))
            {
                _asyncPvpDirector.LoadBoardSnapshot(snapshot, UnitGroup.Enemy);
            }
        }

        [DebugCommand]
        public static void PlaySelfAsyncPvpRound(int waveIndex)
        {
            _asyncPvpDirector.PrepareOpponentBoard(waveIndex);
        }
    }
}
