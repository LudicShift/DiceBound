using System;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class UnitInputHandler :  MonoBehaviour
    {
        [SerializeField] private TweenAnimationPlayer pickSequence;
        [SerializeField] private TweenAnimationPlayer dropSequence;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void OnPick()
        {
            StartCoroutine(pickSequence.Play(0, () =>
            {
                _spriteRenderer.transform.localScale = Vector3.one;
            }));
        } 
        
        public void OnDrop()
        {
            StartCoroutine(dropSequence.Play(0.05f, () =>
            {
                _spriteRenderer.transform.localScale = Vector3.one;
            }));
        }
    }
}