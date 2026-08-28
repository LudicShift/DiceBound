using System;
using System.Collections;
using System.Collections.Generic;
using AutoGroupGenerator;
using DG.Tweening;
using KCoreKit;
using KCoreKit.Scripts;
using UnityEngine;

namespace DiceBound
{
    public class UnitCore : MonoBehaviour
    {
        // 등급별 체력바 색상 - 어떤 유닛이 무슨 등급인지 한눈에 구분되도록 (기획 요청).
        private static readonly Dictionary<string, Color> GradeColors = new Dictionary<string, Color>
        {
            { "Common", new Color(0.78f, 0.78f, 0.78f) },     // 회백색
            { "Rare", new Color(0.30f, 0.55f, 0.95f) },       // 파랑
            { "Epic", new Color(0.65f, 0.35f, 0.95f) },       // 보라
            { "Legendary", new Color(1f, 0.72f, 0.15f) },     // 금색
        };

        private UnitDataTableRow _data;
        [SerializeField] private float moveDuration = 0.3f;

        private StatAgent _statAgent;

        public float _hp;

        public Action<UnitCore> onDeadAction;
        public Action<UnitCore> onDodgeAction;
        public Action<UnitCore, int, bool> onHitAction;
        public Action<UnitCore, int> onHealAction;

        public UnitGroup group;
        private Vector3 _restorePosition;
        private Dictionary<string, Skill> _skills = new Dictionary<string, Skill>();


        private AbilityAgent _abilityAgent;
        private SpriteRenderer _spriteRenderer;
        private UnitInfoWidget _unitInfoWidget;
        private Animator _animator;
        private SpriteOutliner _outliner;
        private UnitFusionEffectHandler _fusionEffectHandler;
        [HideInInspector] public UnitInputHandler inputHandler;


        [SerializeField] private TweenAnimationPlayer appearSequence;
        [SerializeField] private TweenAnimationPlayer hitSequence;
        [SerializeField] private TweenAnimationPlayer attackSequence;
        [SerializeField] private TweenAnimationPlayer deadSequence;
        [SerializeField] private TweenAnimationPlayer dodgeSequence;


        private bool _isBattle;
        private AnimationCallbackBehaviour[] _callBackBehaviours;
        public UnitAttackType attackType;
        private string _lastAttackAnim = "Attack";

        [HideInInspector] public TooltipProvider tooltipProvider;
        public BattleContext battleContext;
        private string _statTooltipFormat;
        private string _unitName;
        private string _currentAnimation;
 

        public void Awake()
        {
            _statTooltipFormat = LocalizationManager.GetLocalizedText("unitStatTooltipFormat");

            tooltipProvider = GetComponent<TooltipProvider>();
            _statAgent = GetComponent<StatAgent>();
            _abilityAgent = GetComponent<AbilityAgent>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _outliner = GetComponentInChildren<SpriteOutliner>();
            inputHandler = GetComponentInChildren<UnitInputHandler>();
            _fusionEffectHandler = GetComponentInChildren<UnitFusionEffectHandler>();
            _animator = GetComponentInChildren<Animator>();
            _statAgent.AddStat("hp");
            _statAgent.AddStat("str");
            _statAgent.AddStat("spd");
            _statAgent.AddStat("def");
            _statAgent.AddStat("mag");
            _statAgent.AddStat("con");
            _statAgent.AddStat("dex");
            _statAgent.AddStat("mdf");
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

            if (_unitInfoWidget)
            {
                _unitInfoWidget.OnUpdate();
                _unitInfoWidget.SetPositionFromWorldPoint(CameraManager.GetMainCamera(),
                    _spriteRenderer.transform.position,
                    new Vector2(0, _data.height));
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
        
        public void Setup(UnitDataTableRow data, UnitGroup group)
        {
            // 풀에서 재사용된 인스턴스가 이전 유닛의 연출(Tweens)로 흐트러진 Sprite 트랜스폼을 물려받지 않도록 방어적으로 초기화.
            ResetVisualState();
            _data = data;
            this.group = group;
            _unitInfoWidget.SetFlip(group == UnitGroup.Enemy);
            _unitName = LocalizationManager.GetLocalizedText(data.nameKey);
            _skills = new Dictionary<string, Skill>();
            attackType = data.attackType;
            
            BindAnimatorController(data.animator);
            _statAgent.ClearStatModifier("hp");
            _statAgent.ClearStatModifier("str");
            _statAgent.ClearStatModifier("spd");
            _statAgent.ClearStatModifier("def");
            _statAgent.ClearStatModifier("mag");
            _statAgent.ClearStatModifier("con");
            _statAgent.ClearStatModifier("dex");
            _statAgent.ClearStatModifier("mdf");
            _statAgent.SetBaseValue("hp",data.hp);
            _statAgent.SetBaseValue("str",data.str);
            _statAgent.SetBaseValue("spd",data.spd);
            _statAgent.SetBaseValue("def",data.def);
            _statAgent.SetBaseValue("mag",data.mag);
            _statAgent.SetBaseValue("con",data.con);
            _statAgent.SetBaseValue("dex",data.dex);
            _statAgent.SetBaseValue("mdf",data.mdf);
            
            tooltipProvider.SetText("name",_unitName);
            tooltipProvider.SetText("desc",GetTooltipText());
            _hp = StatUtility.GetMaxHp(_statAgent);
            _unitInfoWidget.SetMaxHp(_hp);
            var gradeColor = GradeColors.TryGetValue(data.grade, out var color) ? color : Color.white;
            _unitInfoWidget.SetHpGaugeColor(gradeColor);
        }

        private string GetTooltipText()
        {
            var hp = _statAgent.GetStat("hp");
            var str = _statAgent.GetStat("str");
            var spd = _statAgent.GetStat("spd");
            var def = _statAgent.GetStat("def");
            var mag = _statAgent.GetStat("mag");
            var con = _statAgent.GetStat("con");
            var dex = _statAgent.GetStat("dex");
            var mdf = _statAgent.GetStat("mdf");

            return string.Format(_statTooltipFormat,hp, str, spd, def, mag, con, dex, mdf);
        }


        private void BindAnimatorController(RuntimeAnimatorController controller)
        {
            // 컨트롤러를 교체하면 이전 behaviour 인스턴스는 버려지므로 구독도 함께 정리한다.
            if (_callBackBehaviours != null)
            {
                foreach (var behaviour in _callBackBehaviours)
                {
                    if (behaviour)
                    {
                        behaviour.callback -= OnAnimationCallback;
                    }
                }
            }

            _animator.runtimeAnimatorController = controller;
            _callBackBehaviours = _animator.GetBehaviours<AnimationCallbackBehaviour>();

            foreach (var behaviour in _callBackBehaviours)
            {
                behaviour.callback += OnAnimationCallback;
            }
        }

        /// <summary>
        /// 스탯·스킬 없이 외형(애니메이터)만 교체한다. 연출 확인용 테스트 씬에서 사용.
        /// </summary>
        public void SetupAppearanceOnly(UnitDataTableRow data)
        {
            _data = data;
            attackType = data.attackType;
            BindAnimatorController(data.animator);
        }
        public void ResetVisualState()
        {
            _spriteRenderer.transform.localPosition = Vector3.zero;
            _spriteRenderer.color = Color.white;
        }
        
        public void PlayHitAnimation()
        {
            if (_currentAnimation == "Idle" ||  _currentAnimation == "Hurt")
            {
                Animate("Hurt");
            }
            if (hitSequence)
            {
                StartCoroutine(hitSequence.Play());
            }
        }

        public void ResetHp()
        {
            _hp = StatUtility.GetMaxHp(_statAgent);
            _unitInfoWidget.SetHp(_hp);
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
            switch (skill.type)
            {
                case SkillType.Basic:
                    _unitInfoWidget.BindBasicSkill(skill);
                    break;
                case SkillType.Active:
                    _unitInfoWidget.BindActiveSkill(skill);
                    break;
            }
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
            SetHighlight(false);
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
            _outliner?.SetEnable(value);
            _outliner?.SetColor(color);
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
            _currentAnimation = value;
            _animator.Play(_currentAnimation);
        }

        public void OnDamage(float damage, bool isCritical)
        {
            _hp -= damage;
            _hp = Mathf.Clamp(_hp, 0, StatUtility.GetMaxHp(_statAgent));
            onHitAction?.Invoke(this, (int)damage, isCritical);
            Animate("Hurt");

            StartCoroutine(hitSequence.Play());
            _unitInfoWidget.SetHp(_hp);
            if (_isBattle && _hp <= 0)
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


        public void PlayAttackAnimation(string clip = "Attack")
        {
            _lastAttackAnim = string.IsNullOrEmpty(clip) ? "Attack" : clip;
            Animate(_lastAttackAnim);
        }

        public void PlayAttackTween()
        {
            StartCoroutine(attackSequence.Play());
        }

        private void OnAnimationCallback(AnimatorStateInfo info)
        {
            if (info.IsName(_lastAttackAnim) || info.IsName("Hurt"))
            {
                Animate("Idle");
            }
        }


        public void OnHeal(float damage)
        {
            _hp += damage;
            _hp = Mathf.Clamp(_hp, 0, StatUtility.GetMaxHp(_statAgent));
            onHealAction?.Invoke(this, (int)damage);
            _unitInfoWidget.SetHp(_hp);
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
            return _hp <= 0;
        }

        public void PlayAppear(Action onComplete = null)
        {
            StartCoroutine(appearSequence.Play(0.2f, () =>
            {
                onComplete?.Invoke();
            }));
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }
        
        public IEnumerator ShowFusionEffect()
        {
            yield return _fusionEffectHandler.FadeIn();
        }

        public IEnumerator HideFusionEffect()
        {
            yield return _fusionEffectHandler.FadeOut();
        }

        public string GetId()
        {
            return _data.id;
        }
        
        public void OnDodge()
        {
            StartCoroutine(dodgeSequence.Play());
            onDodgeAction?.Invoke(this);
        }

        public float GetHpRate()
        {
            return _hp / _statAgent.GetStat("hp").Value;
        }

        public float GetHp()
        {
            return _hp;
        }

        public UnitInfoWidget GetUnitInfoWidget()
        {
            return _unitInfoWidget;
        }

        public void BindInfoWidget(UnitInfoWidget infoWidget)
        {
            _unitInfoWidget = infoWidget;
        }

        public void OnRelease()
        {
            // 죽음/합성 등으로 연출(Tweens) 도중 풀에 반납되면 Sprite 트랜스폼이 흐트러진 채로 남을 수 있다.
            // 다음에 이 인스턴스를 재사용할 때(다른 유닛일 수도 있음) 그대로 물려받아 UI/외형이 어긋나므로 여기서 원상복구한다.
            ResetVisualState();
            _unitInfoWidget.ReleaseSkills();
            onDeadAction = null;
            onHitAction = null;
            onHealAction = null;
            onDodgeAction = null;
        }
    }
}