using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace YatchDungeon
{
    public class UnitDirector : DirectorBase
    {
        private Dictionary<string, UnitDataTableRow> _unitDataMap;
        private UnitPlaceCell _hoveredCell;
        private UnitPlaceCell _restoreCell;
        private UnitCore _draggingUnit;
        private Vector3 _dragOffset;
        private Camera _camera;
        [SerializeField] private UnitPlaceGrid allyPlaceGrid;
        [SerializeField] private UnitPlaceGrid enemyPlaceGrid;

        public override IEnumerator OnInitialize()
        {
            _camera = CameraManager.GetMainCamera();
            _unitDataMap = DataTableManager.FindAllRows<UnitDataTableRow>().ToDictionary(x => x.id);

            InputManager.RegisterAction("Click", PlayerActionType.Performed, OnMouseDownUnit);
            InputManager.RegisterAction("Click", PlayerActionType.Canceled, OnMouseUpUnit);
            yield return null;
        }

        private void OnMouseUpUnit(InputAction.CallbackContext obj)
        {
            if (_draggingUnit)
            {
                if (_hoveredCell)
                {
                    if (!_hoveredCell.IsEmpty())
                    {
                        var unit = _hoveredCell.PopUnit();
                        unit.MoveTo(_restoreCell.transform.position);
                        _restoreCell.PushUnit(unit);
                    }

                    _draggingUnit.Warp(_hoveredCell.transform.position);
                    _hoveredCell.PushUnit(_draggingUnit);
                }
                else
                {
                    _draggingUnit.MoveTo(_restoreCell.transform.position);
                    _restoreCell.PushUnit(_draggingUnit);
                }

                _restoreCell = null;
                _draggingUnit = null;
                _dragOffset = Vector3.zero;
            }
        }

        private void OnMouseDownUnit(InputAction.CallbackContext obj)
        {
            if (_hoveredCell && !_hoveredCell.IsEmpty())
            {
                _draggingUnit = _hoveredCell.PopUnit();
                _restoreCell = _hoveredCell;
                // 1. 마우스 스크린 좌표 -> 월드 좌표 변환
                Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
                mouseScreenPos.z = -_camera.transform.position.z; // 카메라 거리 맞추기
                Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);

                // 2. 유닛과 마우스 위치의 오프셋 저장 (유닛이 마우스 포인터 기준 갑자기 확 튀는 현상 방지)
                _dragOffset = _draggingUnit.transform.position - mouseWorldPos;
                _dragOffset.z = 0; // 2전용이므로 Z축 고정
            }
        }


        public void Update()
        {
            if (_camera)
            {
                CheckDraggingUnit();
                CheckHoverUnitCell();
            }
        }

        private void CheckDraggingUnit()
        {
            if (_draggingUnit)
            {
                // 마우스 위치를 월드 좌표로 변환하여 유닛 위치 추적
                Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
                mouseScreenPos.z = -_camera.transform.position.z;
                Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);

                Vector3 targetPos = mouseWorldPos + _dragOffset;
                targetPos.z = 0; // 2D 환경 Z축 고정

                _draggingUnit.transform.position = targetPos;
            }
        }

        private void CheckHoverUnitCell()
        {
            // 마우스 스크린 좌표를 월드 좌표로 변환 후 OverlapCircle 수행
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            mouseScreenPos.z = -_camera.transform.position.z;
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);

            var radius = _draggingUnit == null ? 0.1f : 0.3f;
            var result = Physics2D.OverlapCircle(mouseWorldPos, radius, LayerMask.GetMask("Cell"));
            if (result)
            {
                var currentCell = result.GetComponent<UnitPlaceCell>();

                if (_hoveredCell && _hoveredCell != currentCell)
                {
                    _hoveredCell.OnHoverExit();
                }

                _hoveredCell = currentCell;
                _hoveredCell.OnHoverEnter();
            }
            else
            {
                if (_hoveredCell)
                {
                    _hoveredCell.OnHoverExit();
                }

                _hoveredCell = null;
            }
        }

        public void SpawnUnit(string unitId)
        {
            var data = _unitDataMap[unitId];
            var instance = Instantiate(data.prefab);
            instance.Setup(data);

            var cell = PickSpawnCell(data);
            instance.MoveTo(cell.transform.position);
            cell.PushUnit(instance);
        }

        private UnitPlaceCell PickSpawnCell(UnitDataTableRow data)
        {
            UnitPlaceGrid grid = null;
            switch (data.group)
            {
                case UnitGroup.Ally:
                    grid = allyPlaceGrid;
                    break;
                case UnitGroup.Enemy:
                    grid = enemyPlaceGrid;
                    break;
            }

            return grid.GetRandomEmptyCell(data.attackType);
        }
    }
}