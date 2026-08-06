using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KCoreKit;
using KCoreKit.Scripts.Common;
using UnityEngine;
using UnityEngine.Pool;

namespace DiceBound
{
    public class BattleDirector : DirectorBase
    {
        private UnitDirector _unitDirector;
        [SerializeField] private Canvas damageCanvas;
        private PrefabPool<DamageWidget> _damageWidgetPool;

        private Queue<BattleContext> _battleContextQueue = new Queue<BattleContext>();
        private bool _isPlaying;
        private SkillDirector _skillDirector;

        public override IEnumerator OnInitialize()
        {
            _damageWidgetPool = new PrefabPool<DamageWidget>(PrefabManager.CachePrefab<DamageWidget>(),damageCanvas.transform,20);
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _skillDirector = DirectorFacade.GetDirector<SkillDirector>();
            yield return null;
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
                if (!context.self || !_unitDirector.IsAlive(context.self))
                {
                    yield break;
                }
                
                var effect = _skillDirector.GetSkillEffect(context.skillEffectKey);
                effect.SetPosition(context.self.transform.position);
                
                Debug.DrawLine(context.self.transform.position, context.target.transform.position, Color.red);
                //yield return new WaitForSeconds(context.castTime);
                effect.Play(context.target, x =>
                {
                    _skillDirector.Release(context.skillEffectKey, x);
                });
                
                if (!context.self || !_unitDirector.IsAlive(context.self))
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
            var damageWidget = _damageWidgetPool.Get();
            damageWidget.SetColor(Color.green);
            damageWidget.Setup(damage);
            PositionOverUnit(damageWidget.rectTransform, core.transform.position);
            damageWidget.Play(x => _damageWidgetPool.Release(x));
        }

        public void ShowDamage(UnitCore core, int damage)
        {
            var damageWidget = _damageWidgetPool.Get();
            damageWidget.SetColor(core.group == UnitGroup.Ally ? Color.red : Color.orange);

            damageWidget.Setup(damage);
            PositionOverUnit(damageWidget.rectTransform, core.transform.position);
            damageWidget.Play(x => _damageWidgetPool.Release(x));
        }

        private void PositionOverUnit(RectTransform target, Vector3 worldPosition)
        {
            var camera = CameraManager.GetMainCamera();
            var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)damageCanvas.transform, screenPoint, camera, out var localPoint);
            target.anchoredPosition = localPoint;
        }

        public void EnqueueContext(BattleContext battleContext)
        {
            _battleContextQueue.Enqueue(battleContext);
        }
    }
}