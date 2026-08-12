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
        public Action<UnitCore> onDodgeAction;
        public Action<UnitCore, int, bool> onHitAction;
        public Action<UnitCore, int> onHealAction;

        public UnitGroup group;
        private Vector3 _restorePosition;
        private Dictionary<string, Skill> _skills = new Dictionary<string, Skill>();


        private AbilityAgent _abilityAgent;
        private SpriteRenderer _spriteRenderer;
        private GaugeWidget _hpGauge;
        private TierLabelWidget _tierLabel;
        private Animator _animator;
        private SpriteOutliner _outliner;
        private UnitMergeEffectHandler _mergeEffectHandler;


        [SerializeField] private TweenAnimationPlayer appearSequence;
        [SerializeField] private TweenAnimationPlayer hitSequence;
        [SerializeField] private TweenAnimationPlayer attackSequence;
        [SerializeField] private TweenAnimationPlayer deadSequence;
        [SerializeField] private TweenAnimationPlayer dodgeSequence;
        private bool _isBattle;
        private AnimationCallbackBehaviour[] _callBackBehaviours;
        public UnitAttackType attackType;
        private int _tier = 0;
        [HideInInspector] public TooltipProvider tooltipProvider;


        public void Awake()
        {
            tooltipProvider = GetComponent<TooltipProvider>();
            _statAgent = GetComponent<StatAgent>();
            _abilityAgent = GetComponent<AbilityAgent>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _outliner = GetComponentInChildren<SpriteOutliner>();
            _mergeEffectHandler = GetComponentInChildren<UnitMergeEffectHandler>();
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
            if (tooltipProvider.IsHovered())
            {
                tooltipProvider.SetTooltipPosition(transform.position, CalculateTooltipOffset(), false);
            }

            if (IsDead())
            {
                return;
            }

            if (_hpGauge)
            {
                _hpGauge.SetPositionFromWorldPoint(CameraManager.GetMainCamera(), _spriteRenderer.transform.position,
                    new Vector2(0, _data.height));
            }

            if (_tierLabel)
            {
                _tierLabel.SetPositionFromWorldPoint(CameraManager.GetMainCamera(), _spriteRenderer.transform.position,
                    new Vector2(0, _data.height + 30));
            }

            if (_isBattle)
            {
                foreach (var skill in _skills)
                {
                    skill.Value.OnUpdate();
                }
            }
        }

        private Vector2 CalculateTooltipOffset()
        {
            var result = new Vector2();
            result.x = Mathf.Sign(transform.position.x) * -200;
            result.y = 250;
            return result;
        }

        public void Setup(UnitDataTableRow data)
        {
            _tier = 0;
            _data = data;
            group = data.group;
            attackType = data.attackType;

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
            _statAgent.ClearStatModifier("str");
            _statAgent.ClearStatModifier("spd");
            _statAgent.ClearStatModifier("def");
            _statAgent.ClearStatModifier("mag");
            _statAgent.ClearStatModifier("con");
            _statAgent.ClearStatModifier("dex");
            _statAgent.ClearStatModifier("mdf");
            _statAgent.ClearStatModifier("hp");
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

        public Tween Move(Vector3 position)
        {
            return transform.DOMove(position, moveDuration);
        }

        public Tween LocalMove(Vector3 position)
        {
            return transform.DOLocalMove(position, moveDuration);
        }

        public void LocalWarp(Vector3 position)
        {
            transform.localPosition = position;
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
            _spriteRenderer.color = Color.white;
            ResetHp();
            Animate("Idle");
        }
        
        
        public void SetHighlight(bool value, Color color = default, int order = 1)
        {
            _outliner.SetEnable(value);
            _outliner.SetColor(color);
            if (value)
            {
                _spriteRenderer.sortingOrder = order;
            }
            else
            {
                _spriteRenderer.sortingOrder = 0;
            }
        }


        public void Animate(string value)
        {
            _animator.Play(value);
        }

        public void OnDamage(float damage, bool isCritical)
        {
            hp -= damage;
            hp = Mathf.Clamp(hp, 0, StatUtility.GetMaxHp(_statAgent));
            onHitAction?.Invoke(this, (int)damage, isCritical);
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

        public void BindTierLabel(TierLabelWidget tierLabel)
        {
            _tierLabel = tierLabel;
            _tierLabel.OnChange(_tier);
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
            hp = Mathf.Clamp(hp, 0, StatUtility.GetMaxHp(_statAgent));
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

        public void PlayAppear(Action onComplete = null)
        {
            _spriteRenderer.color = new Color();
            _hpGauge.canvasGroup.alpha = 0;
            StartCoroutine(appearSequence.Play(0.2f, () =>
            {
                _hpGauge.canvasGroup.alpha = 1;
                onComplete?.Invoke();
            }));
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public void Upgrade()
        {
            _tier++;

            foreach (var stat in _statAgent.GetAllStats())
            {
                stat.Value.AddModifier(new StatModifier(0.8f, StatModifyType.PercentMult));
            }

            _tierLabel.OnChange(_tier);
            var maxHp = StatUtility.GetMaxHp(_statAgent);
            hp = maxHp;
            _hpGauge.Setup(maxHp, hp);
        }

        public TierLabelWidget GetTierLabel()
        {
            return _tierLabel;
        }

        public IEnumerator ShowUpgradeEffect()
        {
            yield return _mergeEffectHandler.FadeIn();
        }

        public IEnumerator HideUpgradeEffect()
        {
            yield return _mergeEffectHandler.FadeOut();
        }

        public string GetId()
        {
            return _data.id;
        }

        public int GetTier()
        {
            return _tier;
        }

        public void OnDodge()
        {
            StartCoroutine(dodgeSequence.Play());
            onDodgeAction?.Invoke(this);
        }
    }
}