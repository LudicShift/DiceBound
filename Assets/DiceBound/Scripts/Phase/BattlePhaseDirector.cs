using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using DG.Tweening;
using KCoreKit;
using KCoreKit.Scripts.Common;
using UnityEngine;
using UnityEngine.Pool;

namespace DiceBound
{
    public enum DamageType
    {
        Normal,
        Critical,
        Miss,
        Heal
    }

    public class BattlePhaseDirector : PhaseDirectorBase
    {
        private UnitDirector _unitDirector;
        
        [SerializeField] private TextWidget waveTextWidget;

        [SerializeField] private ButtonWidget timeScaleButton;
        [SerializeField] private Color timeScaleButtonActiveColor;
        [SerializeField] private Color timeScaleButtonInactiveColor;

        [SerializeField] private Canvas damageCanvas;
        [SerializeField] private Canvas battleCanvas;
       
        [SerializeField] private TextWidget winCountText;
        [SerializeField] private TextWidget lossCountText;
       
        [SerializeField] private Canvas gameOverCanvas;
        [SerializeField] private Canvas gameClearCanvas;

        [SerializeField] private TextWidget opponentNameText;

        
        private const int RequiredWins = 10;
        private const int MaxLosses = 5;
        private int _winCount;
        private int _lossCount;

        
        private PrefabPool<DamageWidget> _damageWidgetPool;

        private Queue<BattleContext> _battleContextQueue = new Queue<BattleContext>();
        private bool _isBattle;
        private SkillDirector _skillDirector;
        
        private Dictionary<UnitCore, int> _hitCountMap = new Dictionary<UnitCore, int>();
        private WaveDirector _waveDirector;
        private GameDirector _gameDirector;
        
        private int _currentWave;
        private float _battleTimeScale = 1;
        private bool _fastMode;
        private UnitPlaceDirector _unitPlaceDirector;
        private WalletDirector _walletDirector;
        private SoundDirector _soundDirector;

        public override IEnumerator OnInitialize()
        {
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _damageWidgetPool = new PrefabPool<DamageWidget>(PrefabManager.CachePrefab<DamageWidget>(), damageCanvas.transform, 20);
            _gameDirector = DirectorFacade.GetDirector<GameDirector>();
            _waveDirector = DirectorFacade.GetDirector<WaveDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
             _skillDirector = DirectorFacade.GetDirector<SkillDirector>();
             timeScaleButton.onClickAction += OnClickBattleTimeScaleButton;
             
             opponentNameText.Hide();
             UpdateLivesUI();
             
             yield return null;
        }

        public IEnumerator BeginBattle()
        {
         
            yield return _waveDirector.BeginWaveRoutine(_currentWave);
            var units = _unitDirector.GetAllUnit();
            foreach (var unit in units)
            {
                unit.OnBattleBegin();
            }
            _isBattle = true;
            Time.timeScale = _battleTimeScale;
            _unitPlaceDirector.SetEnable(false);
            waveTextWidget.SetText($"{_currentWave + 1}");

            StartCoroutine(BattleRoutine());
        }

        private IEnumerator BattleRoutine()
        {
            yield return new WaitUntil(()=>_unitDirector.GetEnemyUnitCount() == 0 || _unitDirector.GetAllyUnitCount() == _unitDirector.GetDeadAllyUnitCount());
            EndBattle();
        }


        public void SetBattleTimeScale(float timeScale)
        {
            _battleTimeScale = timeScale;    
        }
        
        public void OnClickBattleTimeScaleButton()
        {
            _fastMode = !_fastMode;
            SetBattleTimeScale(_fastMode ? 2 : 1);
            timeScaleButton.image.color = _fastMode ? timeScaleButtonActiveColor : timeScaleButtonInactiveColor;
            if (_isBattle)
            {
                Time.timeScale = _battleTimeScale;
            }
        }

        public void EndBattle()
        {
            if (_unitDirector.GetEnemyUnitCount() == 0)
            {
                _unitDirector.ClearDeadAllies();
                var waveData = _waveDirector.GetWave(_currentWave);
                _walletDirector.AddGold(Mathf.RoundToInt(waveData.waveRewardGold));
                StartCoroutine(_waveDirector.EndWaveRoutine());
                _winCount++;
                UpdateLivesUI();
                if (_winCount >= RequiredWins)
                {
                    ShowGameClear();
                }
            }
            else if (_unitDirector.GetAllyUnitCount() == _unitDirector.GetDeadAllyUnitCount())
            {
                _lossCount++;
                UpdateLivesUI();
                if (_lossCount >= MaxLosses)
                {
                    ShowGameOver();
                }
            }
            _unitDirector.ClearAllEnemies();
            opponentNameText.Hide();
            _unitPlaceDirector.SetEnable(true);
            _isBattle = false;
            _gameDirector.SetGamePhase(GamePhase.Prepare);
            var units = _unitDirector.GetAllUnit();
            foreach (var unit in units)
            {
                unit.OnBattleEnd();
            }

            _battleContextQueue.Clear();
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
        
        public void Update()
        {
            if (_isBattle)
            {
                if (_battleContextQueue.Count > 0)
                {
                    var context = _battleContextQueue.Dequeue();
                    var selfBattleContext = context.self.battleContext;
                    if ( selfBattleContext == null || selfBattleContext.priority >= context.priority)
                    {
                        context.self.battleContext = context;
                        StartCoroutine(ExecuteBattleContext(context.self.battleContext));
                    }
                }
            }
        }

        // 💡 누락되었던 EnqueueContext 메서드 복구
        public void EnqueueContext(BattleContext battleContext)
        {
            _battleContextQueue.Enqueue(battleContext);
        }

        private IEnumerator ExecuteBattleContext(BattleContext context)
        {
            if (context.target && _unitDirector.IsAlive(context.target))
            {
                if (!(CheckAlive(context.target) &&CheckAlive(context.self)))
                {
                    context.self.battleContext = null;
                    yield break;
                }
                
                context.self.PlayAttackAnimation(context.self.battleContext.animClip);
              
                yield return new WaitForSeconds(context.startUpDelay);
                if (!(CheckAlive(context.target) &&CheckAlive(context.self)))
                {
                    context.self.battleContext = null;
                    yield break;
                }
                context.self.PlayAttackTween();
                var effect = _skillDirector.GetSkillEffect(context.skillEffectKey);
                effect.SetPosition(context.self.transform.position);
                effect.Play(context.target, x => { _skillDirector.Release(context.skillEffectKey, x); });
                // 💡 여기서 hitIndex 계산 및 ShowDamage 수동 호출했던 부분들을 모두 지우고 원상복구합니다.
                if (context.damage > 0)
                {
                    var dodgeRoll = Random.Range(0, 1.0f);
                    if (dodgeRoll < StatUtility.GetDodgeRate(context.target.GetStatAgent(), context.self.GetStatAgent()))
                    {
                        context.target.OnDodge();
                        context.self.battleContext = null;
                        yield break;
                    }

                    var criticalRoll = Random.Range(0, 1.0f);
                    var damage = context.damage;
                    if (criticalRoll < StatUtility.GetCritRate(context.self.GetStatAgent()))
                    {
                        damage *= StatUtility.GetCritMult(context.self.GetStatAgent());
                        context.target.OnDamage(damage, true);
                    }
                    else
                    {
                        context.target.OnDamage(damage, false);
                    }
                }

                if (context.healPower > 0)
                {
                    context.target.OnHeal(context.healPower);
                }

                context.self.battleContext = null;
            }
        }

        private bool CheckAlive(UnitCore core)
        {
            return core && core.gameObject.activeInHierarchy && _unitDirector.IsAlive(core);
        }

        // 💡 UnitCore 등에서 이 메서드들이 호출될 때마다 hitIndex를 여기서 직접 계산하도록 옮겼습니다.
        public void ShowHeal(UnitCore core, float healAmount)
        {
            int hitIndex = GetAndIncrementHitCount(core);
            SpawnWidget(core, healAmount, DamageType.Heal, hitIndex);
            StartCoroutine(ResetHitCountAfterDelay(core, 0.5f));
        }

        public void ShowDamage(UnitCore core, float damageAmount, bool isCritical)
        {
            int hitIndex = GetAndIncrementHitCount(core);
            DamageType type = isCritical ? DamageType.Critical : DamageType.Normal;
            SpawnWidget(core, damageAmount, type, hitIndex);
            StartCoroutine(ResetHitCountAfterDelay(core, 0.5f));
        }

        public void ShowMiss(UnitCore core)
        {
            int hitIndex = GetAndIncrementHitCount(core);
            SpawnWidget(core, 0f, DamageType.Miss, hitIndex);
            StartCoroutine(ResetHitCountAfterDelay(core, 0.5f));
        }

        private int GetAndIncrementHitCount(UnitCore target)
        {
            if (!_hitCountMap.ContainsKey(target))
                _hitCountMap[target] = 0;

            int currentCount = _hitCountMap[target];
            _hitCountMap[target]++;
            return currentCount;
        }

        private IEnumerator ResetHitCountAfterDelay(UnitCore target, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_hitCountMap.ContainsKey(target))
            {
                _hitCountMap[target] = Mathf.Max(0, _hitCountMap[target] - 1);
            }
        }

        private void SpawnWidget(UnitCore core, float amount, DamageType type, int hitIndex)
        {
            var damageWidget = _damageWidgetPool.Get();

            float offsetX = Random.Range(-5f, 5f);
            float offsetY = 120f + (hitIndex * 30f);

            damageWidget.SetPositionFromWorldPoint(CameraManager.GetMainCamera(), core.transform.position, new Vector2(offsetX, offsetY));

            damageWidget.Setup(Mathf.RoundToInt(amount), type);
            damageWidget.Play(hitIndex, x => _damageWidgetPool.Release(x));
        }

        public override void OnEnter()
        {
            battleCanvas.Open();
          
            StartCoroutine(BeginBattle());
        }

        public override void OnExit()
        {
            battleCanvas.Close();
        }
        
        private void UpdateLivesUI()
        {
            winCountText.SetText($"{_winCount}/{RequiredWins}");
            lossCountText.SetText($"{MaxLosses - _lossCount}");
        }
    }
}