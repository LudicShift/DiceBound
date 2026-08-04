using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using KCoreKit.Scripts.Common;

namespace DiceBound
{
    public class SkillDirector : DirectorBase
    {
        private Dictionary<string,SkillDataTableRow> _skillDictionary;
        private Dictionary<string,SkillEffectDataTableRow> _skillEffectDictionary;

        private Dictionary<string, PrefabPool<SkillEffect>> _effectPools;
        
        public override IEnumerator OnInitialize()
        {
            _skillDictionary = DataTableManager.FindAllRows<SkillDataTableRow>().ToDictionary(x=>x.id);
            _skillEffectDictionary = DataTableManager.FindAllRows<SkillEffectDataTableRow>().ToDictionary(x=>x.id);
            _effectPools = new Dictionary<string, PrefabPool<SkillEffect>>();
            foreach (var effect in _skillEffectDictionary)
            {
                _effectPools.Add(effect.Key, new PrefabPool<SkillEffect>(effect.Value.prefab));
            }
            yield return null;
        }

        public SkillDataTableRow GetSkill(string id)
        {
            if (id != "")
            {
                return _skillDictionary[id];
            }
            else
            {
                return null;
            }
        }

        public SkillEffect GetSkillEffect(string key)
        {
            return _effectPools[key].Get();
        }

        public void Release(string key, SkillEffect effect)
        {
            _effectPools[key].Release(effect);
        }
    }
}