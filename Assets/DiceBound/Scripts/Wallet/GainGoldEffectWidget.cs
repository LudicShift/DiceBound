using System.Collections;
using DG.Tweening;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class GainGoldEffectWidget : WidgetBase
    {
        public IEnumerator Play(Vector2 from, Vector2 to)
        {
            rectTransform.anchoredPosition = from;
           yield return rectTransform.DOMove(to, 0.5f).WaitForCompletion();
        }
    }
}