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
        private SkillTargetOption _targetOption;


        public Skill(SkillDataTableRow data)
        {
            _data = data;
            type = data.skillType;
            name = data.nameKey;
            desc = data.descKey;
            _abilityId = data.abilityId;
            effectKey = data.effectKey;
            _castTime = data.castTime;
            _cooldown = data.cooldown;
            _targetCount = data.targetCount;
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
                targetCount = _targetCount,
                targetOption = _targetOption,
                animClip = string.IsNullOrEmpty(_data.animClip) ? "Attack" : _data.animClip,
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

        public void OnUpdate()
        {
            switch (type)
            {
                case SkillType.Basic:
                    if (_owner.GetAttackInterval() <_timeElapsed )
                    {
                        _abilityAgent.ExecuteEffectById(_abilityId, ref _abilityContext);
                        _timeElapsed = 0;
                    }
                    break;
                case SkillType.Active:
                    if (_cooldown < _timeElapsed)
                    {
                        _abilityAgent.ExecuteEffectById(_abilityId,ref _abilityContext);
                        _timeElapsed = 0;
                    }
                    break;
            }

            _timeElapsed += Time.deltaTime;
        }

        public void OnBattleEnd()
        {
          
        }
    }
}