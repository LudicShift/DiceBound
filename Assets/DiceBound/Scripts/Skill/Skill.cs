using System;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class Skill
    {
        public SkillType type;
        public string name;
        public string desc;
        private UnitCore _owner;
        private AbilityAgent _abilityAgent;
        private readonly string _abilityId;
        private AbilityContext _abilityContext;
        private readonly string effectKey;
        private readonly float _castTime;
        private readonly float _cooldown;
        private int _targetCount;
        private float _timeElapsed;
        private readonly SkillDataTableRow _data;
        private SkillTargetGroup _targetGroup;
        private SkillTargetOption _targetOption;
        private readonly int _priority;
        private readonly string _animClip;
        private readonly float _startUpDelay;
        

        public float GetCurrentTime()
        {
            return _timeElapsed;
        }
        
        
        public Skill(SkillDataTableRow data)
        {
            _data = data;
            type = data.skillType;
            name = data.nameKey;
            desc = data.descKey;
            _priority = data.priority;
            _abilityId = data.abilityId;
            effectKey = data.effectKey;
            _castTime = data.castTime;
            _animClip =  _data.animClip;
            _startUpDelay = _data.startUpDelay;
            _cooldown = data.cooldown;
            _targetCount = data.targetCount;
            _targetGroup = data.targetGroup;
            _targetOption = data.targetOption;
        }

        public void SetOwner(UnitCore owner)
        {
            _owner = owner;
            _abilityAgent = owner.GetComponent<AbilityAgent>();
            _abilityAgent.AddEffect(_abilityId);
            _abilityContext =  new AbilityContext()
            {
                self =  owner,
                skillEffectKey= effectKey,
                castTime = _castTime,
                priority = _priority,
                targetCount = _targetCount,
                targetGroup = _targetGroup,
                targetOption = _targetOption,
                animClip = _animClip,
                startUpDelay = _startUpDelay,
            };
        }

        public void OnBattleBegin()
        {
            switch (type)
            {
                case SkillType.Passive:
                    _abilityAgent.ExecuteEffectById(_abilityId,ref _abilityContext);
                    break;
            }
        }

        public float GetInterval()
        {
            switch (type)
            {
                case SkillType.Basic:
                    return _owner.GetAttackInterval();
                case SkillType.Active:
                    return _cooldown;
            }

            return 0;
        }
        
        public void OnUpdate()
        {
            if (GetInterval() < _timeElapsed)
            {
                _abilityAgent.ExecuteEffectById(_abilityId, ref _abilityContext);
                _timeElapsed = 0;
            }

            _timeElapsed += Time.deltaTime;
        }

        public void OnBattleEnd()
        {
            _timeElapsed = 0;
        }
    }
}