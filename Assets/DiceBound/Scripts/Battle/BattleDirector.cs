using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KCoreKit;
using UnityEngine;
using UnityEngine.Pool;

namespace DiceBound
{
    public class BattleDirector : DirectorBase
    {
        private UnitDirector _unitDirector;
        [SerializeField] private Canvas damageCanvas;
        private ObjectPool<DamageWidget> _damageWidgetPool;

        private Queue<BattleContext> _battleContextQueue = new Queue<BattleContext>();
        private bool _isPlaying;
        private SkillDirector _skillDirector;

        public override IEnumerator OnInitialize()
        {
            _damageWidgetPool = new ObjectPool<DamageWidget>(CreateDamageWidget, GetDamageWidget, ReleaseDamageWidget);
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _skillDirector = DirectorFacade.GetDirector<SkillDirector>();
            yield return null;
        }

        private void ReleaseDamageWidget(DamageWidget widget)
        {
            widget.Hide();
        }

        private void GetDamageWidget(DamageWidget widget)
        {
            widget.Show();
        }

        private DamageWidget CreateDamageWidget()
        {
            var widget = PrefabManager.Create<DamageWidget>();
            widget.SetParent(damageCanvas.transform);
            return widget;
        }

        public void BeginBattle()
        {
            _isPlaying = true;
            var units = _unitDirector.GetAllUnit();
            foreach (var unit in units)
            {
                unit.OnBattleBegin();
            }
        }

        public void EndBattle()
        {
            _isPlaying = false;
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

        private IEnumerator ExecuteBattleContext(BattleContext context)
        {
            if (context.target)
            {
                if (!_unitDirector.IsAlive(context.target))
                {
                    yield break;
                }
                
                var effect = _skillDirector.GetSkillEffect(context.skillEffectKey);
                effect.SetPosition(context.self.transform.position);
                
                //yield return new WaitForSeconds(context.castTime);
                yield return effect.Play(context.target).WaitForCompletion();
                
                _skillDirector.Release(context.skillEffectKey,effect);
                if (!_unitDirector.IsAlive(context.target))
                {
                    yield break;
                }

                if (context.damage > 0)
                {
                    context.target.OnDamage(context.damage);
                }

                if (context.healPower > 0)
                {
                    context.target.OnHeal(context.healPower);
                }
             
                //context.target.OnDebuff(context.debuff);
                
            }
        }


        public bool IsPlaying()
        {
            return _isPlaying;
        }

        public void ShowHeal(UnitCore core, int damage)
        {
            var screenPoint =
                RectTransformUtility.WorldToScreenPoint(CameraManager.GetMainCamera(), core.transform.position);
            var damageWidget = _damageWidgetPool.Get();
            damageWidget.SetColor(Color.green);
            damageWidget.Setup(damage);
            damageWidget.rectTransform.anchoredPosition = screenPoint;
            damageWidget.Animate(x => _damageWidgetPool.Release(x));
        }

        public void ShowDamage(UnitCore core, int damage)
        {
            var screenPoint =
                RectTransformUtility.WorldToScreenPoint(CameraManager.GetMainCamera(), core.transform.position);
            var damageWidget = _damageWidgetPool.Get();
            damageWidget.SetColor(Color.red);
            damageWidget.Setup(damage);
            damageWidget.rectTransform.anchoredPosition = screenPoint;
            damageWidget.Animate(x => _damageWidgetPool.Release(x));
        }

        public void EnqueueContext(BattleContext battleContext)
        {
            _battleContextQueue.Enqueue(battleContext);
        }
    }
}