using System;
using UnityEngine;

namespace YatchDungeon
{
    public class UnitPlaceGrid : MonoBehaviour
    {
        [SerializeField]
        private int columnCount;
        
        [SerializeField]
        private Vector2 spacing;
        
        [SerializeField]
        private UnitPlaceCell[] cells;
        
        public void OnValidate()
        {
            int count = 0;
            while (cells.Length > count)
            {
                cells[count].transform.position = new Vector3((int)(count / columnCount)*spacing.x, 0, (count % columnCount)*spacing.y);
                count++;
            }
        }
    }
}