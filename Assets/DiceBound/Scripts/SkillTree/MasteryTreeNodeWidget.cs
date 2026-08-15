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
        public TooltipProvider tooltipProvider;

        public void Awake()
        {
            tooltipProvider = GetComponent<TooltipProvider>();
        }

        public void OnUpdate()
        {
            tooltipProvider.SetTooltipPosition(transform.position,Vector2.zero);
        }

        public void Setup(MasteryDataTableRow data)
        {
           _nameText = LocalizationManager.GetLocalizedText(data.nameKey);
           _descText = LocalizationManager.GetLocalizedText(data.descKey);
           _unlockCost = data.cost;
           iconImage.SetSprite(data.icon.ToSprite());
           tooltipProvider.SetText("name",$"{_nameText}");
           tooltipProvider.SetText("desc",$"{_descText}");
           tooltipProvider.SetText("cost",$"{_unlockCost}");
        }

        public void Refresh(NodeVisualState state)
        {
            lockedVeil.Hide();
            switch (state)
            {
                case NodeVisualState.Locked:
                    lockedVeil.Show();
                    break;
                case NodeVisualState.Unlockable:
                    frameImage.SetSprite(lockedFrameSprite);
                    break;
                case NodeVisualState.Unlocked:
                    frameImage.SetSprite(unlockedFrameSprite);
                    break; 
            }
        }
    }
}
