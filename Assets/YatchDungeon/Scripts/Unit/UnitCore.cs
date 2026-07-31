using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YatchDungeon
{
    public class UnitCore : MonoBehaviour
    { 
        
        private UnitDataTableRow _data;

        public void Setup(UnitDataTableRow data)
        {
            _data = data;
        }
        
        public UnitDataTableRow GetData()
        {
            return _data;
        }

 
    }
}