using System;
using DG.Tweening;
using JetBrains.Annotations;
using KCoreKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YatchDungeon
{
    public class UnitCore : MonoBehaviour
    {
        private UnitDataTableRow _data;
        [SerializeField] private float moveDuration = 0.3f;

        
        public float str;
        public float dex;
        public float mag;
        public float spd;
        public float mdf;
        public float def;
        public float con;
        
        
        public float maxhp;
        public float hp;
        
        public Action<UnitCore> onDeadAction;
        private AbilityAgent _abilityAgent;
        public UnitGroup group;
        private SpriteRenderer _spriteRenderer;
        private GaugeWidget _hpGague;
        private readonly Vector2 _hpOffset = new Vector2(0,150f);

        public void Awake()
        {
            _abilityAgent = GetComponent<AbilityAgent>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void Update()
        {
            if (_hpGague)
            {
               var screenPoint =  RectTransformUtility.WorldToScreenPoint(CameraManager.GetMainCamera(), transform.position);
                _hpGague.rectTransform.anchoredPosition = screenPoint+_hpOffset;
            }
        }

        public void Setup(UnitDataTableRow data)
        {
            _data = data;
            group =  data.group;
            spd = data.spd;
            def = data.def;
            mag = data.mag;
            con = data.con;
            dex = data.dex;
            mdf = data.mdf;
            str = data.str;
            
            maxhp = UnitUtility.GetMaxHp(this);
            hp = maxhp;
        }

        public void ResetStatus()
        {
            spd = _data.spd;
            def = _data.def;
            mag = _data.mag;
            con = _data.con;
            dex = _data.dex;
            mdf = _data.mdf;
            str = _data.str;
            maxhp = UnitUtility.GetMaxHp(this);
            hp = maxhp;
        }

        public void BindSkill(SkillDataTableRow skill)
        {
            
            IAbilityContext context = new AbilityContext()
            {
                self = GetComponent<UnitCore>(),
                skillEffectPrefab= skill.effectPrefab,
                castTime = skill.castTime,
                AttackTargetOption =  skill.attackTargetOption,
                targetCount =  skill.targetCount,
            };
            
            _abilityAgent.AddEffect(skill.abilityId);
            switch (skill.skillType)
            {
                case SkillType.Basic:
                    _abilityAgent.AddScheduler("Basic", skill.abilityId, UnitUtility.GetAttackInterval(this), ref context);
                    break;
                case SkillType.Active:
                    _abilityAgent.AddScheduler("Active", skill.abilityId, skill.cooldown, ref context);
                    break;
                case SkillType.Passive:
                    _abilityAgent.ExecuteEffectById(skill.abilityId, ref context);
                    break;
            }
        }

        public UnitDataTableRow GetData()
        {
            return _data;
        }


        public void MoveTo(Vector3 position)
        {
            transform.DOMove(position, moveDuration);
        }

        public void Warp(Vector3 position)
        {
            transform.position = position;
        }

        public void OnBeginBattle()
        {
            _abilityAgent.SetUpdate(true);
        }

        public void OnEndBattle()
        {
            _abilityAgent.SetUpdate(false);
        }

        public void OnDamage(float damage)
        {
            hp -= damage;
            Debug.Log($"hp:{hp}");
            Debug.Log($"damage:{damage}");
            _hpGague.OnChange(hp);
            if (hp <= 0)
            {
                onDeadAction?.Invoke(this);
                Destroy(_hpGague.gameObject);
            }
        }

        public void FlipSprite(bool value)
        {
            _spriteRenderer.flipX  = value;
        }

        public void BindHPGauge(GaugeWidget hpGauge)
        {
            _hpGague = hpGauge;
            _hpGague.Setup(maxhp,hp);
        }
    }
}