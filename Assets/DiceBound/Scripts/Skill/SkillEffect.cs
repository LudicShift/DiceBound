using System;
using DG.Tweening;
using UnityEngine;

namespace DiceBound
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
            psRenderer = GetComponentInChildren<ParticleSystemRenderer>(true);
        }
        

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }


        public Sequence Play(UnitCore target,Action<SkillEffect> callback)
        {
            Sequence sequence = DOTween.Sequence();
            switch (moveOption)
            {
                case SkillMoveOption.Warp:
                    sequence.AppendCallback(() =>
                    {
                        transform.position = target.transform.position;
                    });
                    sequence.AppendInterval(lifetime);
                    sequence.AppendCallback(() => callback.Invoke(this));
                    break;
                case SkillMoveOption.Move:
                    sequence.Append(transform.DOMove(target.transform.position, moveDuration)) ;
                    sequence.AppendInterval(lifetime);
                    sequence.AppendCallback(() => callback.Invoke(this));
                    break;
            }

            return sequence.Play();
        }

        
    }
}