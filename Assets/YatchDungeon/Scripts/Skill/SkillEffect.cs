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
        private ParticleSystemRenderer psRenderer;
        private bool _isHeal;

        public void Awake()
        {
            Destroy(gameObject, lifetime);
            psRenderer = GetComponentInChildren<ParticleSystemRenderer>(true);
        }

        public void SetHeal(bool value)
        {
           _isHeal = value;
        }
        
        public void SetDamage(float damage)
        {
            _damage = damage;
        }

        public void SetDirection(bool flip)
        {
            if (flip)
            {
                psRenderer.flip = new Vector3(1,0,0);
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
                    if (!_isHeal)
                    {
                        target.OnDamage(_damage);
                    }
                    else
                    {
                        target.OnHeal(_damage);
                    }
                    break;
                case SkillMoveOption.Move:
                    transform.DOMove(target.transform.position, moveDuration).OnComplete(() =>
                    {
                        if (!_isHeal)
                        {
                            target.OnDamage(_damage);
                        }
                        else
                        {
                            target.OnHeal(_damage);
                        }
                    });
                    break;
            }
        }

        
    }
}