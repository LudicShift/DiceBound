using System.Collections;
using KCoreKit;
using UnityEngine;
using UnityEngine.Pool;

namespace YatchDungeon
{
    public class BattleDirector : DirectorBase
    {
        private UnitDirector _unitDirector;
        [SerializeField]
        private Canvas damageCanvas;
        private ObjectPool<DamageWidget>  _damageWidgetPool;
        private bool _isPlaying;

        public override IEnumerator OnInitialize()
        {
            _damageWidgetPool=new ObjectPool<DamageWidget>(CreateDamageWidget,GetDamageWidget,ReleaseDamageWidget);
            _unitDirector = DirectorFacade.GetSubMode<UnitDirector>();
            yield return null;
        }

        private void ReleaseDamageWidget(DamageWidget widget)
        {
            widget.Hide();
        }

        private void GetDamageWidget(DamageWidget widget)
        {
            widget.Show();
        }

        private DamageWidget CreateDamageWidget()
        {
            var widget = PrefabManager.Create<DamageWidget>();
            widget.SetParent(damageCanvas.transform);
            return widget;
        }

        public void BeginBattle()
        {
            _isPlaying = true;
           var units =  _unitDirector.GetAllUnit();
           foreach (var unit in units)
           {
               unit.OnBeginBattle();
           }
        }

        public void EndBattle()
        {
            _isPlaying = false;
            var units =  _unitDirector.GetAllUnit();
            foreach (var unit in units)
            {
                unit.OnEndBattle();
            }
        }

        public void ShowDamage(UnitCore core, int damage)
        {
           var screenPoint =  RectTransformUtility.WorldToScreenPoint(CameraManager.GetMainCamera(),core.transform.position);
           var damageWidget =  _damageWidgetPool.Get();
           damageWidget.SetColor(Color.red);
           damageWidget.Setup(damage);
           damageWidget.rectTransform.anchoredPosition = screenPoint;
           damageWidget.Animate(x=>_damageWidgetPool.Release(x));
        }

        public bool IsPlaying()
        {
            return _isPlaying;
        }

        public void ShowHeal(UnitCore core, int damage)
        {
            var screenPoint =  RectTransformUtility.WorldToScreenPoint(CameraManager.GetMainCamera(),core.transform.position);
            var damageWidget =  _damageWidgetPool.Get();
            damageWidget.SetColor(Color.green);
            damageWidget.Setup(damage);
            damageWidget.rectTransform.anchoredPosition = screenPoint;
            damageWidget.Animate(x=>_damageWidgetPool.Release(x));
        }
    }
}