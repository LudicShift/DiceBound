using System;
using KCoreKit;

namespace DiceBound
{
    public class SkillCooldownGaugeWidget : GaugeWidget
    {
        private Skill _skill;
        public void Bind(Skill skill)
        {
            _skill = skill;
            Setup(_skill.GetInterval());
        }

        public void OnUpgrade()
        {
            if (_skill !=null)
            {
                Setup(_skill.GetInterval());
                
            }
        }

        public void OnUpdate()
        {
            if (_skill != null)
            {
                OnChange(_skill.GetCurrentTime());
            }
        }

        public void Release()
        {
            _skill = null;
        }
    }
}