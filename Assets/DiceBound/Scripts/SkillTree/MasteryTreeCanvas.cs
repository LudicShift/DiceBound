using System;
using System.Collections.Generic;
using Ami.BroAudio;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class MasteryTreeCanvas : MonoBehaviour
    {
       
        private List<MasteryTreeNodeWidget> _nodeWidgets;

        private void Awake()
        {
            _nodeWidgets = new List<MasteryTreeNodeWidget>(GetComponentsInChildren<MasteryTreeNodeWidget>(true));
            
        }

        public void Update()
        {
            foreach (var widget in _nodeWidgets)
            {
                widget.OnUpdate();
            }
        }

        public void ShowCanvas()
        {
            gameObject.SetActive(true);
        }

        public void HideCanvas()
        {
            gameObject.SetActive(false);
        }

        public List<MasteryTreeNodeWidget> GetAllNodes()
        {
            return _nodeWidgets;
        }
    }
}