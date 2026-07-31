using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YatchDungeon
{
    public class DiceDirector : DirectorBase
    {
        private CombinationBase[] _combinations;
        private List<DiceWidget> _keepDices;
        private List<DiceWidget> _remainDices;
      
        [SerializeField]
        private ButtonWidget _claimButtonWidget;
        [SerializeField]
        private ButtonWidget _rerollButtonWidget;

        [SerializeField] private ButtonWidget _showFieldButton;
        
        [SerializeField] private TextWidget _combinationInfoWidget;
        
        [SerializeField] private Transform initialDicePoint;
        
        [SerializeField]
        private Canvas canvas;    
        
        [SerializeField]
        private ImageWidget layer;
        private int _remainRollCount = 3;
        private List<DiceKeepPointWidget> _keepPoints;
        private List<DiceRemainPointWidget> _remainPoints;
        [SerializeField] private List<Sprite> diceSprite;
        [SerializeField] private List<Sprite> diceAnimationSprite;
        private UnitDirector _unitDirector;


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
            _rerollButtonWidget.SetInteractable(false);
            _claimButtonWidget.SetInteractable(false);
            _remainRollCount--;
            int count = 0;
            foreach (var dice in _remainDices)
            {
                StartCoroutine(dice.Roll(()=>count++));
            }
            yield return new WaitUntil(()=>count >= _remainDices.Count);
            _claimButtonWidget.SetInteractable(true);
            _rerollButtonWidget.SetInteractable(true);
            if (_remainRollCount <= 0)
            {
                _rerollButtonWidget.SetInteractable(false);
            }
            Debug.Log("Roll");
            ShowResult();
        }
        
        

        private IEnumerator ResetDice()
        {
            if (_keepDices.Count > 0)
            {
                foreach (var dice in _keepDices)
                {
                    _remainDices.Add(dice);
                }
            }

            for (int i = 0; i < _remainDices.Count; i++)
            {
                _remainPoints[i].SetDice(_remainDices[i]);
                _remainDices[i].Warp(initialDicePoint.position);
             _remainDices[i].MoveTo(_remainPoints[i].transform.position);
            }

            yield return new WaitForSeconds(1.0f);
            _keepDices.Clear();
            
            Roll();
        }

        public override IEnumerator OnInitialize()
        {
            _keepDices = new List<DiceWidget>();
            _unitDirector = DirectorFacade.GetSubMode<UnitDirector>();
            _remainDices = canvas.GetComponentsInChildren<DiceWidget>(true).ToList();
            _remainPoints = canvas.GetComponentsInChildren<DiceRemainPointWidget>(true).ToList();
            _keepPoints = canvas.GetComponentsInChildren<DiceKeepPointWidget>(true).ToList();
            
            foreach (var dice in _remainDices)
            {
                dice.onPointerClickCallback += _ => OnDiceClick(dice);
                dice.spriteGetter = GetDiceSprite;
                dice.animationSpriteGetter = GetDiceAnimationSprite;
            }
            
            _showFieldButton.AddOnClickAction(OnShowFieldButtonClick);
            _rerollButtonWidget.AddOnClickAction(Roll);
            _claimButtonWidget.AddOnClickAction(OnClaimButtonClick);

            var combinationDataList = DataTableManager.FindAllRows<CombinationDataTableRow>();
            var dictionary =  combinationDataList.ToDictionary(x => x.id);
            
            
            var combinationList = new List<CombinationBase>();
            combinationList.Add(new OnePairCombination(dictionary["ONE_PAIR"]));
            combinationList.Add(new TwoPairCombination(dictionary["TWO_PAIR"]));
            combinationList.Add(new TripleCombination(dictionary["TRIPLE"]));
            combinationList.Add(new SmallStraightCombination(dictionary["S_STRAIGHT"]));
            combinationList.Add(new LargeStraightCombination(dictionary["FULL_HOUSE"]));
            combinationList.Add(new FullHouseCombination(dictionary["FOUR_KIND"]));
            combinationList.Add(new FourOfKindCombination(dictionary["L_STRAIGHT"]));
            combinationList.Add(new FiveKindCombination(dictionary["FIVE_KIND"]));
            combinationList.Sort((a,b)=>a.GetPriority() - b.GetPriority());
            _combinations = combinationList.ToArray();
            
            yield return null;
        }

        private Sprite GetDiceAnimationSprite(int index)
        {
            return diceAnimationSprite[index % 6];
        }

        private Sprite GetDiceSprite(int index)
        {
            return diceSprite[index];
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

        private DiceKeepPointWidget GetEmptyDiceKeepPoint()
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

        private DiceKeepPointWidget GetLinkedKeepPoint(DiceWidget dice)
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

        private DiceRemainPointWidget GetLinkedRemainPoint(DiceWidget dice)
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
            //대충 유닛 스폰
            _combinationInfoWidget.SetText(combination.GetName());
        }
        
        private void OnClaimButtonClick()
        {
            var allDices = _remainDices.Concat(_keepDices).ToList();
            var combinationContext = new CombinationContext(allDices);
            var combination = Evaluate(combinationContext);
            _unitDirector.SpawnAllyUnit(combination.GetUnitID());
            HideCanvas();
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
