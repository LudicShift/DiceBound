using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class SkillTreeNodeWidget : ButtonWidget
    {
        public enum NodeVisualState
        {
            Locked,
            Unlockable,
            Unlocked,
        }

        [SerializeField] private string nodeId;
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Sprite iconSprite;
        [SerializeField] private ImageWidget frameImage;
        [SerializeField] private ImageWidget iconImage;
        [SerializeField] private TextWidget nameText;
        [SerializeField] private TextWidget costText;
        [SerializeField] private GameObject lockedVeil;
        [SerializeField] private GameObject unlockedCheckmark;

        private const string LockedLabel = "???";
        private static readonly Color LockedTint = new Color(0.35f, 0.35f, 0.35f, 1f);
        private static readonly Color UnlockedTint = Color.white;

        public string GetNodeId()
        {
            return nodeId;
        }

        public void SetVisual(string id, Sprite frame, Sprite icon)
        {
            nodeId = id;
            frameSprite = frame;
            iconSprite = icon;
            if (frameImage) frameImage.SetSprite(frameSprite);
            if (iconImage) iconImage.SetSprite(iconSprite);
        }

        public void Refresh(SkillTreeNodeDataTableRow data, NodeVisualState state, bool canAfford)
        {
            if (lockedVeil)
            {
                lockedVeil.SetActive(state == NodeVisualState.Locked);
            }

            if (unlockedCheckmark)
            {
                unlockedCheckmark.SetActive(state == NodeVisualState.Unlocked);
            }

            if (frameImage)
            {
                frameImage.image.color = state == NodeVisualState.Locked ? LockedTint : UnlockedTint;
            }

            if (state == NodeVisualState.Locked)
            {
                if (nameText) nameText.SetText(LockedLabel);
                if (costText) costText.SetText(LockedLabel);
                if (iconImage) iconImage.gameObject.SetActive(false);
                SetInteractable(false);
                return;
            }

            if (iconImage) iconImage.gameObject.SetActive(true);
            if (nameText) nameText.SetText(data.nameKey);
            if (costText) costText.SetText(data.diamondCost.ToString());
            if (iconImage && data.icon) iconImage.SetSprite(data.icon);

            SetInteractable(state == NodeVisualState.Unlockable && canAfford);
        }
    }
}
