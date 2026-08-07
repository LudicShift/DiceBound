using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using KCoreKit.Scripts.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceBound
{
    public class UnitDirector : DirectorBase
    {
        private Dictionary<string, UnitDataTableRow> _unitDataDictionary;
        private UnitPlaceCell _hoveredCell;
        private UnitPlaceCell _restoreCell;
        private UnitCore _draggingUnit;

        private Vector3 _dragOffset;
        private Camera _camera;
        [SerializeField] private UnitPlaceGrid allyPlaceGrid;
        [SerializeField] private UnitPlaceGrid enemyPlaceGrid;

        [SerializeField] private Canvas hpCanvas;


        private PrefabPool<UnitCore> _allyPrefabPool;
        private PrefabPool<UnitCore> _enemyPrefabPool;
        private PrefabPool<GaugeWidget> _hpGaugePrefabPool;
        private List<UnitCore> _units = new List<UnitCore>();
        private List<UnitCore> _allies = new List<UnitCore>();
        private List<UnitCore> _enemies = new List<UnitCore>();
        private List<UnitCore> _deadAllies = new List<UnitCore>();

        private SkillDirector _skillDirector;
        private BattleDirector _battleDirector;

        public override IEnumerator OnInitialize()
        {
            _hpGaugePrefabPool =
                new PrefabPool<GaugeWidget>(PrefabManager.CachePrefab<GaugeWidget>(), hpCanvas.transform, 100);
            _allyPrefabPool =
                new PrefabPool<UnitCore>(PrefabManager.CachePrefab<UnitCore>("PF_Ally"), World.GetTransform(), 50);
            _enemyPrefabPool = new PrefabPool<UnitCore>(PrefabManager.CachePrefab<UnitCore>("PF_Enemy"),
                World.GetTransform(), 50);
            _allyPrefabPool.onGetAction += OnGetUnit;
            _enemyPrefabPool.onGetAction += OnGetUnit;
            _allyPrefabPool.onReleaseAction += OnReleaseUnit;
            _enemyPrefabPool.onReleaseAction += OnReleaseUnit;

            _skillDirector = DirectorFacade.GetDirector<SkillDirector>();
            _battleDirector = DirectorFacade.GetDirector<BattleDirector>();

            _camera = CameraManager.GetMainCamera();
            _unitDataDictionary = DataTableManager.FindAllRows<UnitDataTableRow>().ToDictionary(x => x.id);

            InputManager.RegisterAction("Click", PlayerActionType.Started, OnMouseDownUnit);
            InputManager.RegisterAction("Click", PlayerActionType.Canceled, OnMouseUpUnit);

            yield return null;
        }

        private void OnReleaseUnit(UnitCore instance)
        {
            instance.onDeadAction -= OnUnitDead;
            instance.onHitAction -= OnUnitHit;
            instance.onHealAction -= OnUnitHeal;
            var hp = instance.GetHpGauge();
            instance.ReleaseHpGauge(hp);
            _hpGaugePrefabPool.Release(hp);
        }

        private void OnGetUnit(UnitCore instance)
        {
            instance.onDeadAction += OnUnitDead;
            instance.onHitAction += OnUnitHit;
            instance.onHealAction += OnUnitHeal;
        }

        private void OnMouseUpUnit(InputAction.CallbackContext obj)
        {
            if (_draggingUnit == null) return;

            allyPlaceGrid.Hide();
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
                    var unit = _hoveredCell.PopUnit();
                    unit.MoveTo(_restoreCell.transform.position);
                    _restoreCell.PushUnit(unit);
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
                allyPlaceGrid.Show();
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
            if (_camera && !_battleDirector.IsPlaying())
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
            var data = _unitDataDictionary[unitId];
            UnitCore instance;
            if (data.group == UnitGroup.Ally)
            {
                instance = _allyPrefabPool.Get();
            }
            else
            {
                instance = _enemyPrefabPool.Get();
            }

            instance.Setup(data);
            instance.BindHpGauge(_hpGaugePrefabPool.Get());
            instance.Animate("Idle");

            instance.BindSkill(_skillDirector.GetSkill(data.skillBasicKey));
            instance.BindSkill(_skillDirector.GetSkill(data.skillActiveKey));
            instance.BindSkill(_skillDirector.GetSkill(data.skillPassiveKey));

            var cell = PickSpawnCell(data);
            instance.Warp(cell.transform.position);
            instance.PlayAppear();
            cell.PushUnit(instance);

            _units.Add(instance);
            switch (data.group)
            {
                case UnitGroup.Ally:
                    _allies.Add(instance);
                    instance.FlipSprite(false);
                    break;
                case UnitGroup.Enemy:
                    _enemies.Add(instance);
                    instance.FlipSprite(true);
                    break;
            }
        }

        private void OnUnitHeal(UnitCore core, int damage)
        {
            _battleDirector.ShowHeal(core, damage);
        }

        private void OnUnitHit(UnitCore core, int damage)
        {
            _battleDirector.ShowDamage(core, damage);
        }

        private void OnUnitDead(UnitCore unit)
        {
            switch (unit.group)
            {
                case UnitGroup.Ally:
                    if (!_deadAllies.Contains(unit))
                    {
                        _units.Remove(unit);
                        _allies.Remove(unit);
                        _deadAllies.Add(unit);
                    }
                    break;
                case UnitGroup.Enemy:
                    if (_units.Contains(unit))
                    {
                        _enemies.Remove(unit);
                        _units.Remove(unit);
                        enemyPlaceGrid.Remove(unit);
                        _enemyPrefabPool.Release(unit);
                    }

                    break;
            }
        }

        public int GetEnemyUnitCount()
        {
            return _enemies.Count;
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

        public List<UnitCore> GetTarget(UnitCore self, SkillTargetOption attackTarget, int count)
        {
            switch (attackTarget)
            {
                case SkillTargetOption.Ally:
                    return self.group == UnitGroup.Ally ? GetRandomAllies(count) : GetRandomEnemies(count);
                case SkillTargetOption.Enemy:
                    return self.group == UnitGroup.Ally ? GetRandomEnemies(count) : GetRandomAllies(count);
                case SkillTargetOption.Self:
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
            return _allies.Where(x => !x.IsDead()).ToList().GetRandomElements(count);
        }

        public int GetAllyUnitCount()
        {
            return _allies.Count;
        }

        public bool IsAlive(UnitCore target)
        {
            return !target.IsDead();
        }

        public int GetDeadAllyUnitCount()
        {
            return _deadAllies.Count;
        }

        public void ReviveDeadAllies()
        {
            foreach (var unit in _deadAllies)
            {
                unit.Revive();
            }

            _deadAllies.Clear();
        }
    }
}