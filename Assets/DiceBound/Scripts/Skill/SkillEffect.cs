using System;
using Ami.BroAudio;
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

        public void Start()
        {
            var soundSource = GetComponent<SoundSource>();
            if (soundSource)
            {
                soundSource.enabled = true;
            }
        }
        

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>발사부터 타겟 도달까지 걸리는 시간.</summary>
        public float GetImpactDelay()
        {
            return moveOption == SkillMoveOption.Move ? moveDuration : 0f;
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
                    var targetDirection = Vector3.Normalize(target.transform.position - transform.position);
                    transform.rotation = Quaternion.FromToRotation(Vector3.right,targetDirection);
                    sequence.Join(transform.DOMove(target.transform.position, moveDuration)) ;
                    sequence.AppendInterval(lifetime);
                    sequence.AppendCallback(() => callback.Invoke(this));
                    break;
            }

            return sequence.Play();
        }

        
    }
}