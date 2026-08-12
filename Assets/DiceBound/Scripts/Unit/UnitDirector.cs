using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using KCoreKit.Scripts.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceBound
{
    public class UnitDirector : DirectorBase
    {
        private Dictionary<string, UnitDataTableRow> _unitDataDictionary;
        private Vector3 _dragOffset;
        private Camera _camera;
        [SerializeField] private Canvas hpCanvas;
        [SerializeField] private TextWidget allyCountWidget;


        private PrefabPool<UnitCore> _allyPrefabPool;
        private PrefabPool<UnitCore> _enemyPrefabPool;
        private PrefabPool<GaugeWidget> _hpGaugePrefabPool;
        private PrefabPool<TierLabelWidget> _tierLabelPrefabPool;
        private List<UnitCore> _units = new List<UnitCore>();
        private List<UnitCore> _allies = new List<UnitCore>();
        private List<UnitCore> _enemies = new List<UnitCore>();
        private List<UnitCore> _deadAllies = new List<UnitCore>();

        private SkillDirector _skillDirector;
        private BattleDirector _battleDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        private TooltipDirector _tooltipDirector;
        private WalletDirector _walletDirector;

        public Action<UnitCore> onSpawnAlly;

        private int _maxAllyNumber = 15;

        [SerializeField] private UnitTrashCan trashCan;

        public void UpdateAllyCountText()
        {
            allyCountWidget.SetText($"{_allies.Count}/{_maxAllyNumber}");
        }
        public override IEnumerator OnInitialize()
        {
            UpdateAllyCountText();
            _hpGaugePrefabPool = new PrefabPool<GaugeWidget>(PrefabManager.CachePrefab<GaugeWidget>(), hpCanvas.transform, 100);
            _tierLabelPrefabPool = new PrefabPool<TierLabelWidget>(PrefabManager.CachePrefab<TierLabelWidget>(), hpCanvas.transform,100);
            
            _allyPrefabPool =
                new PrefabPool<UnitCore>(PrefabManager.CachePrefab<UnitCore>("PF_Ally"), World.GetTransform(), 50);
            _enemyPrefabPool = new PrefabPool<UnitCore>(PrefabManager.CachePrefab<UnitCore>("PF_Enemy"),
                World.GetTransform(), 50);
            
            _allyPrefabPool.onGetAction += OnGetUnit;
            _enemyPrefabPool.onGetAction += OnGetUnit;
            _allyPrefabPool.onReleaseAction += OnReleaseUnit;
            _enemyPrefabPool.onReleaseAction += OnReleaseUnit;
            trashCan.onRemoveUnitAction += SellUnit;

            _tooltipDirector = DirectorFacade.GetDirector<TooltipDirector>();
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _skillDirector = DirectorFacade.GetDirector<SkillDirector>();
            _battleDirector = DirectorFacade.GetDirector<BattleDirector>();
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();

            _camera = CameraManager.GetMainCamera();
            _unitDataDictionary = DataTableManager.FindAllRows<UnitDataTableRow>().ToDictionary(x => x.id);
            yield return null;
        }

        private void SellUnit(UnitCore unit)
        {
            RemoveAllyUnit(unit);
            _walletDirector.PlayGainGoldEffect(trashCan.transform.position,50);
        }


        private void OnReleaseUnit(UnitCore instance)
        {
            instance.onDeadAction -= OnUnitDead;
            instance.onHitAction -= OnUnitHit;
            instance.onHealAction -= OnUnitHeal;
            var hp = instance.GetHpGauge();
            var tierLabel = instance.GetTierLabel();
            instance.ReleaseHpGauge(hp);
            _hpGaugePrefabPool.Release(hp);
            _tierLabelPrefabPool.Release(tierLabel);
        }

        private void OnGetUnit(UnitCore instance)
        {
            instance.onDeadAction += OnUnitDead;
            instance.onHitAction += OnUnitHit;
            instance.onHealAction += OnUnitHeal;
        }

        public bool IsAllyFull()
        {
            return _maxAllyNumber <= _allies.Count;
        }

        public void SpawnUnit(string unitId)
        {
            var data = _unitDataDictionary[unitId];
            UnitCore instance;
            if (data.group == UnitGroup.Ally)
            {
                instance = _allyPrefabPool.Get();
            }
            else
            {
                instance = _enemyPrefabPool.Get();
            }

            instance.Setup(data);
            instance.BindHpGauge(_hpGaugePrefabPool.Get());
            instance.BindTierLabel(_tierLabelPrefabPool.Get());
            instance.Animate("Idle");

            instance.BindSkill(_skillDirector.GetSkill(data.skillBasicKey));
            instance.BindSkill(_skillDirector.GetSkill(data.skillActiveKey));
            instance.BindSkill(_skillDirector.GetSkill(data.skillPassiveKey));
            _tooltipDirector.BindTooltip("Unit",instance.tooltipProvider);
            
            _unitPlaceDirector.PlaceUnit(instance);
            

            _units.Add(instance);
            switch (data.group)
            {
                case UnitGroup.Ally:
                    _allies.Add(instance);
                    instance.PlayAppear(()=>onSpawnAlly.Invoke(instance));
                    instance.FlipSprite(false);
                    UpdateAllyCountText();
                    break;
                case UnitGroup.Enemy:
                    _enemies.Add(instance);
                    instance.PlayAppear();
                    instance.FlipSprite(true);
                    break;
            }
        }

        private void OnUnitHeal(UnitCore core, int damage)
        {
            _battleDirector.ShowHeal(core, damage);
        }

        private void OnUnitHit(UnitCore core, int damage)
        {
            _battleDirector.ShowDamage(core, damage);
        }

        private void OnUnitDead(UnitCore unit)
        {
            switch (unit.group)
            {
                case UnitGroup.Ally:
                    if (!_deadAllies.Contains(unit))
                    {
                        _units.Remove(unit);
                        _deadAllies.Add(unit);
                    }
                    break;
                case UnitGroup.Enemy:
                    if (_units.Contains(unit))
                    {
                        _enemies.Remove(unit);
                        _units.Remove(unit);
                        _unitPlaceDirector.RemoveUnit(unit);
                        _enemyPrefabPool.Release(unit);
                    }

                    break;
            }
        }
        
        public void RemoveAllyUnit(UnitCore unit)
        {
            _allies.Remove(unit);
            _units.Remove(unit);
            _unitPlaceDirector.RemoveUnit(unit);
            _allyPrefabPool.Release(unit);
            UpdateAllyCountText();
        }

        public int GetEnemyUnitCount()
        {
            return _enemies.Count;
        }
        public List<UnitCore> GetAllies()
        {
            return _allies;
        }
        
        public List<UnitCore> GetAllUnit()
        {
            return _units;
        }

        public List<UnitCore> GetTarget(UnitCore self, SkillTargetOption attackTarget, int count)
        {
            switch (attackTarget)
            {
                case SkillTargetOption.Ally:
                    return self.group == UnitGroup.Ally ? GetRandomAllies(count) : GetRandomEnemies(count);
                case SkillTargetOption.Enemy:
                    return self.group == UnitGroup.Ally ? GetRandomEnemies(count) : GetRandomAllies(count);
                case SkillTargetOption.Self:
                    return new List<UnitCore> { self };
            }

            return null;
        }

        private List<UnitCore> GetRandomEnemies(int count)
        {
            return _enemies.GetRandomElements(count);
        }

        private List<UnitCore> GetRandomAllies(int count)
        {
            return _allies.Where(x => !x.IsDead()).ToList().GetRandomElements(count);
        }

        public int GetAllyUnitCount()
        {
            return _allies.Count;
        }

        public bool IsAlive(UnitCore target)
        {
            return !target.IsDead();
        }

        public int GetDeadAllyUnitCount()
        {
            return _deadAllies.Count;
        }

        public void ReviveDeadAllies()
        {
            foreach (var unit in _deadAllies)
            {
                unit.Revive();
            }

            _deadAllies.Clear();
        }
    }
}