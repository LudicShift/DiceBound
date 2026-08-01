using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YatchDungeon
{
    public class WaveDirector : DirectorBase
    {
        private  Dictionary<int,WaveDataTableRow> _waveList;
        private Dictionary<int,List<WaveEnemyPoolDataTableRow>> _wavePoolList;

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
            _walletDirector = DirectorFacade.GetSubMode<WalletDirector>();
            _battleDirector = DirectorFacade.GetSubMode<BattleDirector>();
            _unitDirector = DirectorFacade.GetSubMode<UnitDirector>();
            playWaveButtonWidget.AddOnClickAction(OnPlayWaveButtonClick);
            titleButtonWidget.AddOnClickAction(OnTitleButtonClick);
            _waveList = DataTableManager.FindAllRows<WaveDataTableRow>().ToDictionary(x=>x.index);
            _wavePoolList = DataTableManager.FindAllRows<WaveEnemyPoolDataTableRow>().GroupBy(x=>x.index).ToDictionary(x=>x.Key, x=>x.ToList());
            yield return null;
        }

        private void OnTitleButtonClick()
        {
            SceneManager.LoadScene("TitleScene");
        }

        private void OnPlayWaveButtonClick()
        {
            if (!_isPlaying)
            {
                PlayWave(_currentWave);
                _currentWave++;
                waveTextWidget.SetText($"{_currentWave+1}");
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
                yield return new WaitUntil(() =>_unitDirector.GetEnemyUnitCount() == 0  || _unitDirector.GetAllyUnitCount() == 0);
                if (_unitDirector.GetEnemyUnitCount()== 0 )
                {
                    _walletDirector.AddGold(wave.waveRewardGold);
                    _battleDirector.EndBattle();
                }
                else if( _unitDirector.GetAllyUnitCount() == 0)
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