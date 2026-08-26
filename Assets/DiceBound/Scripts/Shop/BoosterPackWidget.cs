using System;
using System.Collections;
using KCoreKit;
using UnityEngine;

namespace DiceBound.Shop
{
    public class BoosterPackWidget : ImageWidget
    {
        [SerializeField] private TweenAnimationPlayer openTween;
        private Vector3 _initialLocalPosition;

        public void Awake()
        {
            _initialLocalPosition = transform.localPosition;
        }

        public void Setup(BoosterPackDataTableRow data)
        {
            SetSprite(data.texture.ToSprite());
        }

        public IEnumerator PlayOpenTween()
        {
            yield return openTween?.Play();
        }

        public void Rewind()
        {
            transform.localPosition = _initialLocalPosition;
        }
    }
}