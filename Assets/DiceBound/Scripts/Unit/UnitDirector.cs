using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
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
        [SerializeField] private Canvas unitCanvas;
        [SerializeField] private TextWidget allyCountWidget;


        private PrefabPool<UnitCore> _allyPrefabPool;
        private PrefabPool<UnitCore> _enemyPrefabPool;
        private PrefabPool<UnitInfoWidget> _unitInfoPrefabPool;
   
        private List<UnitCore> _units = new List<UnitCore>();
        private List<UnitCore> _allies = new List<UnitCore>();
        private List<UnitCore> _enemies = new List<UnitCore>();
        private List<UnitCore> _deadAllies = new List<UnitCore>();

        private SkillDirector _skillDirector;
        private BattleDirector _battleDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        private TooltipDirector _tooltipDirector;
        private WalletDirector _walletDirector;
        private SoundDirector _soundDirector;

        public Action<UnitCore> onSpawnAlly;

        private int _maxAllyNumber;
        private SkillTreeManager _skillTreeManager;

        private static readonly string[] AllyStatKeys = { "str", "spd", "def", "mag", "con", "dex", "mdf", "hp" };

        [SerializeField] private UnitTrashCan trashCan;

        public void Update()
        {
            Debug.Log($"죽은 아군 수 : {_deadAllies.Count}");
        }
        public void UpdateAllyCountText()
        {
            allyCountWidget.SetText($"{_allies.Count}/{_maxAllyNumber}");
        }
        public override IEnumerator OnInitialize()
        {
            _skillTreeManager = SkillTreeManager.GetInstance();
            _maxAllyNumber = 15 + (int)_skillTreeManager.GetModifierTotal("AllyCapIncrease");
            UpdateAllyCountText();
            _unitInfoPrefabPool = new PrefabPool<UnitInfoWidget>(PrefabManager.CachePrefab<UnitInfoWidget>(), unitCanvas.transform, 100);
          
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
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
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
            BroAudio.Play(_soundDirector.sellUnitSFX);
        }


        private void OnReleaseUnit(UnitCore instance)
        {
            instance.OnRelease();
            var info = instance.GetUnitInfoWidget();
            _unitInfoPrefabPool.Release(info);
        }

        private void OnUnitDodge(UnitCore unit)
        {
            _battleDirector.ShowMiss(unit);
        }

        private void OnGetUnit(UnitCore instance)
        {
            instance.onDeadAction += OnUnitDead;
            instance.onHitAction += OnUnitHit;
            instance.onHealAction += OnUnitHeal;
            instance.onDodgeAction += OnUnitDodge;
        }

        public bool IsAllyFull()
        {
            return _maxAllyNumber <= _allies.Count;
        }

        public void SpawnUnit(string unitId,int tier = 0)
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
            instance.BindInfoWidget(_unitInfoPrefabPool.Get());
           _tooltipDirector.BindTooltip("Unit",instance.tooltipProvider);
            instance.Setup(data,tier);
            if (data.group == UnitGroup.Ally)
            {
                ApplyAllyStatModifiers(instance);
            }
            
            instance.Animate("Idle");
            instance.BindSkill(_skillDirector.GetSkill(data.skillBasicKey));
            instance.BindSkill(_skillDirector.GetSkill(data.skillActiveKey));
            instance.BindSkill(_skillDirector.GetSkill(data.skillPassiveKey));
            
           for (int i = 0; i < tier; i++)
           {
               instance.Upgrade();
           }
            _unitPlaceDirector.PlaceUnit(instance);
            _units.Add(instance);
            
            switch (data.group)
            {
                case UnitGroup.Ally:
                    _allies.Add(instance);
                    instance.PlayAppear(()=>onSpawnAlly.Invoke(instance));
                    instance.FlipSprite(false);
                    UpdateAllyCountText();

                    BroAudio.Play(_soundDirector.spawnAllySFX);
                    break;
                case UnitGroup.Enemy:
                    _enemies.Add(instance);
                    instance.PlayAppear();
                    instance.FlipSprite(true);
                    
                    
                    BroAudio.Play(_soundDirector.spawnEnemySFX);
                    break;
            }
        }

        private void ApplyAllyStatModifiers(UnitCore instance)
        {
            var statPercent = _skillTreeManager.GetModifierTotal("AllyAllStatsPercent") / 100f;
            if (statPercent == 0f)
            {
                return;
            }

            var statAgent = instance.GetStatAgent();
            foreach (var statKey in AllyStatKeys)
            {
                statAgent.GetStat(statKey).AddModifier(new StatModifier(statPercent, StatModifyType.PercentAdd, 0, this));
            }
        }

        private void OnUnitHeal(UnitCore core, int damage)
        {
            _battleDirector.ShowHeal(core, damage);
        }

        private void OnUnitHit(UnitCore core, int damage,bool isCritical)
        {
            _battleDirector.ShowDamage(core, damage,isCritical);
        }

        private void OnUnitDead(UnitCore unit)
        {
            switch (unit.group)
            {
                case UnitGroup.Ally:
                    if (!_deadAllies.Contains(unit))
                    {
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

        public List<UnitCore> GetTarget(UnitCore self, SkillTargetGroup targetGroup,SkillTargetOption targetOption, int count)
        {
            
            if (targetOption == SkillTargetOption.General)
            {
                
            }
            
            switch (targetGroup)
            {
                case SkillTargetGroup.Ally:
                    return self.group == UnitGroup.Ally ? GetTargetAllies(targetOption,count) : GetTargetEnemies(targetOption,count);
                case SkillTargetGroup.Enemy:
                    return self.group == UnitGroup.Ally ? GetTargetEnemies(targetOption,count) : GetTargetAllies(targetOption,count);
                case SkillTargetGroup.Self:
                    return new List<UnitCore> { self };
            }

            return null;
        }

        private List<UnitCore> GetTargetEnemies(SkillTargetOption targetOption,int count)
        {
            switch (targetOption)
            {
                case  SkillTargetOption.General:
                    return _unitPlaceDirector.GetGeneralTargets(UnitGroup.Enemy,count);
                case SkillTargetOption.Weak:
                    return _enemies.Where(x => !x.IsDead()).OrderBy(x=>x.GetHp()).Take(count).ToList();
                case SkillTargetOption.Strong:
                    return _enemies.Where(x => !x.IsDead()).OrderByDescending(x=>x.GetHp()).Take(count).ToList();
                case SkillTargetOption.LessHp:
                    return _enemies.Where(x => !x.IsDead()).OrderBy(x=>x.GetHpRate()).Take(count).ToList();
                case SkillTargetOption.Random:
                    return _enemies.Where(x => !x.IsDead()).ToList().GetRandomElements(count);
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetOption), targetOption, null);
            }
        }

        private List<UnitCore> GetTargetAllies(SkillTargetOption targetOption,int count)
        {
            switch (targetOption)
            {
                case  SkillTargetOption.General:
                    return _unitPlaceDirector.GetGeneralTargets(UnitGroup.Ally,count);
                case SkillTargetOption.Weak:
                    return _allies.Where(x => !x.IsDead()).OrderBy(x=>x.GetHp()).Take(count).ToList();
                case SkillTargetOption.Strong:
                    return _allies.Where(x => !x.IsDead()).OrderByDescending(x=>x.GetHp()).Take(count).ToList();
                case SkillTargetOption.LessHp:
                    return _allies.Where(x => !x.IsDead()).OrderBy(x=>x.GetHpRate()).Take(count).ToList();
                case SkillTargetOption.Random:
                    return _allies.Where(x => !x.IsDead()).ToList().GetRandomElements(count);
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetOption), targetOption, null);
            }
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

        public void ClearDeadAllies()
        {
            _deadAllies.Clear();
        }
    }
}