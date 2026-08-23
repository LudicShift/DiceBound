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
        private static CheatManager _instance;

        public void Start()
        {
            _instance = this;
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
            _instance.StartCoroutine(_asyncPvpDirector.PrepareOpponentBoard(waveIndex));
        }

        [DebugCommand]
        public static void PrintAsyncPvpOwnerId()
        {
            Debug.Log($"[AsyncPvp] 현재 UID: {_asyncPvpDirector.OwnerId}");
        }
    }
}
