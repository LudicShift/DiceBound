using System;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class MasteryTreeNodeWidget : ButtonWidget
    {
        public enum NodeVisualState
        {
            Locked,
            Unlockable,
            Unlocked,
        }

        public string id;
        
        [SerializeField] private ImageWidget iconImage;
        [SerializeField] private ImageWidget frameImage;
        [SerializeField] private ImageWidget lockedVeil;
        [SerializeField] private Sprite unlockedFrameSprite;
        [SerializeField] private Sprite lockedFrameSprite;
        
        private string _nameText;
        private string _descText;
        private float _unlockCost;
        
        [HideInInspector]
        public TooltipProvider tooltipProvider => GetComponent<TooltipProvider>();

       
        public void OnUpdate()
        {
            tooltipProvider.SetTooltipPosition(transform.position,new Vector2(200,0));
        }

        public void Setup(MasteryDataTableRow data)
        {
           _nameText = LocalizationManager.GetLocalizedText(data.nameKey);
           _descText = LocalizationManager.GetLocalizedText(data.descKey);
           _unlockCost = data.cost;
           tooltipProvider.SetText("name",$"{_nameText}");
           tooltipProvider.SetText("desc",$"{_descText}");
        }

        public void Refresh(NodeVisualState state)
        {
            lockedVeil.Hide();
            switch (state)
            {
                case NodeVisualState.Locked:
                    lockedVeil.Show();
                    tooltipProvider.SetEnabled(false);
                    break;
                case NodeVisualState.Unlockable:
                    frameImage.SetSprite(lockedFrameSprite);
                    tooltipProvider.SetEnabled(true);
                    tooltipProvider.SetText("cost",$"{_unlockCost}");
                    break;
                case NodeVisualState.Unlocked:
                    frameImage.SetSprite(unlockedFrameSprite);
                    tooltipProvider.SetEnabled(true);
                    tooltipProvider.SetText("cost",$"Unlocked");
                    break; 
            }
        }
    }
}
