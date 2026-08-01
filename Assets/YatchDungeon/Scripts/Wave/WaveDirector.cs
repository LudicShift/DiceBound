using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;

namespace YatchDungeon
{
    public class WaveDirector : DirectorBase
    {
        private  Dictionary<int,WaveDataTableRow> _waveList;
        private Dictionary<int,List<WaveEnemyPoolDataTableRow>> _wavePoolList;

        [SerializeField] private ButtonWidget playWaveButtonWidget;

        private int _currentWave;

        private UnitDirector _unitDirector;
        private bool _isPlaying;
        private BattleDirector _battleDirector;

        public override IEnumerator OnInitialize()
        {
            _battleDirector = DirectorFacade.GetSubMode<BattleDirector>();
            _unitDirector = DirectorFacade.GetSubMode<UnitDirector>();
            playWaveButtonWidget.AddOnClickAction(OnPlayWaveButtonClick);
            _waveList = DataTableManager.FindAllRows<WaveDataTableRow>().ToDictionary(x=>x.index);
            _wavePoolList = DataTableManager.FindAllRows<WaveEnemyPoolDataTableRow>().GroupBy(x=>x.index).ToDictionary(x=>x.Key, x=>x.ToList());
            yield return null;
        }

        private void OnPlayWaveButtonClick()
        {
            if (!_isPlaying)
            {
                PlayWave(_currentWave);
                _currentWave++;
            }
        }

        public void PlayWave(int index)
        {
            _isPlaying = true;
            StartCoroutine(WaveRoutine(index));
        }

        private IEnumerator WaveRoutine(int index)
        {
            var wave = _waveList[index];
            int enemyCount = 0;
            while (enemyCount < wave.numberOfEnemy)
            {
                _unitDirector.SpawnUnit(PickEnemy(wave.index));
                enemyCount++;
            }
            
            yield return new WaitForSeconds(0.5f);
            _battleDirector.BeginBattle();
            yield return new WaitUntil(() => _unitDirector.GetEnemyUnitCount() == 0 || _unitDirector.GetAllUnitCount() == 0);
            _battleDirector.EndBattle();
            _isPlaying = false;
        }
        
        

        private string PickEnemy(int waveIndex)
        {
            var enemyPool = _wavePoolList[waveIndex];
            float sum = enemyPool.Sum(x => x.encounterWeight);
            float randomValue = Random.Range(0f, sum);
            float accum = 0f;
            for (int i = 0; i < enemyPool.Count; i++)
            {
                accum+=  enemyPool[i].encounterWeight;
                if (accum >= randomValue)
                {
                    return enemyPool[i].enemyId;
                }
            }
            throw new System.NotSupportedException();
        }
    }
}