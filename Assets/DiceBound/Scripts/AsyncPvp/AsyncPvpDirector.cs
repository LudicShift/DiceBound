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
        private AsyncPvpBackendService _backendService;

        public string OwnerId => _backendService.OwnerId;

        public override IEnumerator OnInitialize()
        {
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();

            _backendService = new AsyncPvpBackendService();
            yield return _backendService.EnsureSignedIn();
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

        // 내 보드를 캡처해서 로컬 저장 + 서버 업로드하고, 서버에서 상대 스냅샷을 받아와 적 진영에 로드한다.
        // 서버 상대가 없으면(네트워크 실패, 또는 콜드 스타트) 로컬 풀로 폴백하고, 그마저 없으면 부전승(전투 스킵).
        public IEnumerator PrepareOpponentBoard(int waveIndex)
        {
            var mySnapshot = CaptureOwnBoardSnapshot(waveIndex);
            SaveOwnSnapshot(mySnapshot);
            yield return _backendService.UploadSnapshot(mySnapshot);

            UnitAsyncBoardData opponent = null;
            yield return _backendService.FetchOpponentSnapshot(waveIndex, result => opponent = result);

            if (opponent == null)
            {
                TryGetRandomSnapshot(waveIndex, out opponent);
            }

            if (opponent == null)
            {
                yield break;
            }

            LoadBoardSnapshot(opponent, UnitGroup.Enemy);
        }

        private static string GetSnapshotFileName(int waveIndex)
        {
            return $"wave_{waveIndex}_{Guid.NewGuid():N}.sav";
        }
    }
}
