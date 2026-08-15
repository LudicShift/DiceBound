using System.Collections;
using Ami.BroAudio;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class MasteryTreeDirector : DirectorBase
    {
        [SerializeField]
        private MasteryTreeCanvas masteryTreeCanvas;
        private MasteryManager _masteryManager;
        private TooltipDirector _tooltipDirector;
        [SerializeField] private TextWidget diamondTextWidget1;
        [SerializeField] private TextWidget diamondTextWidget2;
        [SerializeField] private SoundID unlockSFX;
        [SerializeField] private ButtonWidget openButtonWidget;
        [SerializeField] private ButtonWidget closeButtonWidget;
        
        public override IEnumerator OnInitialize()
        {
            _masteryManager = MasteryManager.GetInstance();
            _tooltipDirector = DirectorFacade.GetDirector<TooltipDirector>();
            
            if (openButtonWidget) openButtonWidget.onClickAction += masteryTreeCanvas.ShowCanvas;
            if (closeButtonWidget) closeButtonWidget.onClickAction +=  masteryTreeCanvas.HideCanvas;
            
            var nodes = masteryTreeCanvas.GetAllNodes();
            foreach (var node in nodes)
            {
                _tooltipDirector.BindTooltip("MasteryNode",node.tooltipProvider);
                var data = _masteryManager.GetNode(node.id);
                node.Setup(data);
                node.onClickAction += () => OnNodeClick(node.id);
            }

            yield return null;
        }
        
        private void OnNodeClick(string nodeId)
        {
            if (_masteryManager.TryUnlock(nodeId))
            {
                BroAudio.Play(unlockSFX);
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            diamondTextWidget1.SetText(_masteryManager.GetDiamond().ToString());
            diamondTextWidget2.SetText(_masteryManager.GetDiamond().ToString());

            var nodeData = _masteryManager.GetAllNodeData();
            var nodes = masteryTreeCanvas.GetAllNodes();
            foreach (var node in nodes)
            {
                if (!nodeData.TryGetValue(node.id, out var data))
                {
                    continue;
                }

                MasteryTreeNodeWidget.NodeVisualState state;
                if (_masteryManager.IsNodeUnlocked(node.id))
                {
                    state = MasteryTreeNodeWidget.NodeVisualState.Unlocked;
                }
                else if (_masteryManager.ArePrerequisitesMet(node.id))
                {
                    state = MasteryTreeNodeWidget.NodeVisualState.Unlockable;
                }
                else
                {
                    state = MasteryTreeNodeWidget.NodeVisualState.Locked;
                }

                node.Refresh(state);
            }
        }
        
    }
}