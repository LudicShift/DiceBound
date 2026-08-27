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
        private Dictionary<int, WaveDataTableRow> _waveDictionary;
        private Dictionary<int, List<WaveEnemyPoolDataTableRow>> _waveEnemyPoolDictionary;

        [SerializeField] private ImageWidget waveLabelImage;
        [SerializeField] private TextWidget waveLabelText;
        [SerializeField] private TweenAnimationPlayer waveLabelAppearTween;
        [SerializeField] private TweenAnimationPlayer waveLabelDisappearTween;
        [SerializeField] private TextWidget waveTextWidget;
 
        [SerializeField] private ButtonWidget fastButtonWidget;

        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;

        [SerializeField] private TextWidget opponentNameText;

        [SerializeField] private TextWidget winCountText;
        [SerializeField] private TextWidget lossCountText;
        private const int RequiredWins = 10;
        private const int MaxLosses = 5;
        private int _winCount;
        private int _lossCount;

        private int _currentWave;
        private float _waveTimeScale = 1f;

        private UnitDirector _unitDirector;
        private bool _isPlaying;
        private BattlePhaseDirectorBase _battlePhaseDirector;
        private AsyncPvpDirector _asyncPvpDirector;
        [SerializeField] private Canvas gameOverCanvas;

        [SerializeField] private Canvas gameClearCanvas;

        private WalletDirector _walletDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        private SoundDirector _soundDirector;
        private bool _fastMode;

        public override IEnumerator OnInitialize()
        {
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
        
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _battlePhaseDirector = DirectorFacade.GetDirector<BattlePhaseDirectorBase>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            _asyncPvpDirector = DirectorFacade.GetDirector<AsyncPvpDirector>();
            fastButtonWidget.onClickAction += OnClickFastButton;


            _waveDictionary = DataTableManager.FindAllRows<WaveDataTableRow>().ToDictionary(x => x.index);
            _waveEnemyPoolDictionary = DataTableManager.FindAllRows<WaveEnemyPoolDataTableRow>().GroupBy(x => x.index)
                .ToDictionary(x => x.Key, x => x.ToList());

            opponentNameText.Hide();
            UpdateLivesUI();
            yield return null;
        }

        private void UpdateLivesUI()
        {
            winCountText.SetText($"{_winCount}/{RequiredWins}");
            lossCountText.SetText($"{_lossCount}/{MaxLosses}");
        }


        public void PlayWave()
        {
            if (!_isPlaying)
            {
                _isPlaying = true;
                Time.timeScale = _waveTimeScale;
                StartCoroutine(WaveRoutine(_currentWave));
                waveTextWidget.SetText($"{_currentWave + 1}");
                _currentWave++;
            }
        }

        public void OnClickFastButton()
        {
            _fastMode = !_fastMode;
            SetWaveTimeScale(_fastMode ? 2 : 1);
            fastButtonWidget.image.color = _fastMode ? activeColor : inactiveColor;
            if (_isPlaying)
            {
                Time.timeScale = _waveTimeScale;
            }
        }

        public void SetWaveTimeScale(float timeScale)
        {
            _waveTimeScale = timeScale;
        }
        

        private IEnumerator WaveRoutine(int index)
        {
            var hasWave = _waveDictionary.TryGetValue(index, out var wave);
            if (hasWave)
            {
                _unitPlaceDirector.SetEnable(false);
                
                int enemyCount = 0;
                switch (wave.roundType)
                {
                    case RoundType.Creep:
                        var enemyPool = _waveEnemyPoolDictionary[wave.index];
                        foreach (var enemyData in enemyPool)
                        {
                            for (int i = 0; i < enemyData.number; i++)
                            {
                                _unitDirector.SpawnUnit(enemyData.enemyId, UnitGroup.Enemy);
                            }
                        }
                        break;
                    case RoundType.Pvp:
                        yield return StartCoroutine(_asyncPvpDirector.PrepareOpponentBoard(wave.index));
                        var opponentName = _asyncPvpDirector.CurrentOpponentDisplayName;
                        if (!string.IsNullOrEmpty(opponentName))
                        {
                            opponentNameText.SetText(opponentName);
                            opponentNameText.Show();
                        }
                        break;
                }


                enemyCount++;


                waveLabelText.SetText($"Wave {_currentWave + 1}");
                waveLabelImage.Show();
                BroAudio.Play(_soundDirector.waveStartSFX);
                yield return waveLabelAppearTween.Play();
                yield return new WaitForSeconds(0.3f);
                yield return waveLabelDisappearTween.Play();
                waveLabelImage.Hide();

                _battlePhaseDirector.BeginBattle();
                yield return new WaitUntil(() =>
                    _unitDirector.GetEnemyUnitCount() == 0 ||
                    _unitDirector.GetAllyUnitCount() == _unitDirector.GetDeadAllyUnitCount());
                if (_unitDirector.GetEnemyUnitCount() == 0)
                {
                    _battlePhaseDirector.EndBattle();
                    _unitDirector.ClearDeadAllies();
                    _walletDirector.AddGold(Mathf.RoundToInt(wave.waveRewardGold));
                    waveLabelText.SetText($"Victory");
                    waveLabelImage.Show();
                    BroAudio.Play(_soundDirector.waveVictorySFX);
                    yield return waveLabelAppearTween.Play();
                    yield return new WaitForSeconds(0.3f);
                    yield return waveLabelDisappearTween.Play();

                    _winCount++;
                    UpdateLivesUI();
                    if (_winCount >= RequiredWins)
                    {
                        ShowGameClear();
                    }
                }
                else if (_unitDirector.GetAllyUnitCount() == _unitDirector.GetDeadAllyUnitCount())
                {
                    _battlePhaseDirector.EndBattle();

                    _lossCount++;
                    UpdateLivesUI();
                    if (_lossCount >= MaxLosses)
                    {
                        ShowGameOver();
                    }
                }

                if (wave.roundType == RoundType.Pvp)
                {
                    _unitDirector.ClearAllEnemies();
                    opponentNameText.Hide();
                }

                _isPlaying = false;
                Time.timeScale = 1;
                _unitPlaceDirector.SetEnable(true);
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
    }
}