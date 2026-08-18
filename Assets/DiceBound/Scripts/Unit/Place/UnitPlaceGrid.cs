using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class UnitPlaceGrid : MonoBehaviour
    {
        [SerializeField] private int columnCount;

        [SerializeField] private Vector2 spacing;
        [SerializeField] private bool flip;

        private List<UnitPlaceCell> _cells;
        [SerializeField] private bool reverseIndexing;

        public void Awake()
        {
            _cells = GetComponentsInChildren<UnitPlaceCell>().ToList();
        }

        public void Show()
        {
            foreach (var cell in _cells)
            {
                cell.Show();
            }
        }

        public void Hide()
        {
            foreach (var cell in _cells)
            {
                cell.Hide();
            }
        }

        public void OnValidate()
        {
            _cells = GetComponentsInChildren<UnitPlaceCell>().ToList();
            if (_cells.Count == 0)
            {
                return;
            }

            int count = 0;
            int rowCount = _cells.Count / columnCount;
            while (_cells.Count > count)
            {
                int currentRow = count / columnCount;
                int currentColumn = count % columnCount;

                // 짝수번째 칸(2, 4, ...)을 반 칸 밀어 지그재그로 배치한다.
              
                float zigzagOffset = currentColumn % 2 == 1 ? spacing.x * 0.5f : 0f;
                if (flip)
                {
                    zigzagOffset *= -1;
                }
                _cells[count].transform.localPosition =
                    new Vector3(currentRow * spacing.x + zigzagOffset, currentColumn * spacing.y);

                if (!reverseIndexing)
                {
                    _cells[count].Setup(Mathf.Lerp(0, 1, (rowCount - currentRow - 1) / (float)(rowCount - 1)));
                }
                else
                {
                    _cells[count].Setup(Mathf.Lerp(0, 1, currentRow / (float)(rowCount - 1)));
                }

                count++;
            }
        }

        public UnitPlaceCell GetRandomEmptyCell(UnitAttackType attackType)
        {
            switch (attackType)
            {
                case UnitAttackType.Melee:
                    var nearCells = _cells.FindAll(x => x.IsEmpty() && x.distance <= 0.5f);
                    if (nearCells.Count > 0)
                    {
                        return nearCells.GetRandomElement();
                    }

                    break;
                case UnitAttackType.Ranged:
                case UnitAttackType.Magic:
                    var farCells = _cells.FindAll(x => x.IsEmpty() && x.distance > 0.5f);
                    if (farCells.Count > 0)
                    {
                        return farCells.GetRandomElement();
                    }

                    break;
            }

            var emptyCells = _cells.FindAll(x => x.IsEmpty());
            return emptyCells.GetRandomElement();
        }

        public int GetUnitCount()
        {
            return _cells.Count(x => !x.IsEmpty());
        }

        public UnitPlaceCell FindCellByUnit(UnitCore unit)
        {
            return _cells.Find(x => x.GetUnit() == unit);
        }

        public void RemoveUnit(UnitCore unit)
        {
            var cell = _cells.Find(x => x.GetUnit() == unit);
            cell?.RemoveUnit();
        }

        public List<UnitCore> GetGeneralTargets(int count)
        {
            var result = new List<UnitCore>();
            var unitCells = _cells.FindAll(x => !x.IsEmpty());

            for (int i = 0; i < count; i++)
            {
                if (unitCells.Count == 0)
                {
                    break;
                }
                
                var weightSum = unitCells.Sum(x => 2-x.distance);
                float current = 0;
                var randomValue = Random.Range(0, weightSum);
                foreach (var cell in unitCells)
                {
                    current += 2-cell.distance;
                    if (randomValue < current)
                    {
                        result.Add(cell.GetUnit());
                        unitCells.Remove(cell);
                        break;
                    }
                }
            }

            return result;

        }
    }
}