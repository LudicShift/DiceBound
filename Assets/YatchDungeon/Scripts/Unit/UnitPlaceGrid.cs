using System;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;

namespace YatchDungeon
{
    public class UnitPlaceGrid : MonoBehaviour
    {
        [SerializeField] private int columnCount;

        [SerializeField] private Vector2 spacing;

        private List<UnitPlaceCell> _cells;
        [SerializeField] private bool reverseIndexing;

        public void OnValidate()
        {
            _cells = GetComponentsInChildren<UnitPlaceCell>().ToList();
            if (_cells.Count == 0)
            {
                return;
            }

            int count = 0;
            while (_cells.Count > count)
            {
                _cells[count].transform.localPosition = new Vector3((int)(count / columnCount) * spacing.x,
                    (count % columnCount) * spacing.y);
                if (reverseIndexing)
                {
                    _cells[count].Setup((_cells.Count/columnCount)-(count / columnCount)-1);
                }
                else
                {
                    _cells[count].Setup((count / columnCount));
                }

                count++;
            }
        }

        public UnitPlaceCell GetRandomEmptyCell()
        {
            var emptyCells = _cells.FindAll(x=>x.IsEmpty());
            return emptyCells.GetRandomElement();
        }
    }
}