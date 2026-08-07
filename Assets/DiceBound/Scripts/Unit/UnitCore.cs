using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KCoreKit;
using KCoreKit.Scripts;
using UnityEngine;

namespace DiceBound
{
    public class UnitCore : MonoBehaviour
    {
        private UnitDataTableRow _data;
        [SerializeField] private float moveDuration = 0.3f;

        private StatAgent _statAgent;

        public float hp;

        public Action<UnitCore> onDeadAction;
        public Action<UnitCore, int> onHitAction;
        public Action<UnitCore, int> onHealAction;

        public UnitGroup group;
        private Vector3 _restorePosition;
        private Dictionary<string, Skill> _skills = new Dictionary<string, Skill>();


        private AbilityAgent _abilityAgent;
        private SpriteRenderer _spriteRenderer;
        private GaugeWidget _hpGauge;
        private Animator _animator;


        [SerializeField] private TweenAnimationCombiner appearSequence;
        [SerializeField] private TweenAnimationCombiner hitSequence;
        [SerializeField] private TweenAnimationCombiner attackSequence;
        [SerializeField] private TweenAnimationCombiner deadSequence;
        private bool _isBattle;
        private AnimationCallbackBehaviour[] _callBackBehaviours;


        public void Awake()
        {
            _statAgent = GetComponent<StatAgent>();
            _abilityAgent = GetComponent<AbilityAgent>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _animator = GetComponentInChildren<Animator>();
            _animator.speed = 1.5f;
            _statAgent.AddStat("str");
            _statAgent.AddStat("spd");
            _statAgent.AddStat("def");
            _statAgent.AddStat("mag");
            _statAgent.AddStat("con");
            _statAgent.AddStat("dex");
            _statAgent.AddStat("mdf");
            _statAgent.AddStat("hp");
        }

        public void Update()
        {
            if (IsDead())
            {
                return;
            }

            if (_hpGauge)
            {
               _hpGauge.SetPositionFromWorldPoint(CameraManager.GetMainCamera(), _spriteRenderer.transform.position, new Vector2(0,_data.height));
            }

            if (_isBattle)
            {
                foreach (var skill in _skills)
                {
                    skill.Value.OnUpdate();
                }
            }
        }

        public void Setup(UnitDataTableRow data)
        {
            _data = data;
            group = data.group;
            
            _animator.runtimeAnimatorController = data.animator;
            _callBackBehaviours = _animator.GetBehaviours<AnimationCallbackBehaviour>();
            
            foreach (var behaviour in _callBackBehaviours)
            {
                behaviour.callback += OnAnimationCallback;
            }

            _skills = new Dictionary<string, Skill>();
            _statAgent.SetBaseValue("str", data.str);
            _statAgent.SetBaseValue("spd", data.spd);
            _statAgent.SetBaseValue("def", data.def);
            _statAgent.SetBaseValue("mag", data.mag);
            _statAgent.SetBaseValue("con", data.con);
            _statAgent.SetBaseValue("dex", data.dex);
            _statAgent.SetBaseValue("mdf", data.mdf);
            _statAgent.SetBaseValue("hp", data.hp);
            hp = StatUtility.GetMaxHp(_statAgent);
        }

       
        public void ResetHp()
        {
            hp = StatUtility.GetMaxHp(_statAgent);
            _hpGauge.OnChange(hp);
        }

        public void BindSkill(SkillDataTableRow data)
        {
            if (!data)
            {
                return;
            }

            var skill = new Skill(data);
            _skills.Add(data.id, skill);
            skill.SetOwner(this);
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

        public void OnBattleBegin()
        {
            _isBattle = true;
            _restorePosition = transform.position;
            foreach (var skill in _skills)
            {
                skill.Value.OnBattleBegin();
            }
        }

        public void OnBattleEnd()
        {
            _isBattle = false;
            foreach (var skill in _skills)
            {
                skill.Value.OnBattleEnd();
            }

            transform.position = _restorePosition;

            if (!IsDead())
            {
                _spriteRenderer.color = Color.white;
                ResetHp();
                Animate("Idle");
            }
        }

        public void Revive()
        {
            _spriteRenderer.color = Color.white;
            ResetHp();
            _hpGauge.OnChange(hp);
            Animate("Idle");
        }

        public void Animate(string value)
        {
            _animator.Play(value);
            
        }

        public void OnDamage(float damage)
        {
            hp -= damage;
            onHitAction?.Invoke(this, (int)damage);
            Animate("Hurt");

            StartCoroutine(hitSequence.Play());
            _hpGauge.OnChange(hp);
            
            if (_isBattle && hp <= 0)
            {
                StartCoroutine(DeadRoutine());
            }
        }

        private IEnumerator DeadRoutine()
        {
            Animate("Death");
            _isBattle = false;
            StartCoroutine(deadSequence.Play());
            yield return new WaitForSeconds(0.5f);
            onDeadAction?.Invoke(this);
        }

        public void FlipSprite(bool value)
        {
            _spriteRenderer.flipX = value;
        }

        public void BindHpGauge(GaugeWidget hpGauge)
        {
            _hpGauge = hpGauge;
            _hpGauge.Setup(StatUtility.GetMaxHp(_statAgent), hp);
        }
        public GaugeWidget GetHpGauge()
        {
           return _hpGauge;
        }
        public void ReleaseHpGauge(GaugeWidget hpGauge)
        {
            _hpGauge = null;
        }

    
        public void ShowAttackAnimation(Action attackAction)
        {
            Animate("Attack");
            StartCoroutine(attackSequence.Play());
            attackAction.Invoke();
        }
        
        private void OnAnimationCallback(AnimatorStateInfo info)
        {
            if (info.IsName("Attack") || info.IsName("Hurt"))
            {
                Animate("Idle");
            }
        }

        
        public void OnHeal(float damage)
        {
            hp += damage;
            onHealAction?.Invoke(this, (int)damage);
            //hitSequence.Play();
            _hpGauge.OnChange(hp);
        }

        public GaugeWidget ReleaseHpGauge()
        {
            var hpGauge = _hpGauge;
            _hpGauge = null;
            return hpGauge;
        }

        public float GetAttackInterval()
        {
            return StatUtility.GetAttackInterval(_statAgent);
        }

        public StatAgent GetStatAgent()
        {
            return _statAgent;
        }

        public bool IsDead()
        {
            return hp <= 0;
        }

        public void PlayAppear()
        {
            StartCoroutine(appearSequence.Play(0.2f));
        }
    }
}