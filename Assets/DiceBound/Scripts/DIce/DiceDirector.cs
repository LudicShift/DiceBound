using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class DiceDirector : DirectorBase
    {
        [SerializeField] private ButtonWidget _claimButtonWidget;
        [SerializeField] private DiceRerollButtonWidget _rerollButtonWidget;

        [SerializeField] private ButtonWidget _showFieldButton;
        
        [SerializeField] private TextWidget _combinationInfoWidget;
        
        [SerializeField] private Transform initialDicePoint;
        
        [SerializeField]
        private Canvas canvas;    
        
        [SerializeField] private ImageWidget layer;

        [SerializeField] private List<Sprite> diceSprite;
        [SerializeField] private List<Sprite> diceAnimationSprite;
        [SerializeField] private Sprite emptySprite;
        
        
        [SerializeField] private RectTransform remainPointGroup;
        [SerializeField] private RectTransform keepPointGroup;
        
        private int _remainRollCount = 3;
        private UnitDirector _unitDirector;
        private CombinationBase[] _combinations;
        private List<DicePointWidget> _keepPoints;
        private List<DicePointWidget> _remainPoints;
        private List<DiceWidget> _keepDices;
        private List<DiceWidget> _remainDices;
        private string _noCombinationText;

        public void ShowCanvas()
        {
            canvas.gameObject.SetActive(true);
        }

        public void HideCanvas()
        {
            canvas.gameObject.SetActive(false);
        }
        
        public void ShowLayer()
        {
            layer.gameObject.SetActive(true);
        }
        
        public void HideLayer()
        {
            layer.gameObject.SetActive(false);
        }
        
        public void Setup()
        {
            _remainRollCount = 3;
            StartCoroutine(ResetDice());
           
        }
        
        public void Roll()
        {
            StartCoroutine(RollRoutine());
        }

        private IEnumerator RollRoutine()
        {
            _combinationInfoWidget.SetText("Rolling...");
           
            _remainRollCount--;
            _claimButtonWidget.SetInteractable(false);
            _rerollButtonWidget.Disable();
            _rerollButtonWidget.SetCount(_remainRollCount);
            
            int count = 0;
            foreach (var dice in _remainDices)
            {
                StartCoroutine(dice.Roll(()=>count++));
            }
            yield return new WaitUntil(()=>count >= _remainDices.Count);
            
            
            if (_remainRollCount <= 0)
            {
                _rerollButtonWidget.Disable();
            }
            else
            {
                _rerollButtonWidget.Enable();
            }
            ShowResult();
            _claimButtonWidget.SetInteractable(true);
        }
        
        

        private IEnumerator ResetDice()
        {
            _claimButtonWidget.SetInteractable(false);
            if (_keepDices.Count > 0)
            {
                foreach (var dice in _keepDices)
                {
                    _remainDices.Add(dice);
                }
            }
            
            _combinationInfoWidget.SetText("");

            for (int i = 0; i < _remainDices.Count; i++)
            {
                _remainPoints[i].SetDice(_remainDices[i]);
                _remainDices[i].Warp(initialDicePoint.position);
                _remainDices[i].SetSprite(emptySprite);
                _remainDices[i].MoveTo(_remainPoints[i].transform.position);
            }

            yield return new WaitForSeconds(0.5f);
            _keepDices.Clear();
            foreach (var point in _keepPoints)
            {
                point.SetDice(null);
            }
            
            Roll();
        }

        public override IEnumerator OnInitialize()
        {
            _noCombinationText = "No Combination";//추후 현지화
            
            _keepDices = new List<DiceWidget>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _remainDices = canvas.GetComponentsInChildren<DiceWidget>(true).ToList();
            _remainPoints = remainPointGroup.GetComponentsInChildren<DicePointWidget>(true).ToList();
            _keepPoints = keepPointGroup.GetComponentsInChildren<DicePointWidget>(true).ToList();
            
            foreach (var dice in _remainDices)
            {
                dice.onPointerClickAction += _ => OnDiceClick(dice);
                dice.spriteGetter = GetDiceSprite;
                dice.animationSpriteGetter = GetDiceAnimationSprite;
            }
            
            _showFieldButton.onClickAction+=OnShowFieldButtonClick;
            _rerollButtonWidget.onClickAction+=Roll;
            _claimButtonWidget.onClickAction+=OnClaimButtonClick;

            var combinationDataList = DataTableManager.FindAllRows<CombinationDataTableRow>();
            var dictionary =  combinationDataList.ToDictionary(x => x.id);
            
            
            var combinationList = new List<CombinationBase>();
            combinationList.Add(new OnePairCombination(dictionary["ONE_PAIR"]));
            combinationList.Add(new TwoPairCombination(dictionary["TWO_PAIR"]));
            combinationList.Add(new TripleCombination(dictionary["TRIPLE"]));
            combinationList.Add(new SmallStraightCombination(dictionary["S_STRAIGHT"]));
            combinationList.Add(new LargeStraightCombination(dictionary["L_STRAIGHT"]));
            combinationList.Add(new FullHouseCombination(dictionary["FULL_HOUSE"]));
            combinationList.Add(new FourOfKindCombination(dictionary["FOUR_KIND"]));
            combinationList.Add(new FiveKindCombination(dictionary["FIVE_KIND"]));
            combinationList.Sort((a,b)=>a.GetPriority() - b.GetPriority());
            _combinations = combinationList.ToArray();
            
            yield return null;
        }

        private Sprite GetDiceAnimationSprite(int index)
        {
            return diceAnimationSprite[index % 6];
        }

        private Sprite GetDiceSprite(int number)
        {
            return diceSprite[number-1];
        }

        private void OnDiceClick(DiceWidget dice)
        {
            if (dice.IsMoving())
            {
                return;
            }
            
            if (_remainDices.Contains(dice))
            {
                KeepDice(dice);
            }
            else if(_keepDices.Contains(dice))
            {
                UnKeepDice(dice);
            }
        }

        private void KeepDice(DiceWidget dice)
        {
            _remainDices.Remove(dice);
            _keepDices.Add(dice);
            var keepPoint = GetEmptyDiceKeepPoint();
            keepPoint.SetDice(dice);
            dice.MoveTo(keepPoint.transform.position);
        }

        private DicePointWidget GetEmptyDiceKeepPoint()
        {
            foreach (var keepPoint in _keepPoints)
            {
                if (keepPoint.IsEmpty())
                {
                    return keepPoint;
                }
            }

            return null;
        }

        private void UnKeepDice(DiceWidget dice)
        {
            _keepDices.Remove(dice);
            _remainDices.Add(dice);
            
            var keepPoint = GetLinkedKeepPoint(dice);
            var point = GetLinkedRemainPoint(dice);
            keepPoint.SetDice(null);
            dice.MoveTo(point.transform.position);
        }

        private DicePointWidget GetLinkedKeepPoint(DiceWidget dice)
        {
            foreach (var keepPoint in _keepPoints)
            {
                if (keepPoint.IsLinked(dice))
                {
                    return keepPoint;
                }
            }

            return null;
        }

        private DicePointWidget GetLinkedRemainPoint(DiceWidget dice)
        {
            foreach (var point in _remainPoints)
            {
                if (point.IsLinked(dice))
                {
                    return point;
                }
            }

            return null;
        }

        private void ShowResult()
        {
            var allDices = _remainDices.Concat(_keepDices).ToList();
            var combinationContext = new CombinationContext(allDices);
            var combination = Evaluate(combinationContext);
            if (combination!=null)
            {
                _combinationInfoWidget.SetText(combination.GetName());
            }
            else
            {
                _combinationInfoWidget.SetText(_noCombinationText);
            }
        }
        
        private void OnClaimButtonClick()
        {
           
            var allDices = _remainDices.Concat(_keepDices).ToList();
            var combinationContext = new CombinationContext(allDices);
            var combination = Evaluate(combinationContext);
            if (combination != null)
            {
                _unitDirector.SpawnUnit(combination.GetUnitID());
                HideCanvas();
            }
        }

        private void OnShowFieldButtonClick()
        {
            if (layer.isShown)
            {
                HideLayer();
            }
            else
            {
                ShowLayer();
            }
        }


        public CombinationBase Evaluate(CombinationContext context)
        {
            foreach (var combination in _combinations)
            {
                if (combination.Evaluate(context))
                {
                    return combination;
                }
            }

            return null;
        }
        
        
    }
}
