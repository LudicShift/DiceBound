using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using DG.Tweening;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class DiceDirector : DirectorBase
    {
        [BigHeader("General")] [SerializeField]
        private Canvas canvas;

        [SerializeField] private Transform initialDicePoint;
        [SerializeField] private RectTransform remainPointGroup;
        [SerializeField] private RectTransform keepPointGroup;

        [BigHeader("Widget")] [SerializeField] private DiceRerollButtonWidget rerollButtonWidget;
        [SerializeField] private TextWidget combinationInfoWidget;

        [BigHeader("Sprite")] [SerializeField] private List<Sprite> diceSprite;
        [SerializeField] private List<Sprite> diceAnimationSprite;
        [SerializeField] private Sprite emptySprite;


        private int _remainRollCount = 3;
        private int _maxDiceFace = 6;
        private UnitDirector _unitDirector;
        private WalletDirector _walletDirector;
        private Dictionary<string, CombinationBase> _combinations;
        private List<DicePointWidget> _keepPoints;
        private List<DicePointWidget> _remainPoints;
        private List<DiceWidget> _keepDices;
        private List<DiceWidget> _remainDices;
        private string _noCombinationText;
        private SoundDirector _soundDirector;
        private UnitPlaceDirector _unitPlaceDirector;
        private MasteryManager _masteryManager;
        private List<CombinationBase> _currentResult;

        public void ShowCanvas()
        {
            _unitPlaceDirector.SetEnable(false);
            canvas.gameObject.SetActive(true);
        }

        public void HideCanvas()
        {
            _unitPlaceDirector.SetEnable(true);
            canvas.gameObject.SetActive(false);
        }


        public void Setup()
        {
            rerollButtonWidget.Show();
            _remainRollCount = 3 + (int)_masteryManager.GetModifierTotal("DiceRerollCountIncrease");
            StartCoroutine(ResetDice());
        }

        public void Roll()
        {
            if (_remainDices.Count > 0)
            {
                StartCoroutine(RollRoutine());
            }
        }

        private IEnumerator RollRoutine()
        {
            BroAudio.Play(_soundDirector.diceRollSFX);
            combinationInfoWidget.SetText("Rolling...");

            _remainRollCount--;
            rerollButtonWidget.Disable();
            rerollButtonWidget.SetCount(_remainRollCount);

            int count = 0;
            foreach (var dice in _remainDices)
            {
                StartCoroutine(dice.Roll(() => count++, _maxDiceFace));
            }

            yield return new WaitUntil(() => count >= _remainDices.Count);


            if (_remainRollCount <= 0)
            {
                rerollButtonWidget.Disable();
            }
            else
            {
                rerollButtonWidget.Enable();
            }
            var allDices = _remainDices.Concat(_keepDices).ToList();
            var combinationContext = new CombinationContext(allDices);
            _currentResult = Evaluate(combinationContext);
            ShowResult();
        }
        
        public void ClearDices()
        {
            _currentResult = null;
            rerollButtonWidget.Hide();
            
            if (_keepDices.Count > 0)
            {
                foreach (var dice in _keepDices)
                {
                    _remainDices.Add(dice);
                }
            }
            combinationInfoWidget.SetText("");
            for (int i = 0; i < _remainDices.Count; i++)
            {
                _remainPoints[i].SetDice(_remainDices[i]);
                _remainDices[i].Warp(initialDicePoint.position);
            }
        }

        private IEnumerator ResetDice()
        {
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
            _noCombinationText = "No Combination"; //추후 현지화

            _keepDices = new List<DiceWidget>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();

            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            _masteryManager = MasteryManager.GetInstance();
            _maxDiceFace = 6 - (int)_masteryManager.GetModifierTotal("DiceFaceRemoval");
            _remainDices = canvas.GetComponentsInChildren<DiceWidget>(true).ToList();
            _remainPoints = remainPointGroup.GetComponentsInChildren<DicePointWidget>(true).ToList();
            _keepPoints = keepPointGroup.GetComponentsInChildren<DicePointWidget>(true).ToList();

            foreach (var dice in _remainDices)
            {
                dice.onPointerClickAction += _ => OnDiceClick(dice);
                dice.spriteGetter = GetDiceSprite;
                dice.animationSpriteGetter = GetDiceAnimationSprite;
            }

            rerollButtonWidget.onClickAction += Roll;

            var combinationDataList = DataTableManager.FindAllRows<CombinationDataTableRow>();
            var dictionary = combinationDataList.ToDictionary(x => x.id);


            _combinations = new Dictionary<string, CombinationBase>();
            _combinations.Add("ONE_PAIR", new OnePairCombination(dictionary["ONE_PAIR"]));
            _combinations.Add("TWO_PAIR", new TwoPairCombination(dictionary["TWO_PAIR"]));
            _combinations.Add("TRIPLE", new TripleCombination(dictionary["TRIPLE"]));
            _combinations.Add("S_STRAIGHT", new SmallStraightCombination(dictionary["S_STRAIGHT"]));
            _combinations.Add("L_STRAIGHT", new LargeStraightCombination(dictionary["L_STRAIGHT"]));
            _combinations.Add("FULL_HOUSE", new FullHouseCombination(dictionary["FULL_HOUSE"]));
            _combinations.Add("FOUR_KIND", new FourOfKindCombination(dictionary["FOUR_KIND"]));
            _combinations.Add("FIVE_KIND", new FiveKindCombination(dictionary["FIVE_KIND"]));
            yield return null;
        }


        private Sprite GetDiceAnimationSprite(int index)
        {
            return diceAnimationSprite[index % 6];
        }

        private Sprite GetDiceSprite(int number)
        {
            return diceSprite[number - 1];
        }

        private void OnDiceClick(DiceWidget dice)
        {
            dice.OnClick();
            BroAudio.Play(_soundDirector.diceClickSFX);
            if (dice.IsMoving())
            {
                return;
            }

            if (_remainDices.Contains(dice))
            {
                KeepDice(dice);
            }
            else if (_keepDices.Contains(dice))
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
        
            if (_currentResult != null)
            {
                combinationInfoWidget.SetText(_currentResult[0].GetName());
            }
            else
            {
                combinationInfoWidget.SetText(_noCombinationText);
            }
        }


        public List<CombinationBase> Evaluate(CombinationContext context)
        {
            List<CombinationBase> result = new List<CombinationBase>();
            foreach (var combination in _combinations)
            {
                if (combination.Value.Evaluate(context))
                {
                    result.Add(combination.Value);
                }
            }

            result.Sort((a,b)=>a.GetPriority().CompareTo(b.GetPriority()));
            return result;
        }


        public CombinationBase GetCombination(string combinationID)
        {
            return _combinations[combinationID];
        }

        public List<CombinationBase> GetResult()
        {
            return _currentResult;
        }

        
    }
}