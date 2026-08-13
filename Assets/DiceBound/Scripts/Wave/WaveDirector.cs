using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using KCoreKit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceBound
{
    public class WaveDirector : DirectorBase
    {
        private Dictionary<int,WaveDataTableRow> _waveDictionary;
        private Dictionary<int,List<WaveEnemyPoolDataTableRow>> _waveEnemyPoolDictionary;

        [SerializeField] private ImageWidget waveLabelImage;
        [SerializeField] private TextWidget waveLabelText;
        [SerializeField] private TweenAnimationPlayer waveLabelAppearTween;
        [SerializeField] private TweenAnimationPlayer waveLabelDisappearTween;
        [SerializeField] private TextWidget waveTextWidget;
        [SerializeField] private ButtonWidget playWaveButtonWidget;
     
        private int _currentWave;

        private UnitDirector _unitDirector;
        private bool _isPlaying;
        private BattleDirector _battleDirector;
        [SerializeField]
        private Canvas gameOverCanvas;
        
        [SerializeField]
        private Canvas gameClearCanvas;

        private WalletDirector _walletDirector;
        private ShopDirector _shopDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        private SoundDirector _soundDirector;

        public override IEnumerator OnInitialize()
        {
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _shopDirector = DirectorFacade.GetDirector<ShopDirector>();

            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _battleDirector = DirectorFacade.GetDirector<BattleDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            playWaveButtonWidget.onClickAction+=OnPlayWaveButtonClick;
        
            
            _waveDictionary = DataTableManager.FindAllRows<WaveDataTableRow>().ToDictionary(x=>x.index);
            _waveEnemyPoolDictionary = DataTableManager.FindAllRows<WaveEnemyPoolDataTableRow>().GroupBy(x=>x.index).ToDictionary(x=>x.Key, x=>x.ToList());
            yield return null;
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
            var hasWave =  _waveDictionary.TryGetValue(index,out var wave);
            if (!hasWave)
            {
                ShowGameClear();
            }
            else
            {
                _unitPlaceDirector.SetEnable(false);
                _shopDirector.SetEnable(false);
                int enemyCount = 0;
                while (enemyCount < wave.numberOfEnemy)
                {
                    _unitDirector.SpawnUnit(PickEnemy(wave.index),wave.enemyStarTier);
                    enemyCount++;
                }
                
                waveLabelText.SetText($"Wave {_currentWave+1}");
                waveLabelImage.Show();
                BroAudio.Play(_soundDirector.waveStartSFX);
                yield return waveLabelAppearTween.Play();
                yield return new WaitForSeconds(0.3f);
                yield return waveLabelDisappearTween.Play();
                waveLabelImage.Hide();
            
                _battleDirector.BeginBattle();
                yield return new WaitUntil(() =>_unitDirector.GetEnemyUnitCount() == 0  || _unitDirector.GetAllyUnitCount() ==_unitDirector.GetDeadAllyUnitCount());
                if (_unitDirector.GetEnemyUnitCount() == 0)
                {
                    _battleDirector.EndBattle();
                    _unitDirector.ClearDeadAllies();
                    _walletDirector.AddGold(wave.waveRewardGold);
                    
                    waveLabelText.SetText($"Victory");
                    waveLabelImage.Show();
                    BroAudio.Play(_soundDirector.waveVictorySFX);
                    yield return waveLabelAppearTween.Play();
                    yield return new WaitForSeconds(0.3f);
                    yield return waveLabelDisappearTween.Play();
                }
                else if( _unitDirector.GetAllyUnitCount() == _unitDirector.GetDeadAllyUnitCount())
                {
                    _battleDirector.EndBattle();
                    ShowGameOver();
                }
            
                _isPlaying = false;
                _unitPlaceDirector.SetEnable(true);
                _shopDirector.SetEnable(true);
            }
        }

        private void ShowGameClear()
        {
            gameClearCanvas.gameObject.SetActive(true);
            BroAudio.Play(_soundDirector.gameClearSFX);
        }

        private void ShowGameOver()
        {
            gameOverCanvas.gameObject.SetActive(true);
            BroAudio.Play(_soundDirector.gameOverSFX);         
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