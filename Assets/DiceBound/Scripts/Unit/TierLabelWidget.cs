using System;
using KCoreKit;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound
{
    public class TierLabelWidget :WidgetBase
    {

        private ImageWidget[] stars;

        private void Awake()
        {
            stars = GetComponentsInChildren<ImageWidget>();
        }

        public void OnChange(int tier)
        {
            foreach (var star in stars)
            {
                star.Hide();
            }
            
            for (int i = 0; i <= tier; i++)
            {
                stars[i].Show();
            }
        }
    }
}