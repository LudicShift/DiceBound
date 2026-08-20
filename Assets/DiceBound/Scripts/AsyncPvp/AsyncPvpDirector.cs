using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;

namespace DiceBound
{
    public class AsyncPvpDirector : DirectorBase
    {
        private const string MySnapshotDirectory = "AsyncPvp/MySnapshots";

        private UnitDirector _unitDirector;
        private UnitPlaceDirector _unitPlaceDirector;

        public override IEnumerator OnInitialize()
        {
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            yield return null;
        }

        public UnitAsyncBoardData CaptureOwnBoardSnapshot(int waveIndex)
        {
            var board = new UnitAsyncBoardData
            {
                waveIndex = waveIndex,
                capturedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            foreach (var unit in _unitDirector.GetAllies())
            {
                board.units.Add(new UnitAsyncData
                {
                    unitId = unit.GetId(),
                    tier = unit.GetTier(),
                    cellIndex = _unitPlaceDirector.GetCellIndex(UnitGroup.Ally, unit)
                });
            }

            return board;
        }

        public void LoadBoardSnapshot(UnitAsyncBoardData snapshot, UnitGroup targetGroup)
        {
            foreach (var data in snapshot.units)
            {
                var unit = _unitDirector.SpawnUnit(data.unitId, targetGroup, false, data.tier);
                var cell = _unitPlaceDirector.GetCell(targetGroup, data.cellIndex);
                _unitPlaceDirector.PlaceUnit(unit, cell);
            }
        }

        public void SaveOwnSnapshot(UnitAsyncBoardData snapshot)
        {
            SaveSystem.Save(snapshot, GetSnapshotFileName(snapshot.waveIndex), MySnapshotDirectory, true);
        }

        // 해당 라운드에 대해 지금까지 저장된 스냅샷들(=풀) 중 하나를 무작위로 골라 반환한다.
        // Phase 2에서는 이 로컬 풀 대신 Firestore의 Cloud Function 조회로 교체될 지점이다.
        public bool TryGetRandomSnapshot(int waveIndex, out UnitAsyncBoardData snapshot)
        {
            List<UnitAsyncBoardData> pool;
            try
            {
                SaveSystem.LoadAll(MySnapshotDirectory, out pool);
            }
            catch (Exception)
            {
                snapshot = null;
                return false;
            }

            var candidates = pool.Where(x => x.waveIndex == waveIndex).ToList();
            snapshot = candidates.GetRandomElement();
            return snapshot != null;
        }

        // Phase 1: 아직 Firebase 연동 전이라 네트워크 상대가 없음.
        // 로컬에 저장해둔 스냅샷 풀 중 하나를 무작위로 상대로 삼아 로직을 테스트한다.
        // Phase 2에서 이 메서드를 업로드/조회 흐름으로 교체할 예정.
        public void PrepareOpponentBoard(int waveIndex)
        {
            var mySnapshot = CaptureOwnBoardSnapshot(waveIndex);
            SaveOwnSnapshot(mySnapshot);

            if (TryGetRandomSnapshot(waveIndex, out var opponent))
            {
                LoadBoardSnapshot(opponent, UnitGroup.Enemy);
            }
        }

        private static string GetSnapshotFileName(int waveIndex)
        {
            return $"wave_{waveIndex}_{Guid.NewGuid():N}.sav";
        }
    }
}
