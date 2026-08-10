using System;
using System.Collections;
using KCoreKit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceBound
{
    public enum UnitPlaceMode
    {
        Normal,
        Drag,
        Block
    }

    public class UnitPlaceDirector : DirectorBase
    {
        private UnitPlaceMode _mode;

        private UnitCore _hoveredUnit;
        private UnitCore _draggingUnit;
        private UnitPlaceCell _hoveredCell;
        private UnitPlaceCell _replaceCell;

        [SerializeField] private UnitPlaceGrid allyGrid;

        [SerializeField] private UnitPlaceGrid enemyGrid;

        public override IEnumerator OnInitialize()
        {
            InputManager.RegisterAction("Click", PlayerActionType.Started, OnDragBegin);
            InputManager.RegisterAction("Click", PlayerActionType.Canceled, OnDragEnd);
            yield return null;
        }


        private void OnDragBegin(InputAction.CallbackContext context)
        {
            if (_mode == UnitPlaceMode.Normal && _hoveredUnit)
            {
                _draggingUnit = _hoveredUnit;
                _replaceCell = allyGrid.FindCellByUnit(_hoveredUnit);
                _mode = UnitPlaceMode.Drag;
                allyGrid.Show();
            }
        }

        private void OnDragEnd(InputAction.CallbackContext context)
        {
            if (_mode == UnitPlaceMode.Drag && _draggingUnit)
            {
                if (_hoveredCell)
                {
                    _replaceCell.RemoveUnit();
                    if (!_hoveredCell.IsEmpty())
                    {
                        var unit = _hoveredCell.GetUnit();
                        _replaceCell.PlaceUnit(unit);
                    }
                    _hoveredCell.PlaceUnit(_draggingUnit);
                }
                else
                {
                    _replaceCell.PlaceUnit(_draggingUnit, false);
                }

                _mode = UnitPlaceMode.Normal;
                allyGrid.Hide();
            }
        }

        public void Update()
        {
            switch (_mode)
            {
                case UnitPlaceMode.Normal:
                    CheckHoveredUnit();
                    break;
                case UnitPlaceMode.Drag:
                    CheckHoveredCell();
                    MoveDraggingUnit();
                    break;
            }
        }

        private void MoveDraggingUnit()
        {
            if (_mode == UnitPlaceMode.Drag && _draggingUnit)
            {
                _draggingUnit.transform.position = InputManager.GetWorldMousePosition();
            }
        }

        private void CheckHoveredCell()
        {
            var result = Physics2D.OverlapCircle(InputManager.GetWorldMousePosition(), 1, LayerMask.GetMask("Cell"));
            if (result != null)
            {
                _hoveredCell = result.GetComponent<UnitPlaceCell>();
            }
            else
            {
                _hoveredCell = null;
            }
        }

        private void CheckHoveredUnit()
        {
            var result = Physics2D.OverlapCircle(InputManager.GetWorldMousePosition(), 1, LayerMask.GetMask("Unit"));
            if (result != null)
            {
                _hoveredUnit = result.GetComponent<UnitCore>();
            }
            else
            {
                _hoveredUnit = null;
            }
        }

        public void PlaceUnit(UnitCore unit)
        {
            switch (unit.group)
            {
                case UnitGroup.Ally:
                    var cell1 = allyGrid.GetRandomEmptyCell(unit.attackType);
                    cell1.PlaceUnit(unit);
                    break;
                case UnitGroup.Enemy:
                    var cell2 = enemyGrid.GetRandomEmptyCell(unit.attackType);
                    cell2.PlaceUnit(unit);
                    break;
            }
        }

        public void RemoveUnit(UnitCore unit)
        {
            switch (unit.group)
            {
                case UnitGroup.Ally:
                    allyGrid.RemoveUnit(unit);
                    break;
                case UnitGroup.Enemy:
                    enemyGrid.RemoveUnit(unit);
                    break;
            }
        }
    }
}