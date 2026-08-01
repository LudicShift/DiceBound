using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace YatchDungeon
{
    public enum UnitTargetOption
    {
        Ally,
        Enemy,
        Self
    }

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

        private List<UnitCore> _units = new List<UnitCore>();
        private List<UnitCore> _allies = new List<UnitCore>();
        private List<UnitCore> _enemies = new List<UnitCore>();

        private SkillDirector _skillDirector;

        public override IEnumerator OnInitialize()
        {
            _skillDirector = DirectorFacade.GetSubMode<SkillDirector>();
            _camera = CameraManager.GetMainCamera();
            _unitDataMap = DataTableManager.FindAllRows<UnitDataTableRow>().ToDictionary(x => x.id);

            InputManager.RegisterAction("Click", PlayerActionType.Started, OnMouseDownUnit);
            InputManager.RegisterAction("Click", PlayerActionType.Canceled, OnMouseUpUnit);
            yield return null;
        }

        private void OnMouseUpUnit(InputAction.CallbackContext obj)
        {
            if (_draggingUnit == null) return;

            if (_hoveredCell != null)
            {
                // 1. 호버된 셀이 비어있는 경우: 해당 셀에 배치
                if (_hoveredCell.IsEmpty())
                {
                    _draggingUnit.Warp(_hoveredCell.transform.position);
                    _hoveredCell.PushUnit(_draggingUnit);
                }
                // 2. 호버된 셀에 이미 다른 유닛이 있는 경우 (자리 교체 혹은 원래 자리 복귀)
                else
                {
                    // 여기서는 안전하게 원래 자리로 되돌리거나, 필요에 따라 스왑 로직 구현 가능
                    // 우선 원래 자리로 돌리는 안전한 처리:
                    _draggingUnit.MoveTo(_restoreCell.transform.position);
                    _restoreCell.PushUnit(_draggingUnit);
                }
            }
            else
            {
                // 3. 셀이 아닌 곳에 드롭한 경우: 원래 자리로 복귀
                _draggingUnit.MoveTo(_restoreCell.transform.position);
                _restoreCell.PushUnit(_draggingUnit);
            }

            // 상태 초기화
            _restoreCell = null;
            _draggingUnit = null;
            _dragOffset = Vector3.zero;
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
                _hoveredCell.OnHoverEnter(_draggingUnit != null);
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
            if (data.skillBasicKey != "")
            {
                instance.BindSkill(_skillDirector.GetSkill(data.skillBasicKey));
            }

            if (data.skillActiveKey != "")
            {
                instance.BindSkill(_skillDirector.GetSkill(data.skillActiveKey));
            }

            if (data.skillPassiveKey != "")
            {
                instance.BindSkill(_skillDirector.GetSkill(data.skillPassiveKey));
            }

            instance.onDeadAction += OnUnitDead;

            var cell = PickSpawnCell(data);
            instance.MoveTo(cell.transform.position);
            cell.PushUnit(instance);
            _units.Add(instance);
            switch (data.group)
            {
                case UnitGroup.Ally:
                    _allies.Add(instance);
                    break;
                case UnitGroup.Enemy:
                    _enemies.Add(instance);
                    break; 
            }
        }

        private void OnUnitDead(UnitCore unit)
        {
            _units.Remove(unit);
            _allies.Remove(unit);
            _enemies.Remove(unit);
            Destroy(unit.gameObject);
        }

        public int GetEnemyUnitCount()
        {
            return enemyPlaceGrid.GetUnitCount();
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

        public List<UnitCore> GetAllUnit()
        {
            return _units;
        }

        public List<UnitCore> GetTarget(UnitCore self, UnitTargetOption target, int count)
        {
            switch (target)
            {
                case UnitTargetOption.Ally:
                    return GetRandomAllies(count);
                case UnitTargetOption.Enemy:
                    return GetRandomEnemies(count);
                case UnitTargetOption.Self:
                    return new List<UnitCore> { self };
            }

            return null;
        }

        private List<UnitCore> GetRandomEnemies(int count)
        {
            return _enemies.GetRandomElements(count);
        }

        private List<UnitCore> GetRandomAllies(int count)
        {
            return _allies.GetRandomElements(count);
        }
    }
}