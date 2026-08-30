using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KCoreKit;
using NUnit.Framework.Internal;
using UnityEngine;

namespace DiceBound
{
    public class AsyncPvpDirector : DirectorBase
    {
        private const string MySnapshotDirectory = "AsyncPvp/MySnapshots";

        private UnitDirector _unitDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        private AsyncPvpBackendService _backendService;
        
        public override IEnumerator OnInitialize()
        {
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _backendService = new AsyncPvpBackendService();
            yield return _backendService.EnsureSignedIn();
        }

        public async Task<List<UnitAsyncBoardData>> PrepareBattleDataList(int waveLength)
        {
            var tasks = new List<Task<UnitAsyncBoardData>>();
            for (int i = 0; i < waveLength; i++)
            {
              var task = PickRandomSnapshot(i);
              tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            var result = new List<UnitAsyncBoardData>();
            foreach (var task in tasks)
            {
                result.Add(task.Result);
            }
            return result;
        }

        public UnitAsyncBoardData CaptureOwnBoardSnapshot(int waveIndex)
        {
            var board = new UnitAsyncBoardData
            {
                waveIndex = waveIndex,
                capturedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ownerId = _backendService.OwnerId
            };

            foreach (var unit in _unitDirector.GetAllies())
            {
                board.units.Add(new UnitAsyncData
                {
                    unitId = unit.GetId(),
                    cellIndex = _unitPlaceDirector.GetCellIndex(UnitGroup.Ally, unit),
                });
            }

            return board;
        }
        
        public void SaveOwnSnapshot(UnitAsyncBoardData snapshot)
        {
            SaveSystem.Save(snapshot, GetSnapshotFileName(snapshot.waveIndex), MySnapshotDirectory, true);
        }
        
        public IEnumerator UploadSnapShot(UnitAsyncBoardData snapshot)
        {
            yield return _backendService.UploadSnapshot(snapshot);
        }
        
        public async Task<UnitAsyncBoardData> PickRandomSnapshot(int waveIndex)
        {
            UnitAsyncBoardData result = null;
            var task = _backendService.FetchOpponentSnapshot(waveIndex);
            await task;
            return task.Result;
        }

        private static string GetSnapshotFileName(int waveIndex)
        {
            return $"wave_{waveIndex}_{Guid.NewGuid():N}.sav";
        }
    }
}