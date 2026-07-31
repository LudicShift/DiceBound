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
        private Animator _animator;
        private bool _isRolling;
        private int _number;

        public Func<int, Sprite> spriteGetter; 
        
        public override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
        }

        public IEnumerator Roll(Action finishCallback)
        {
            _isRolling = true;
            // _animator.SetInteger("Index",Random.Range(1,3));
            //yield return new WaitUntil(() => !_isRolling);
            yield return null;
            _number = Random.Range(1, 6);
            finishCallback?.Invoke();
            SetSprite(spriteGetter.Invoke(_number));
        }

        public void RollEnd()
        {
            _isRolling = false;
         
        }

        public int GetNumber()
        {
            return _number;
        }

        public void MoveTo(Vector3 position)
        {
            transform.DOMove(position,1);
        }

        public void Warp(Vector3 position)
        {
            transform.position = position;
        }
    }
}