using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using DG.Tweening;
using KCoreKit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceBound
{
    public class UnitMergeDirector : DirectorBase
    {
        private UnitDirector _unitDirector;
        private ShopDirector _shopDirector;
        private SoundDirector _soundDirector;
        private MasteryManager _masteryManager;
        private UnitPlaceDirector _unitPlaceDirector;

        private List<UnitMergeDataTableRow> _mergeDataList;
        [SerializeField] private List<UnitMergeSlot> mergeSlots;
        [SerializeField] private UnitMergeSlot resultSlot;

        [SerializeField] private ButtonWidget mergeModeToggleButton;
        [SerializeField] private ButtonWidget mergeButton;

        [SerializeField] private SpriteRenderGroup mergeGroup;
        [SerializeField] private SpriteRenderer preview;

        private bool _isMergeMode;
        private UnitCore _hoveredUnit;
        private UnitDataTableRow _previewUnitData;

        public void OnPlaced(UnitCore unit)
        {
            var units = mergeSlots
                .Select(x => x.GetUnit())
                .Where(u => u != null)
                .ToList();

            foreach (var data in _mergeDataList)
            {
                if (units.Find(x => x.GetId() == data.inputUnit1) && units.Find(x => x.GetId() == data.inputUnit2))
                {
                    _previewUnitData = _unitDirector.GetUnitData(data.outputUnit);
                    preview.sprite = _previewUnitData.texture.ToSprite(3);
                    preview.DOFade(0.5f, 0.3f);
                    return;
                }
            }
        }

        public override IEnumerator OnInitialize()
        {
            foreach (var slot in mergeSlots)
            {
                slot.onPlaceAction += OnPlaced;
            }
            
            mergeModeToggleButton.onClickAction += OnMergeModeToggleButtonClick;
            mergeButton.onClickAction += OnMergeButtonClick;
            _mergeDataList = DataTableManager.FindAllRows<UnitMergeDataTableRow>();
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            _shopDirector = DirectorFacade.GetDirector<ShopDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _masteryManager = MasteryManager.GetInstance();
            yield return null;
        }

        private void OnMergeButtonClick()
        {
            var units = mergeSlots
                .Select(x => x.GetUnit())
                .Where(u => u != null)
                .ToList();
            StartCoroutine(MergeUnit(units));
        }

        private void OnMergeModeToggleButtonClick()
        {
            _isMergeMode = !_isMergeMode;
            if (_isMergeMode)
            {
                StartCoroutine(OnEnterMergeMode());
            }
            else
            {
                StartCoroutine(OnExitMergeMode());
            }
        }

        private IEnumerator OnExitMergeMode()
        {
            var units = mergeSlots
                .Select(x => x.GetUnit())
                .Where(u => u != null)
                .ToList();

            foreach (var unit in units)
            {
                _unitPlaceDirector.RemoveUnit(unit);
                _unitPlaceDirector.PlaceUnit(unit, _unitPlaceDirector.GetRandomEmptyCell(UnitGroup.Ally, unit.attackType) );
            }

            var resultUnit = resultSlot.GetUnit();

            if (resultUnit)
            {
                _unitPlaceDirector.RemoveUnit(resultUnit);
                _unitPlaceDirector.PlaceUnit(resultUnit, _unitPlaceDirector.GetRandomEmptyCell(UnitGroup.Ally, resultUnit.attackType) );

            }
            

            mergeGroup.Fade(0, 0.3f);
            yield return new WaitForSeconds(0.3f);
            mergeButton.Hide();
            preview.gameObject.SetActive(false);
            mergeGroup.gameObject.SetActive(false);
        }

        private IEnumerator OnEnterMergeMode()
        {
            mergeButton.Show();
            preview.gameObject.SetActive(true);
            mergeGroup.gameObject.SetActive(true);
            mergeGroup.Fade(1, 0.3f);
            yield return null;
        }


        private IEnumerator MergeUnit(List<UnitCore> inputs)
        {
            if (_previewUnitData)
            {
                BroAudio.Play(_soundDirector.mergeUnitSFX);
                int count = 0;
                foreach (var unit in inputs)
                {
                    StartCoroutine(unit.ShowUpgradeEffect());
                    unit.Move(preview.transform.position).OnComplete(() =>
                    {
                        preview.transform.DOScale(0.2f, 0.05f).SetRelative().SetEase(Ease.Linear)
                            .SetLoops(2, LoopType.Yoyo);
                        count++;
                    });
                }

                yield return new WaitUntil(() => count == inputs.Count);
                var resultUnit = _unitDirector.SpawnUnit(_previewUnitData.id,  UnitGroup.Ally,false);
                _unitPlaceDirector.PlaceUnit(resultUnit, resultSlot);
                preview.DOFade(0, 0.3f);
                yield return resultUnit.ShowUpgradeEffect();
                //var cell = _unitPlaceDirector.GetRandomEmptyCell(UnitGroup.Ally, resultUnit.attackType);
                //_unitPlaceDirector.RemoveUnit(resultUnit);
                //yield return resultUnit.transform.DOJump(cell.transform.position, 10, 1, 0.3f).WaitForCompletion();
                //_unitPlaceDirector.PlaceUnit(resultUnit, cell);

                yield return new WaitForSeconds(0.5f);
                foreach (var unit in inputs)
                {
                    StartCoroutine(unit.HideUpgradeEffect());
                    _unitDirector.RemoveAllyUnit(unit);
                }

                _previewUnitData = null;
                yield return resultUnit.HideUpgradeEffect();

                yield break;
            }
        }
    }
}