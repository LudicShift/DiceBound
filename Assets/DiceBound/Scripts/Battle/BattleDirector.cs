using System.Collections;
using System.Collections.Generic;
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

    public class BattleDirector : DirectorBase
    {
        private UnitDirector _unitDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        [SerializeField] private Canvas damageCanvas;
        private PrefabPool<DamageWidget> _damageWidgetPool;

        private Queue<BattleContext> _battleContextQueue = new Queue<BattleContext>();
        private bool _isPlaying;
        private SkillDirector _skillDirector;
        private ShopDirector _shopDirector;

        private Dictionary<UnitCore, int> _hitCountMap = new Dictionary<UnitCore, int>();

        public override IEnumerator OnInitialize()
        {
            _damageWidgetPool = new PrefabPool<DamageWidget>(PrefabManager.CachePrefab<DamageWidget>(), damageCanvas.transform, 20);
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _skillDirector = DirectorFacade.GetDirector<SkillDirector>();
            _shopDirector = DirectorFacade.GetDirector<ShopDirector>();
            yield return null;
        }

        public void BeginBattle()
        {
            _isPlaying = true;
            _unitPlaceDirector.SetEnable(false);
            _shopDirector.SetEnable(false);
            var units = _unitDirector.GetAllUnit();
            foreach (var unit in units)
            {
                unit.OnBattleBegin();
            }
        }

        public void EndBattle()
        {
            _isPlaying = false;
            _unitPlaceDirector.SetEnable(true);
            _shopDirector.SetEnable(true);
            var units = _unitDirector.GetAllUnit();
            foreach (var unit in units)
            {
                unit.OnBattleEnd();
            }

            _battleContextQueue.Clear();
        }

        public void Update()
        {
            if (_isPlaying)
            {
                if (_battleContextQueue.Count > 0)
                {
                    StartCoroutine(ExecuteBattleContext(_battleContextQueue.Dequeue()));
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
                if (!context.self || !_unitDirector.IsAlive(context.self)) yield break;

                var effect = _skillDirector.GetSkillEffect(context.skillEffectKey);
                effect.SetPosition(context.self.transform.position);

                effect.Play(context.target, x => { _skillDirector.Release(context.skillEffectKey, x); });

                if (!context.self || !_unitDirector.IsAlive(context.self)) yield break;

                // 💡 여기서 hitIndex 계산 및 ShowDamage 수동 호출했던 부분들을 모두 지우고 원상복구합니다.
                if (context.damage > 0)
                {
                    var dodgeRoll = Random.Range(0, 1.0f);
                    if (dodgeRoll < StatUtility.GetDodgeRate(context.target.GetStatAgent(), context.self.GetStatAgent()))
                    {
                        context.target.OnDodge();
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
            }
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
    }
}