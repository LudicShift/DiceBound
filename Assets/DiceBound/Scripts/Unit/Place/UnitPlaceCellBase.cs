using System;
using UnityEngine;

namespace DiceBound
{
    public abstract class UnitPlaceCellBase :MonoBehaviour
    {
        private UnitCore _unit;
        public Action<UnitCore> onPlaceAction;
        public void RemoveUnit()
        {
            _unit = null;
        }

        public bool IsEmpty()
        {
            return !_unit;
        }

        public UnitCore GetUnit()
        {
            return _unit;
        }

        public void PlaceUnit(UnitCore unit, bool warp = true)
        {
            _unit = unit;
            _unit.SetParent(transform);
            if (warp)
            {
                _unit.LocalWarp(Vector3.zero);
            }
            else
            {
                _unit.LocalMove(Vector3.zero);
            }
            onPlaceAction?.Invoke(unit);
        }

    }
}