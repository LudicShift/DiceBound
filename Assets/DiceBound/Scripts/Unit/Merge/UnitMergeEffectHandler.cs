using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace DiceBound
{
    public class UnitMergeEffectHandler : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private Material mergeEffectMaterial;

        private Material _original;
        
        private float _alpha;

        public Ease fadeInEase;
        public Ease fadeOutEase;
        public void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _original = _spriteRenderer.material;
        }

        public IEnumerator FadeIn()
        {
            _spriteRenderer.material = mergeEffectMaterial;
            yield return DOTween.To(x => _alpha = x, 0, 1, 0.1f).SetEase(fadeInEase).WaitForCompletion();
        }
        
        public IEnumerator FadeOut()
        {
          yield return  DOTween.To(x => _alpha = x, 1, 0, 1f).SetEase(fadeOutEase).WaitForCompletion();
          _spriteRenderer.material = _original;
        }

        public void Update()
        {
            _spriteRenderer.material.SetFloat("_Alpha", _alpha);
        }
    }
}