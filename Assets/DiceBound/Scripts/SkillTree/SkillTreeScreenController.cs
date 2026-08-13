using System.Collections.Generic;
using Ami.BroAudio;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class SkillTreeScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private ButtonWidget openButtonWidget;
        [SerializeField] private ButtonWidget closeButtonWidget;
        [SerializeField] private TextWidget diamondTextWidget;
        [SerializeField] private SoundID unlockSFX;

        private SkillTreeManager _skillTreeManager;
        private List<SkillTreeNodeWidget> _nodeWidgets;

        private void Awake()
        {
            _skillTreeManager = SkillTreeManager.GetInstance();
            _nodeWidgets = new List<SkillTreeNodeWidget>(GetComponentsInChildren<SkillTreeNodeWidget>(true));

            foreach (var nodeWidget in _nodeWidgets)
            {
                var capturedNodeId = nodeWidget.GetNodeId();
                nodeWidget.onClickAction += () => OnNodeClick(capturedNodeId);
            }

            if (openButtonWidget) openButtonWidget.onClickAction += ShowCanvas;
            if (closeButtonWidget) closeButtonWidget.onClickAction += HideCanvas;

            HideCanvas();
        }

        public void ShowCanvas()
        {
            panelGroup.alpha = 1f;
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
            RefreshAll();
        }

        public void HideCanvas()
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }

        private void OnNodeClick(string nodeId)
        {
            if (_skillTreeManager.TryUnlock(nodeId))
            {
                if (unlockSFX.IsValid())
                {
                    BroAudio.Play(unlockSFX);
                }

                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            if (diamondTextWidget)
            {
                diamondTextWidget.SetText(_skillTreeManager.GetDiamond().ToString());
            }

            var nodeData = _skillTreeManager.GetAllNodeData();
            foreach (var nodeWidget in _nodeWidgets)
            {
                var nodeId = nodeWidget.GetNodeId();
                if (!nodeData.TryGetValue(nodeId, out var data))
                {
                    continue;
                }

                SkillTreeNodeWidget.NodeVisualState state;
                if (_skillTreeManager.IsNodeUnlocked(nodeId))
                {
                    state = SkillTreeNodeWidget.NodeVisualState.Unlocked;
                }
                else if (_skillTreeManager.ArePrerequisitesMet(nodeId))
                {
                    state = SkillTreeNodeWidget.NodeVisualState.Unlockable;
                }
                else
                {
                    state = SkillTreeNodeWidget.NodeVisualState.Locked;
                }

                var canAfford = _skillTreeManager.GetDiamond() >= data.diamondCost;
                nodeWidget.Refresh(data, state, canAfford);
            }
        }
    }
}
