using System;
using System.Collections;
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
        public Action<UnitCore,int> onHitAction;
        public Action<UnitCore,int> onHealAction;
        private AbilityAgent _abilityAgent;
        public UnitGroup group;
        private SpriteRenderer _spriteRenderer;
        private GaugeWidget _hpGague;
        
        private Animator _animator;
        private Vector3 _restorePosition;
        private float _direction;

        public void Awake()
        {
            _abilityAgent = GetComponent<AbilityAgent>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _animator = GetComponentInChildren<Animator>();
            _animator.speed = 1.5f;
        }

        public void Update()
        {
            if (_hpGague)
            {
               var screenPoint =  RectTransformUtility.WorldToScreenPoint(CameraManager.GetMainCamera(), transform.position);
                _hpGague.rectTransform.anchoredPosition = screenPoint+new Vector2( 0,_data.height);
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
            _restorePosition = transform.position;
        }

        public void OnEndBattle()
        {
            _abilityAgent.SetUpdate(false);
            _spriteRenderer.color = Color.white;
            transform.position = _restorePosition;
            Animate("Idle");
        }

        public void Animate(string value)
        {
            _animator.Play(value);
        }

        public void OnDamage(float damage)
        {
            hp -= damage;
            onHitAction?.Invoke(this,(int)damage);
            Animate("Hurt");

            _spriteRenderer.DOColor(Color.red, 0.2f).OnComplete(() =>
            {
                _spriteRenderer.DOColor(Color.white, 0.2f);
            });
            
            _hpGague.OnChange(hp);
            if (hp <= 0)
            {
                StartCoroutine(DeadRoutine());
            }
        }

        private IEnumerator DeadRoutine()
        {
            Animate("Death");
            yield return new WaitForSeconds(0.5f);
            onDeadAction?.Invoke(this);
            Destroy(_hpGague.gameObject);
        }

        public void FlipSprite(bool value)
        {
            _spriteRenderer.flipX  = value;
            _direction = value ? -1 : 1;
        }

        public void BindHPGauge(GaugeWidget hpGauge)
        {
            _hpGague = hpGauge;
            _hpGague.Setup(maxhp,hp);
        }

        public void ShowAttackAnimation(Action attackAction)
        {
            Sequence seq = DOTween.Sequence();
            seq.Join(transform.DOMoveX(1.5f*_direction, 0.075f).SetRelative(true));
            seq.AppendCallback(()=> Animate("Attack01"));
            seq.AppendInterval(0.6f);
            seq.AppendCallback(attackAction.Invoke);
            
            seq.Append(transform.DOMoveX(-1.5f*_direction, 0.075f).SetRelative(true));
            
            seq.Play();
        }

        public void OnHeal(float damage)
        {
            hp += damage;
            onHealAction?.Invoke(this,(int)damage);
            _hpGague.OnChange(hp);
            if (hp <= 0)
            {
                StartCoroutine(DeadRoutine());
            }
        }
    }
}