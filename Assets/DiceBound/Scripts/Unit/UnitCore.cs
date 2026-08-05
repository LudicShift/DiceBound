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


        [SerializeField] private TweenAnimationSequenceConvertor hitSequence;
        [SerializeField] private TweenAnimationSequenceConvertor attackSequence;
        private bool _isBattle;


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
                var screenPoint =
                    RectTransformUtility.WorldToScreenPoint(CameraManager.GetMainCamera(), transform.position);
                _hpGauge.rectTransform.anchoredPosition = screenPoint + new Vector2(0, _data.height);
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

            _spriteRenderer.color = Color.white;
            transform.position = _restorePosition;
            ResetHp();
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

            hitSequence.Play();

            _hpGauge.OnChange(hp);
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

        public void ShowAttackAnimation(Action attackAction)
        {
            Sequence seq = DOTween.Sequence();
            seq.JoinCallback(() => Animate("Attack01"));
            seq.JoinCallback(() => attackSequence.Play());
            seq.JoinCallback(attackAction.Invoke);
            seq.Play();
        }

        public void OnHeal(float damage)
        {
            hp += damage;
            onHealAction?.Invoke(this, (int)damage);
            //hitSequence.Play();
            _hpGauge.OnChange(hp);
            if (hp <= 0)
            {
                StartCoroutine(DeadRoutine());
            }
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
    }
}