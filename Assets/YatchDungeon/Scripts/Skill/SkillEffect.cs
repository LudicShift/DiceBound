using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace YatchDungeon
{
    public enum SkillMoveOption
    {
        Warp,
        Move,
    }

    public class SkillEffect : MonoBehaviour
    {
        private float _damage;

        [SerializeField] private float lifetime;

        [SerializeField] private float moveDuration;

        [SerializeField] private SkillMoveOption moveOption;

        public void Awake()
        {
            Destroy(gameObject, lifetime);
        }

        public void SetDamage(float damage)
        {
            _damage = damage;
        }

        public void SetDirection(bool flip)
        {
            if (flip)
            {
                transform.eulerAngles = new Vector3(0, 180f, 0);
            }
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }


        public void Execute(UnitCore target)
        {
            switch (moveOption)
            {
                case SkillMoveOption.Warp:
                    transform.position = target.transform.position;
                    target.OnDamage(_damage);
                    break;
                case SkillMoveOption.Move:
                    transform.DOMove(target.transform.position, moveDuration).OnComplete(() =>
                    {
                        target.OnDamage(_damage);
                    });
                    break;
            }
        }
    }
}