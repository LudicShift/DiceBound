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
        private UnitPlaceCellBase _hoveredCell;
        private UnitPlaceCellBase _replaceCell;

        [SerializeField] private UnitPlaceGrid allyGrid;
        [SerializeField] private UnitPlaceGrid enemyGrid;
        
        private Dictionary<UnitCore, UnitPlaceCellBase> _placeDictionary = new Dictionary<UnitCore, UnitPlaceCellBase>();
        
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
                _draggingUnit.inputHandler.OnPick();
                _draggingUnit.tooltipProvider.SetEnabled(false);
                _replaceCell = _placeDictionary[_hoveredUnit];
                RemoveUnit(_hoveredUnit);
                _mode = UnitPlaceMode.Drag;
                _dragOffset =  _draggingUnit.transform.position - InputManager.GetWorldPointerPosition();
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
                        PlaceUnit(unit,_replaceCell );
                        unit.SetHighlight(false);
                    }
                    PlaceUnit(_draggingUnit,_hoveredCell );
                    _draggingUnit.inputHandler.OnDrop();
                }
                else
                {
                    PlaceUnit(_draggingUnit,_replaceCell,false);
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
                _draggingUnit.transform.position = InputManager.GetWorldPointerPosition()+_dragOffset;
            }
        }
        
        private void CheckHoveredTrashcan()
        {
            var result = Physics2D.OverlapCircle(InputManager.GetWorldPointerPosition(), 1, LayerMask.GetMask("TrashCan"));
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
            var result = Physics2D.OverlapCircle(InputManager.GetWorldPointerPosition(), 1, LayerMask.GetMask("Cell"));
            if (result != null)
            {
                if (_hoveredCell)
                {
                    var unit1 = _hoveredCell.GetUnit();
                    unit1?.SetHighlight(false);
                }
                _hoveredCell = result.GetComponent<UnitPlaceCellBase>();
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
            var result = Physics2D.OverlapCircle(InputManager.GetWorldPointerPosition(), 1, LayerMask.GetMask("Unit"));
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

        public UnitPlaceCellBase GetCellByUnit(UnitCore unit)
        {
            return _placeDictionary[unit];
        }

        public UnitPlaceCell GetAllyCell(int index)
        {
            return allyGrid.GetCell(index);
        }

        public int GetAllyCellIndex(UnitCore unit)
        {
            return allyGrid.GetCellIndex(unit);
        }

        public UnitPlaceCell GetCell(UnitGroup group, int index)
        {
            switch (group)
            {
                case UnitGroup.Ally:
                    return allyGrid.GetCell(index);
                case UnitGroup.Enemy:
                    return enemyGrid.GetCell(index);
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
            }
        }

        public int GetCellIndex(UnitGroup group, UnitCore unit)
        {
            switch (group)
            {
                case UnitGroup.Ally:
                    return allyGrid.GetCellIndex(unit);
                case UnitGroup.Enemy:
                    return enemyGrid.GetCellIndex(unit);
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
            }
        }

        public void PlaceUnit(UnitCore unit, UnitPlaceCellBase cell, bool warp = true)
        {
            cell.PlaceUnit(unit,warp);
            _placeDictionary.Add(unit,cell);
        }

        public void RemoveUnit(UnitCore unit)
        {
            _placeDictionary[unit].RemoveUnit();
            _placeDictionary.Remove(unit);
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

        public UnitPlaceCell GetRandomEmptyCell(UnitGroup group, UnitAttackType attackType)
        {
            switch (group)
            {
                case UnitGroup.Ally:
                    return allyGrid.GetRandomEmptyCell(attackType);
                    break;
                case UnitGroup.Enemy:
                    return enemyGrid.GetRandomEmptyCell(attackType);
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
            }
        }
    }
}