using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
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
        private UnitTrashCan _trashCan;
        [SerializeField] private Color unitColor1;
        [SerializeField] private Color unitColor2;
        private SoundDirector _soundDirector;
        private Vector3 _dragOffset;


        public override IEnumerator OnInitialize()
        {
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();

            InputManager.RegisterAction("Click", PlayerActionType.Started, OnDragBegin);
            InputManager.RegisterAction("Click", PlayerActionType.Canceled, OnDragEnd);
            yield return null;
        }

        public void SetEnable(bool value)
        {
            _mode = value ? UnitPlaceMode.Normal : UnitPlaceMode.Block;
        }


        private void OnDragBegin(InputAction.CallbackContext context)
        {
            if (_mode == UnitPlaceMode.Normal && _hoveredUnit)
            {
                BroAudio.Play(_soundDirector.pickUnitSFX);
                _draggingUnit = _hoveredUnit;
                _draggingUnit.OnPick();
                _draggingUnit.tooltipProvider.SetEnabled(false);
                _replaceCell = allyGrid.FindCellByUnit(_hoveredUnit);
                _replaceCell.RemoveUnit();
                _mode = UnitPlaceMode.Drag;
                _dragOffset =  _draggingUnit.transform.position - InputManager.GetWorldMousePosition();
                allyGrid.Show();
            }
        }

        private void OnDragEnd(InputAction.CallbackContext context)
        {
            if (_mode == UnitPlaceMode.Drag && _draggingUnit)
            {
                _draggingUnit.tooltipProvider.SetEnabled(true);
                if (_trashCan)
                {
                    _replaceCell.RemoveUnit();
                    _trashCan.SetHighlight(false);
                    _trashCan.Execute(_draggingUnit);
                }
                else if (_hoveredCell)
                {
                    BroAudio.Play(_soundDirector.dropUnitSFX);
                    _replaceCell.RemoveUnit();
                    if (!_hoveredCell.IsEmpty())
                    {
                        var unit = _hoveredCell.GetUnit();
                        _replaceCell.PlaceUnit(unit);
                        unit.SetHighlight(false);
                    }
                    _hoveredCell.PlaceUnit(_draggingUnit);
                    _draggingUnit.OnDrop();
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
                    CheckHoveredTrashcan();
                    MoveDraggingUnit();
                    break;
                
            }
        }

        private void MoveDraggingUnit()
        {
            if (_mode == UnitPlaceMode.Drag && _draggingUnit)
            {
                _draggingUnit.transform.position = InputManager.GetWorldMousePosition()+_dragOffset;
            }
        }
        
        private void CheckHoveredTrashcan()
        {
            var result = Physics2D.OverlapCircle(InputManager.GetWorldMousePosition(), 1, LayerMask.GetMask("TrashCan"));
            if (result != null)
            {
                if (!_trashCan)
                {
                    _trashCan = result.GetComponent<UnitTrashCan>();
                    _trashCan.SetHighlight(true);
                }
            }
            else
            {
                if (_trashCan)
                {
                    _trashCan.SetHighlight(false);
                    _trashCan = null;
                }
            }
           
        }
        
        private void CheckHoveredCell()
        {
            var result = Physics2D.OverlapCircle(InputManager.GetWorldMousePosition(), 1, LayerMask.GetMask("Cell"));
            if (result != null)
            {
                if (_hoveredCell)
                {
                    var unit1 = _hoveredCell.GetUnit();
                    unit1?.SetHighlight(false);
                }
                _hoveredCell = result.GetComponent<UnitPlaceCell>();
                var unit = _hoveredCell.GetUnit();
                unit?.SetHighlight(true,unitColor2,0);
            }
            else
            {
                if (_hoveredCell)
                {
                    var unit = _hoveredCell.GetUnit();
                    unit?.SetHighlight(false);
                }
                _hoveredCell = null;
                
            }
        }

        private void CheckHoveredUnit()
        {
            var result = Physics2D.OverlapCircle(InputManager.GetWorldMousePosition(), 1, LayerMask.GetMask("Unit"));
            if (result != null)
            {
                if (_hoveredUnit)
                {
                    _hoveredUnit.SetHighlight(false);
                }
                _hoveredUnit = result.GetComponent<UnitCore>();
                _hoveredUnit.SetHighlight(true,unitColor1);
            }
            else
            {
                if (_hoveredUnit)
                {
                    _hoveredUnit.SetHighlight(false);
                }
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

        public List<UnitCore> GetGeneralTargets(UnitGroup group, int count)
        {
            switch (group)
            {
                case UnitGroup.Ally:
                    return allyGrid.GetGeneralTargets(count);
                    break;
                case UnitGroup.Enemy:
                    return enemyGrid.GetGeneralTargets(count);
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
            }
        }
    }
}