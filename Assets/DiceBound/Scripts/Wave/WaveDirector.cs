using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ami.BroAudio;
using KCoreKit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceBound
{
    public class WaveDirector : DirectorBase
    {
        private Dictionary<int, WaveDataTableRow> _waveDictionary;
        private Dictionary<int, List<WaveEnemyPoolDataTableRow>> _waveEnemyPoolDictionary;

        [SerializeField] private ImageWidget waveLabelImage;
        [SerializeField] private TextWidget waveLabelText;
        [SerializeField] private TweenAnimationPlayer waveLabelAppearTween;
        [SerializeField] private TweenAnimationPlayer waveLabelDisappearTween;
       
    
     
  
        private UnitDirector _unitDirector;
        private BattlePhaseDirector _battlePhaseDirector;
        private AsyncPvpDirector _asyncPvpDirector;
      
        private SoundDirector _soundDirector;
        private List<UnitAsyncBoardData> _wavePvpCacheData;
        private UnitPlaceDirector _unitPlaceDirector;


        public override IEnumerator OnInitialize()
        {
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _battlePhaseDirector = DirectorFacade.GetDirector<BattlePhaseDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            _asyncPvpDirector = DirectorFacade.GetDirector<AsyncPvpDirector>();
          

            _waveDictionary = DataTableManager.FindAllRows<WaveDataTableRow>().ToDictionary(x => x.index);
            _waveEnemyPoolDictionary = DataTableManager.FindAllRows<WaveEnemyPoolDataTableRow>().GroupBy(x => x.index)
                .ToDictionary(x => x.Key, x => x.ToList());
            StartCoroutine(PrepareWaves());
            yield return null;
        }

        private IEnumerator PrepareWaves()
        {
            yield return DirectorFacade.WaitUntilInitialized();

            var task = _asyncPvpDirector.PrepareBattleDataList(30);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"[AsyncPvp] PrepareBattleDataList 실패: {task.Exception}");
                yield break;
            }

            _wavePvpCacheData = task.Result;
        }


        public WaveDataTableRow GetWave(int waveIndex)
        {
            return _waveDictionary[waveIndex];
        }

        public IEnumerator BeginWaveRoutine(int index)
        {
            var hasWave = _waveDictionary.TryGetValue(index, out var wave);
            if (hasWave)
            {
                
               var ownSnapshot = _asyncPvpDirector.CaptureOwnBoardSnapshot(index);
               _asyncPvpDirector.SaveOwnSnapshot(ownSnapshot);
               StartCoroutine(_asyncPvpDirector.UploadSnapShot(ownSnapshot));
               
               switch (wave.roundType)
                {
                    case RoundType.Creep:
                        yield return PrepareCreepWave(wave);
                        break;
                    case RoundType.Pvp:
                        var snapshot = _wavePvpCacheData[index];
                        if (snapshot == null)
                        { 
                            snapshot = ownSnapshot;
                        }

                        LoadBoardSnapshot(snapshot, UnitGroup.Enemy);
                        break;
                }

                yield return PlayWaveBeginEffect(index);
            }
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

        public IEnumerator EndWaveRoutine()
        {
            waveLabelText.SetText($"Victory");
            waveLabelImage.Show();
            BroAudio.Play(_soundDirector.waveVictorySFX);
            yield return waveLabelAppearTween.Play();
            yield return new WaitForSeconds(0.3f);
            yield return waveLabelDisappearTween.Play();
        }
        
        private IEnumerator PlayWaveBeginEffect(int index)
        {
            waveLabelText.SetText($"Wave {index + 1}");
            waveLabelImage.Show();
            BroAudio.Play(_soundDirector.waveStartSFX);
            yield return waveLabelAppearTween.Play();
            yield return new WaitForSeconds(0.3f);
            yield return waveLabelDisappearTween.Play();
            waveLabelImage.Hide();
        }
        
        private IEnumerator PrepareCreepWave(WaveDataTableRow wave)
        {
            var enemyPool = _waveEnemyPoolDictionary[wave.index];
            foreach (var enemyData in enemyPool)
            {
                for (int i = 0; i < enemyData.number; i++)
                {
                    _unitDirector.SpawnUnit(enemyData.enemyId, UnitGroup.Enemy);
                }
            }

            yield return null;
        }

        
    }
}