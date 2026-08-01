using System.Collections;
using KCoreKit;

namespace YatchDungeon
{
    public class BattleDirector : DirectorBase
    {
        private UnitDirector _unitDirector;

        public override IEnumerator OnInitialize()
        {
            _unitDirector = DirectorFacade.GetSubMode<UnitDirector>();
            yield return null;
        }

        public void BeginBattle()
        {
           var units =  _unitDirector.GetAllUnit();
           foreach (var unit in units)
           {
               unit.OnBeginBattle();
           }
        }

        public void EndBattle()
        {
            var units =  _unitDirector.GetAllUnit();
            foreach (var unit in units)
            {
                unit.OnEndBattle();
            }
        }
    }
}