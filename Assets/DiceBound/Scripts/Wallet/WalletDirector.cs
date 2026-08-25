using System.Collections;
using Ami.BroAudio;
using KCoreKit;
using KCoreKit.Scripts.Common;
using UnityEngine;

namespace DiceBound
{
    public class WalletDirector : DirectorBase
    {
        private SoundDirector _soundDirector;
        
        [SerializeField]
        private TextWidget goldTextWidget;
        [SerializeField]
        private Canvas goldEffectCanvas;
        [SerializeField]
        private TweenAnimationPlayer addGoldTween;
        
        private const int BaseGold = 500;
        private int _gold;

        private PrefabPool<GainGoldEffectWidget>  _goldEffectPool;
        private Camera _camera;

        public override IEnumerator OnInitialize()
        {
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            _camera = CameraManager.GetMainCamera();
            _goldEffectPool = new PrefabPool<GainGoldEffectWidget>(PrefabManager.CachePrefab<GainGoldEffectWidget>(),goldEffectCanvas.transform); 
            goldTextWidget.SetText(_gold.ToString());
            yield return null;
        }

        public void PlayGainGoldEffect(Vector3 worldFrom, int gold)
        {
            var to = goldTextWidget.rectTransform.position;
            float delay = 0;
            int devide = 5;
            for (int i = gold; i >= 0; i-=devide)
            {
                var offset = new Vector2(Random.Range(-50, 50), Random.Range(-50, 50));
                var from = WidgetUtility.WorldPositionToScreenPosition(_camera, worldFrom) +offset;
                
                if (i >= devide)
                {
                    StartCoroutine(GainGold(from, to,devide, delay));
                }
                else
                {
                    StartCoroutine(GainGold(from,to,i, delay));
                }

                delay += 0.03f;
            }
           
        }

        private IEnumerator GainGold(Vector2 from, Vector2 to, int gold, float delay)
        {
            yield return new WaitForSeconds(delay);
            var goldEffectWidget = _goldEffectPool.Get();
            yield return goldEffectWidget.Play(from, to);
            _goldEffectPool.Release(goldEffectWidget);
            AddGold(gold);
            
        }

        public bool HasGold(int gold)
        {
            return _gold >= gold;
        }

        public void SpendGold(int gold)
        {
            _gold -= gold;
            goldTextWidget.SetText(_gold.ToString());
        }

        public void AddGold(int gold)
        {
            _gold += gold;
            goldTextWidget.SetText(_gold.ToString());
            StartCoroutine(addGoldTween.Play());
            BroAudio.Play(_soundDirector.addGoldSFX);
        }

        public int GetGold()
        {
            return _gold;
        }
    }
}