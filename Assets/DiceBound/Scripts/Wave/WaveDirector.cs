using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceBound
{
    public class WaveDirector : DirectorBase
    {
        private Dictionary<int,WaveDataTableRow> _waveDictionary;
        private Dictionary<int,List<WaveEnemyPoolDataTableRow>> _waveEnemyPoolDictionary;

        [SerializeField] private TextWidget waveTextWidget;
        [SerializeField] private ButtonWidget playWaveButtonWidget;
        [SerializeField] private ButtonWidget titleButtonWidget;

        private int _currentWave;

        private UnitDirector _unitDirector;
        private bool _isPlaying;
        private BattleDirector _battleDirector;
        [SerializeField]
        private Canvas gameOverCanvas;

        private WalletDirector _walletDirector;

        public override IEnumerator OnInitialize()
        {
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _battleDirector = DirectorFacade.GetDirector<BattleDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            
            playWaveButtonWidget.onClickAction+=OnPlayWaveButtonClick;
            titleButtonWidget.onClickAction+=OnTitleButtonClick;
            
            _waveDictionary = DataTableManager.FindAllRows<WaveDataTableRow>().ToDictionary(x=>x.index);
            _waveEnemyPoolDictionary = DataTableManager.FindAllRows<WaveEnemyPoolDataTableRow>().GroupBy(x=>x.index).ToDictionary(x=>x.Key, x=>x.ToList());
            yield return null;
        }

        private void OnTitleButtonClick()
        {
            LoadingManager.LoadScene("TitleScene");
        }

        private void OnPlayWaveButtonClick()
        {
            if (!_isPlaying)
            {
                PlayWave(_currentWave);
                waveTextWidget.SetText($"{_currentWave+1}");
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
            var wave = _waveDictionary[index];
            if (wave == null)
            {
                ShowGameOver();
            }
            else
            {
                int enemyCount = 0;
                while (enemyCount < wave.numberOfEnemy)
                {
                    _unitDirector.SpawnUnit(PickEnemy(wave.index));
                    enemyCount++;
                }
            
                yield return new WaitForSeconds(0.5f);
                _battleDirector.BeginBattle();
                yield return new WaitUntil(() =>_unitDirector.GetEnemyUnitCount() == 0  || _unitDirector.GetAllyUnitCount() ==_unitDirector.GetDeadAllyUnitCount());
                if (_unitDirector.GetEnemyUnitCount() == 0 )
                {
                    _unitDirector.ClearDeadAllies();
                    _walletDirector.AddGold(wave.waveRewardGold);
                    _battleDirector.EndBattle();
                }
                else if( _unitDirector.GetAllyUnitCount() == _unitDirector.GetDeadAllyUnitCount())
                {
                    _battleDirector.EndBattle();
                    ShowGameOver();
                }
            
                _isPlaying = false;
            }
        }

        private void ShowGameOver()
        {
            gameOverCanvas.gameObject.SetActive(true);
        }


        private string PickEnemy(int waveIndex)
        {
            var enemyPool = _waveEnemyPoolDictionary[waveIndex];
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