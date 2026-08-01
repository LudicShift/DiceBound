using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;

namespace YatchDungeon
{
    public class SkillDirector : DirectorBase
    {
        private Dictionary<string,SkillDataTableRow> _skillDataMap;

        public override IEnumerator OnInitialize()
        {
            _skillDataMap = DataTableManager.FindAllRows<SkillDataTableRow>().ToDictionary(x=>x.id);
            yield return null;
        }

        public SkillDataTableRow GetSkill(string id)
        {
            return _skillDataMap[id];
        }
        
    }
}