using UnityEngine;

namespace YatchDungeon
{
    public class UnitPlaceCell : MonoBehaviour
    {
        private UnitCore _unit;

        public bool IsEmpty()
        {
            return !_unit;
        }

        public void SetUnit(UnitCore unit)
        {
            _unit = unit;
        }
        
    }
}