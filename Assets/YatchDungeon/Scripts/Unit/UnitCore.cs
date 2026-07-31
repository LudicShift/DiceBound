using System;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YatchDungeon
{
    public class UnitCore : MonoBehaviour
    { 
        
        private UnitDataTableRow _data;
        [SerializeField] private float moveDuration = 0.3f;

        public void Setup(UnitDataTableRow data)
        {
            _data = data;
        }
        
        public UnitDataTableRow GetData()
        {
            return _data;
        }


        public void MoveTo(Vector3 position)
        {
            transform.DOMove(position, moveDuration);
        }

        public void Warp(Vector3 position)
        {
            transform.position = position;
        }
    }
}