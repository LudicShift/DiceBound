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
        [SerializeField] private ButtonWidget playWaveButtonWidget;
        [SerializeField] private ButtonWidget fastButtonWidget;

        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;

        private int _currentWave;
        private float _waveTimeScale = 1f;

        private UnitDirector _unitDirector;
        private bool _isPlaying;
        private BattleDirector _battleDirector;
        private AsyncPvpDirector _asyncPvpDirector;
        [SerializeField] private Canvas gameOverCanvas;

        [SerializeField] private Canvas gameClearCanvas;

        private WalletDirector _walletDirector;
        private ShopDirector _shopDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        private SoundDirector _soundDirector;
        private MasteryManager _masteryManager;
        private bool _fastMode;

        public override IEnumerator OnInitialize()
        {
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _shopDirector = DirectorFacade.GetDirector<ShopDirector>();

            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _battleDirector = DirectorFacade.GetDirector<BattleDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            _asyncPvpDirector = DirectorFacade.GetDirector<AsyncPvpDirector>();
            _masteryManager = MasteryManager.GetInstance();
            playWaveButtonWidget.onClickAction += OnPlayWaveButtonClick;
            fastButtonWidget.onClickAction += OnClickFastButton;


            _waveDictionary = DataTableManager.FindAllRows<WaveDataTableRow>().ToDictionary(x => x.index);
            _waveEnemyPoolDictionary = DataTableManager.FindAllRows<WaveEnemyPoolDataTableRow>().GroupBy(x => x.index)
                .ToDictionary(x => x.Key, x => x.ToList());
            yield return null;
        }


        private void OnPlayWaveButtonClick()
        {
            if (!_isPlaying)
            {
                Time.timeScale = _waveTimeScale;
                PlayWave(_currentWave);
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

        public void PlayWave(int index)
        {
            _isPlaying = true;
            playWaveButtonWidget.image.color = activeColor;
            StartCoroutine(WaveRoutine(index));
        }

        private IEnumerator WaveRoutine(int index)
        {
            var hasWave = _waveDictionary.TryGetValue(index, out var wave);
            if (hasWave)
            {
                _unitPlaceDirector.SetEnable(false);
                _shopDirector.SetEnable(false);
                
                int enemyCount = 0;
                switch (wave.roundType)
                {
                    case RoundType.Creep:
                        var enemyPool = _waveEnemyPoolDictionary[wave.index];
                        foreach (var enemyData in enemyPool)
                        {
                            for (int i = 0; i < enemyData.number; i++)
                            {
                                _unitDirector.SpawnUnit(enemyData.enemyId, UnitGroup.Enemy,true, enemyData.tier);
                            }
                        }
                        break;
                    case RoundType.Pvp:
                        _asyncPvpDirector.PrepareOpponentBoard(wave.index);
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

                _battleDirector.BeginBattle();
                yield return new WaitUntil(() =>
                    _unitDirector.GetEnemyUnitCount() == 0 ||
                    _unitDirector.GetAllyUnitCount() == _unitDirector.GetDeadAllyUnitCount());
                if (_unitDirector.GetEnemyUnitCount() == 0)
                {
                    _battleDirector.EndBattle();
                    _unitDirector.ClearDeadAllies();
                    var goldRatePercent = _masteryManager.GetModifierTotal("WaveGoldRewardPercent");
                    _walletDirector.AddGold(Mathf.RoundToInt(wave.waveRewardGold * (1f + goldRatePercent / 100f)));
                    _masteryManager.AddDiamond(wave.waveRewardDiamond);

                    waveLabelText.SetText($"Victory");
                    waveLabelImage.Show();
                    BroAudio.Play(_soundDirector.waveVictorySFX);
                    yield return waveLabelAppearTween.Play();
                    yield return new WaitForSeconds(0.3f);
                    yield return waveLabelDisappearTween.Play();
                    if (_currentWave == _waveDictionary.Count)
                    {
                        ShowGameClear();
                    }
                }
                else if (_unitDirector.GetAllyUnitCount() == _unitDirector.GetDeadAllyUnitCount())
                {
                    _battleDirector.EndBattle();
                    ShowGameOver();
                }

                if (wave.roundType == RoundType.Pvp)
                {
                    _unitDirector.ClearAllEnemies();
                }

                _isPlaying = false;
                Time.timeScale = 1;
                playWaveButtonWidget.image.color = inactiveColor;
                _unitPlaceDirector.SetEnable(true);
                _shopDirector.SetEnable(true);
                _shopDirector.OnRoundBegin();
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