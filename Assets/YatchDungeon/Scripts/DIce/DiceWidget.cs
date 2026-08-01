using System;
using System.Collections;
using DG.Tweening;
using KCoreKit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace YatchDungeon
{
    public class DiceWidget : ImageWidget
    {
     
        private bool _isRolling;
        private int _number;

        public Func<int, Sprite> spriteGetter;
        public Func<int, Sprite> animationSpriteGetter;
        private bool _isMoving;
        private int _animationIndex;
        
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private int rollTime = 2;
        [SerializeField] private float rollDuration = 0.3f;

        public IEnumerator Roll(Action finishCallback)
        {
            _isRolling = true;
            //_animator.SetInteger("Index",Random.Range(1,3));
            //yield return new WaitUntil(() => !_isRolling);
            var rollStartIndex = Random.Range(0, 5);
            var rollEndIndex = rollStartIndex + 6*rollTime;
            _animationIndex = rollStartIndex;
            yield return rectTransform.DOAnchorPosY(50f, 0.1f).SetRelative(true).WaitForCompletion();
            yield return DOTween.To(GetRollIndex, SetRollIndex, rollEndIndex,rollDuration).WaitForCompletion();
            yield return rectTransform.DOAnchorPosY(-50f, 0.1f).SetRelative(true).WaitForCompletion();
            _number = Random.Range(1, 6);
            SetSprite(spriteGetter.Invoke(_number));
            finishCallback?.Invoke();
        }

        private void SetRollIndex(int value)
        {
            _animationIndex = value;
            SetSprite(animationSpriteGetter.Invoke(_animationIndex));
        }

        private int GetRollIndex()
        {
            return _animationIndex;
        }

        public bool IsMoving()
        {
            return _isMoving;
        }

        public int GetNumber()
        {
            return _number;
        }

        public void MoveTo(Vector3 position)
        {
            _isMoving = true;
             transform.DOMove(position, moveDuration).OnComplete(() => { _isMoving = false; });
        }

        public void Warp(Vector3 position)
        {
            transform.position = position;
        }
    }
}