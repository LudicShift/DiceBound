using System.Collections.Generic;
using KCoreKit;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound
{
    [ExecuteAlways]
    public class MasteryTreeConnectorGraphic : MaskableGraphic
    {
        private const string MasteryTableResourcePath = "DataTables/DT_Mastery";

        [SerializeField] private float lineWidth = 4f;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = new Color(1f, 1f, 1f, 0.35f);
        }

        private void Update()
        {
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var canvasComponent = GetComponentInParent<MasteryTreeCanvas>();
            if (canvasComponent == null)
            {
                return;
            }

            var nodes = canvasComponent.GetComponentsInChildren<MasteryTreeNodeWidget>(true);
            if (nodes.Length == 0)
            {
                return;
            }

            var table = Resources.Load<DataTable>(MasteryTableResourcePath);
            if (table == null)
            {
                return;
            }

            var nodesById = new Dictionary<string, MasteryTreeNodeWidget>();
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.id))
                {
                    nodesById[node.id] = node;
                }
            }

            foreach (var node in nodes)
            {
                if (string.IsNullOrEmpty(node.id))
                {
                    continue;
                }

                var row = table.Find<MasteryDataTableRow>(node.id);
                if (row?.prerequisiteIds == null)
                {
                    continue;
                }

                foreach (var prerequisiteId in row.prerequisiteIds)
                {
                    if (nodesById.TryGetValue(prerequisiteId, out var prerequisiteNode))
                    {
                        AddLine(vh, GetLocalPosition(prerequisiteNode.transform), GetLocalPosition(node.transform));
                    }
                }
            }
        }

        private Vector2 GetLocalPosition(Transform target)
        {
            return rectTransform.InverseTransformPoint(target.position);
        }

        private void AddLine(VertexHelper vh, Vector2 from, Vector2 to)
        {
            var direction = to - from;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            direction.Normalize();
            var normal = new Vector2(-direction.y, direction.x) * (lineWidth * 0.5f);

            var vertex = UIVertex.simpleVert;
            vertex.color = color;

            int startIndex = vh.currentVertCount;

            vertex.position = from - normal;
            vh.AddVert(vertex);
            vertex.position = from + normal;
            vh.AddVert(vertex);
            vertex.position = to + normal;
            vh.AddVert(vertex);
            vertex.position = to - normal;
            vh.AddVert(vertex);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }
    }
}
