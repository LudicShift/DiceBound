using KCoreKit;
using UnityEngine;

namespace YatchDungeon
{
    public class DiceRerollButtonWidget : ButtonWidget
    {
        [SerializeField] private TextWidget rerollCount;

        public void SetCount(int count)
        {
            rerollCount.SetText($"x{count}");
        }

        public void Disable()
        {
            SetInteractable(false);
            rectTransform.localScale = Vector3.one * 0.8f;
            canvasGroup.alpha = 0.6f;
        }

        public void Enable()
        {
            SetInteractable(true);
            rectTransform.localScale = Vector3.one;
            canvasGroup.alpha = 1f;
        }
    }
}