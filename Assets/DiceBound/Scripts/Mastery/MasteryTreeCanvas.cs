using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceBound
{
    [ExecuteAlways]
    public class MasteryTreeCanvas : MonoBehaviour
    {
        private List<MasteryTreeNodeWidget> _nodeWidgets;
        [SerializeField] private MasteryTreeConnectorGraphic connectorGraphic;
        [SerializeField] private RectTransform root;

        public RectTransform Root => root;

        public void Setup()
        {
            _nodeWidgets = GetComponentsInChildren<MasteryTreeNodeWidget>(true).ToList();
        }

        private void OnEnable()
        {
            EnsureConnectorGraphic();
        }

        public void Update()
        {
            if (_nodeWidgets == null)
            {
                return;
            }

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

        // Lazily creates the line-connector graphic the first time a node exists to parent it
        // alongside, so prerequisite lines "just work" without needing manual prefab wiring.
        public void EnsureConnectorGraphic()
        {
            if (connectorGraphic != null)
            {
                return;
            }

            connectorGraphic = GetComponentInChildren<MasteryTreeConnectorGraphic>(true);
            if (connectorGraphic != null)
            {
                if (connectorGraphic.GetComponent<CanvasRenderer>() == null)
                {
                    connectorGraphic.gameObject.AddComponent<CanvasRenderer>();
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorUtility.SetDirty(connectorGraphic.gameObject);
                    }
#endif
                }
                return;
            }

            Transform parent = root;
            if (parent == null)
            {
                var nodeWidgets = GetComponentsInChildren<MasteryTreeNodeWidget>(true);
                if (nodeWidgets.Length == 0)
                {
                    return;
                }

                parent = nodeWidgets[0].transform.parent;
            }

            if (parent == null)
            {
                return;
            }

            var go = new GameObject("Connectors", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            connectorGraphic = go.AddComponent<MasteryTreeConnectorGraphic>();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.EditorUtility.SetDirty(go);
            }
#endif
        }
    }
}
