using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KCoreKit;
using NUnit.Framework.Internal;

namespace DiceBound
{
    public class AsyncPvpDirector : DirectorBase
    {
        private const string MySnapshotDirectory = "AsyncPvp/MySnapshots";

        private UnitDirector _unitDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        private AsyncPvpBackendService _backendService;

        public string OwnerId => _backendService.OwnerId;

        // 현재 로드된 상대 스냅샷의 소유자 표시용 식별자. 닉네임 시스템이 붙기 전까지는 uid를 그대로 노출한다.
        // 서버/로컬 풀 어디서도 상대를 못 찾은 경우(부전승) null.
        public string CurrentOpponentDisplayName { get; private set; }

        List<UnitAsyncBoardData> _fallbackSnapShotpool;

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
                    cellIndex = _unitPlaceDirector.GetCellIndex(UnitGroup.Ally, unit)
                });
            }

            return board;
        }

        public void LoadBoardSnapshot(UnitAsyncBoardData snapshot, UnitGroup targetGroup)
        {
            foreach (var data in snapshot.units)
            {
                var unit = _unitDirector.SpawnUnit(data.unitId, targetGroup, false);
                var cell = _unitPlaceDirector.GetCell(targetGroup, data.cellIndex);
                _unitPlaceDirector.PlaceUnit(unit, cell);
            }
        }


        public void SaveOwnSnapshot(UnitAsyncBoardData snapshot)
        {
            SaveSystem.Save(snapshot, GetSnapshotFileName(snapshot.waveIndex), MySnapshotDirectory, true);
        }

        //사전에 구성된 fallback 스냅샷 중 선택됨
        public bool TryGetRandomFallbackSnapshot(int waveIndex, out UnitAsyncBoardData snapshot)
        {
            if (_fallbackSnapShotpool.Count == 0)
            {
                snapshot = null;
                return false;
            }

            var candidates = _fallbackSnapShotpool.Where(x => x.waveIndex == waveIndex).ToList();
            if (candidates.Count == 0)
            {
                snapshot = null;
                return false;
            }

            snapshot = candidates.GetRandomElement();
            return snapshot != null;
        }

        // 내 보드를 캡처해서 로컬 저장 + 서버 업로드하고, 서버에서 상대 스냅샷을 받아와 적 진영에 로드한다.
        // 서버 상대가 없으면(네트워크 실패, 또는 콜드 스타트) 로컬 풀로 폴백하고, 그마저 없으면 부전승(전투 스킵).
        public IEnumerator PrepareOpponentBoard(int waveIndex)
        {
            CurrentOpponentDisplayName = null;

            var mySnapshot = CaptureOwnBoardSnapshot(waveIndex);
            SaveOwnSnapshot(mySnapshot);
            yield return _backendService.UploadSnapshot(mySnapshot);

            UnitAsyncBoardData opponent = null;
            yield return _backendService.FetchOpponentSnapshot(waveIndex, result => opponent = result);

            var isLocalFallback = false;
            if (opponent == null)
            {
                isLocalFallback = TryGetRandomFallbackSnapshot(waveIndex, out opponent);
            }

            if (opponent == null)
            {
                throw new Exception("No Snapshot");
            }

            // 로컬 폴백은 서버에 상대가 없을 때 내가 과거에 저장해둔 내 스냅샷을 대신 쓰는 것이라 ownerId가 비어있다.
            CurrentOpponentDisplayName = isLocalFallback ? "???" : opponent.ownerId;

            LoadBoardSnapshot(opponent, UnitGroup.Enemy);
        }

        private static string GetSnapshotFileName(int waveIndex)
        {
            return $"wave_{waveIndex}_{Guid.NewGuid():N}.sav";
        }
    }
}