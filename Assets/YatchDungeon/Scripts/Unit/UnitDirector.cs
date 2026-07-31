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
        private UnitCore _hoveredUnit;
        private UnitCore _draggingUnit;
        private Vector3 _dragOffset;
        private Camera _camera;

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
                _draggingUnit = null;
            }
        }

        private void OnMouseDownUnit(InputAction.CallbackContext obj)
        {
            // 클릭 순간 호버링 중인 유닛이 있다면 드래그 시작
            if (_hoveredUnit)
            {
                _draggingUnit = _hoveredUnit;

                // 1. 마우스 스크린 좌표 -> 월드 좌표 변환
                Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
                mouseScreenPos.z = -_camera.transform.position.z; // 카메라 거리 맞추기
                Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);

                // 2. 유닛과 마우스 위치의 오프셋 저장 (유닛이 마우스 포인터 기준 갑자기 확 튀는 현상 방지)
                _dragOffset = _draggingUnit.transform.position - mouseWorldPos;
                _dragOffset.z = 0; // 2전용이므로 Z축 고정
            }
        }

        public void SpawnAllyUnit(string unitId)
        {
            var data = _unitDataMap[unitId];
            var instance = Instantiate(data.prefab);
            instance.Setup(data);
        }

        public void Update()
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
            else
            {
                CheckHoverUnit();
            }
        }

        private void CheckHoverUnit()
        {
            // 마우스 스크린 좌표를 월드 좌표로 변환 후 OverlapCircle 수행
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            mouseScreenPos.z = -_camera.transform.position.z;
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);

            var result = Physics2D.OverlapCircle(mouseWorldPos, 0.1f, LayerMask.GetMask("Unit"));
            if (result)
            {
                _hoveredUnit = result.GetComponent<UnitCore>();
            }
            else
            {
                _hoveredUnit = null;
            }
        }
    }
}