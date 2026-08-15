using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound.Editor
{
    public class MasteryTreeEditorWindow : EditorWindow
    {
        private const string MasteryTablePath = "Assets/DiceBound/Resources/DataTables/DT_Mastery.asset";
        private const string MasteryTextTablePath = "Assets/DiceBound/Resources/DataTables/DT_MasteryText.asset";
        private const string NodePrefabPath = "Assets/DiceBound/Prefabs/PF_MasteryTreeNode.prefab";

        private MasteryTreeCanvas _canvas;
        private List<MasteryTreeNodeWidget> _nodeWidgets = new List<MasteryTreeNodeWidget>();
        private MasteryTreeNodeWidget _selectedWidget;

        private DataTable _masteryTable;
        private DataTable _masteryTextTable;

        private Vector2 _listScroll;
        private bool _showUnusedIds = true;

        private GUIStyle _titleStyle;
        private GUIStyle _sectionTitleStyle;
        private GUIStyle _rowStyle;
        private Texture _warningIcon;

        [MenuItem("DiceBound/Mastery Tree Editor")]
        private static void Open()
        {
            var window = GetWindow<MasteryTreeEditorWindow>("Mastery Tree Editor");
            window.minSize = new Vector2(640, 480);
            window.Show();
        }

        private void OnEnable()
        {
            LoadDataTables();
            FindCanvasIfNeeded();
            RescanNodes();
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private void OnFocus()
        {
            FindCanvasIfNeeded();
            RescanNodes();
        }

        private void OnHierarchyChanged()
        {
            FindCanvasIfNeeded();
            RescanNodes();
            Repaint();
        }

        private void OnSelectionChange()
        {
            var go = Selection.activeGameObject;
            var widget = go != null ? go.GetComponent<MasteryTreeNodeWidget>() : null;
            if (widget != null && widget != _selectedWidget && _nodeWidgets.Contains(widget))
            {
                _selectedWidget = widget;
                Repaint();
            }
        }

        private void LoadDataTables()
        {
            _masteryTable = AssetDatabase.LoadAssetAtPath<DataTable>(MasteryTablePath);
            _masteryTextTable = AssetDatabase.LoadAssetAtPath<DataTable>(MasteryTextTablePath);
        }

        private void FindCanvasIfNeeded()
        {
            if (_canvas != null)
            {
                return;
            }

            var canvases = FindObjectsByType<MasteryTreeCanvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _canvas = canvases.Length > 0 ? canvases[0] : null;
        }

        private void RescanNodes()
        {
            _nodeWidgets = _canvas != null
                ? _canvas.GetComponentsInChildren<MasteryTreeNodeWidget>(true).ToList()
                : new List<MasteryTreeNodeWidget>();

            if (_selectedWidget != null && !_nodeWidgets.Contains(_selectedWidget))
            {
                _selectedWidget = null;
            }
        }

        private void InitStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            _titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, margin = new RectOffset(4, 4, 8, 4) };
            _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
            _rowStyle = new GUIStyle(EditorStyles.helpBox) { margin = new RectOffset(0, 0, 1, 1), padding = new RectOffset(4, 4, 2, 2) };
            _warningIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
        }

        private void OnGUI()
        {
            InitStyles();
            DrawHeader();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawListPanel();
                DrawDetailPanel();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                var icon = EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow").image;
                if (icon != null)
                {
                    GUILayout.Label(icon, GUILayout.Width(24), GUILayout.Height(24));
                }
                GUILayout.Label("Mastery Tree Editor", _titleStyle);
            }

            EditorGUILayout.Space(2);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                var newCanvas = (MasteryTreeCanvas)EditorGUILayout.ObjectField("Canvas", _canvas, typeof(MasteryTreeCanvas), true);
                if (EditorGUI.EndChangeCheck())
                {
                    _canvas = newCanvas;
                    RescanNodes();
                }

                if (GUILayout.Button("새로고침", GUILayout.Width(70)))
                {
                    LoadDataTables();
                    FindCanvasIfNeeded();
                    RescanNodes();
                }
            }

            if (_canvas == null)
            {
                EditorGUILayout.HelpBox("씬에서 MasteryTreeCanvas를 찾을 수 없습니다.", MessageType.Warning);
            }

            EditorGUILayout.Space(4);
        }

        private void DrawListPanel()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(280)))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label($"노드 ({_nodeWidgets.Count})", _sectionTitleStyle);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(_canvas == null))
                    {
                        if (GUILayout.Button("+ 새 노드", GUILayout.Width(70)))
                        {
                            CreateNode(null);
                        }
                    }
                }
                EditorGUILayout.Space(2);

                _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.ExpandHeight(true));
                foreach (var widget in _nodeWidgets)
                {
                    DrawNodeRow(widget);
                }
                EditorGUILayout.EndScrollView();

                DrawUnusedIdsSection();
            }
        }

        private void DrawNodeRow(MasteryTreeNodeWidget widget)
        {
            if (widget == null)
            {
                return;
            }

            bool isSelected = widget == _selectedWidget;
            var rect = EditorGUILayout.BeginHorizontal(_rowStyle, GUILayout.Height(22));

            var sprite = GetIconSprite(widget);
            var thumbnail = sprite != null ? AssetPreview.GetMiniThumbnail(sprite) : null;
            if (thumbnail != null)
            {
                GUILayout.Label(thumbnail, GUILayout.Width(18), GUILayout.Height(18));
            }
            else
            {
                GUILayout.Space(20);
            }

            var row = _masteryTable != null && !string.IsNullOrEmpty(widget.id) ? _masteryTable.Find<MasteryDataTableRow>(widget.id) : null;
            bool showWarning = string.IsNullOrEmpty(widget.id) || row == null;

            var labelText = string.IsNullOrEmpty(widget.id) ? "<비어있음>" : widget.id;
            var labelStyle = isSelected ? EditorStyles.boldLabel : EditorStyles.label;
            GUILayout.Label($"{labelText} ({widget.gameObject.name})", labelStyle, GUILayout.ExpandWidth(true));

            if (showWarning && _warningIcon != null)
            {
                GUILayout.Label(_warningIcon, GUILayout.Width(16), GUILayout.Height(16));
            }

            EditorGUILayout.EndHorizontal();

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                SelectAndFocus(widget);
                Event.current.Use();
            }
        }

        private void DrawUnusedIdsSection()
        {
            if (_masteryTable == null)
            {
                return;
            }

            var allIds = _masteryTable.Get<MasteryDataTableRow>().Select(r => r.id).ToList();
            var usedIds = new HashSet<string>(_nodeWidgets.Where(w => w != null && !string.IsNullOrEmpty(w.id)).Select(w => w.id));
            var unusedIds = allIds.Where(id => !usedIds.Contains(id)).ToList();

            EditorGUILayout.Space(4);
            _showUnusedIds = EditorGUILayout.Foldout(_showUnusedIds, $"미배치 DT_Mastery ID ({unusedIds.Count})", true);
            if (!_showUnusedIds)
            {
                return;
            }

            if (unusedIds.Count == 0)
            {
                EditorGUILayout.HelpBox("모든 id가 배치되었습니다.", MessageType.Info);
                return;
            }

            foreach (var id in unusedIds)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(id, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("새로 생성", GUILayout.Width(70)))
                    {
                        CreateNode(id);
                    }
                    using (new EditorGUI.DisabledScope(_selectedWidget == null))
                    {
                        if (GUILayout.Button("선택 노드에 적용", GUILayout.Width(110)))
                        {
                            ApplyIdToSelected(id);
                        }
                    }
                }
            }
        }

        private void DrawDetailPanel()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (_selectedWidget == null)
                {
                    EditorGUILayout.HelpBox("리스트에서 노드를 선택하세요.", MessageType.Info);
                    return;
                }

                DrawDetailHeader();
                EditorGUILayout.Space(6);
                DrawIdentitySection();
                EditorGUILayout.Space(6);
                DrawVisualsSection();
                EditorGUILayout.Space(6);
                DrawMasteryPreviewSection();
            }
        }

        private void DrawDetailHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(_selectedWidget.gameObject.name, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                bool canRename = !string.IsNullOrEmpty(_selectedWidget.id) && _selectedWidget.gameObject.name != _selectedWidget.id;
                using (new EditorGUI.DisabledScope(!canRename))
                {
                    if (GUILayout.Button("이름을 id로 변경", GUILayout.Width(120)))
                    {
                        RenameToId(_selectedWidget);
                    }
                }

                var previousColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
                if (GUILayout.Button("삭제", GUILayout.Width(50)))
                {
                    DeleteNode(_selectedWidget);
                }
                GUI.backgroundColor = previousColor;
            }
        }

        private void DrawIdentitySection()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Identity", _sectionTitleStyle);
                EditorGUILayout.Space(2);

                var so = new SerializedObject(_selectedWidget);
                so.Update();
                var idProp = so.FindProperty("id");
                string currentId = idProp.stringValue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    var newId = EditorGUILayout.TextField("Id", currentId);
                    if (EditorGUI.EndChangeCheck())
                    {
                        idProp.stringValue = newId;
                        currentId = newId;
                    }

                    if (GUILayout.Button("▾", GUILayout.Width(24)))
                    {
                        OpenIdDropdown(GUILayoutUtility.GetLastRect());
                    }
                }

                if (so.ApplyModifiedProperties())
                {
                    MarkSceneDirty(_selectedWidget);
                }

                if (string.IsNullOrEmpty(currentId))
                {
                    EditorGUILayout.HelpBox("id가 비어있습니다.", MessageType.Warning);
                }
                else if (_masteryTable == null || _masteryTable.Find<MasteryDataTableRow>(currentId) == null)
                {
                    EditorGUILayout.HelpBox($"'{currentId}' 는 DT_Mastery에 없는 id입니다.", MessageType.Warning);
                }
            }
        }

        private void OpenIdDropdown(Rect activatorRect)
        {
            if (_masteryTable == null)
            {
                return;
            }

            var ids = _masteryTable.Get<MasteryDataTableRow>().Select(r => r.id).ToList();
            var dropdown = new MasteryIdSelectorDropdown(new AdvancedDropdownState(), selectedId =>
            {
                var widget = _selectedWidget;
                if (widget == null)
                {
                    return;
                }

                var so = new SerializedObject(widget);
                so.Update();
                so.FindProperty("id").stringValue = selectedId;
                if (so.ApplyModifiedProperties())
                {
                    MarkSceneDirty(widget);
                }
                Repaint();
            });
            dropdown.Setup(ids);
            dropdown.Show(activatorRect);
        }

        private void DrawVisualsSection()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Visuals", _sectionTitleStyle);
                EditorGUILayout.Space(2);

                DrawIconField();

                var so = new SerializedObject(_selectedWidget);
                so.Update();
                DrawSpriteProperty(so, "unlockedFrameSprite", "Unlocked Frame Sprite");
                DrawSpriteProperty(so, "lockedFrameSprite", "Locked Frame Sprite");
                if (so.ApplyModifiedProperties())
                {
                    MarkSceneDirty(_selectedWidget);
                }
            }
        }

        private static void DrawSpriteProperty(SerializedObject so, string propertyName, string label)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(prop, new GUIContent(label));
        }

        private void DrawIconField()
        {
            var image = GetIconImageComponent(_selectedWidget);
            if (image == null)
            {
                EditorGUILayout.HelpBox("iconImage 참조가 없습니다.", MessageType.Warning);
                return;
            }

            var imageSo = new SerializedObject(image);
            imageSo.Update();
            var spriteProp = imageSo.FindProperty("m_Sprite");

            EditorGUILayout.PropertyField(spriteProp, new GUIContent("Icon"));

            if (imageSo.ApplyModifiedProperties())
            {
                MarkSceneDirty(_selectedWidget);
            }
        }

        private void DrawMasteryPreviewSection()
        {
            if (_masteryTable == null || string.IsNullOrEmpty(_selectedWidget.id))
            {
                return;
            }

            var row = _masteryTable.Find<MasteryDataTableRow>(_selectedWidget.id);
            if (row == null)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("DT_Mastery 미리보기", _sectionTitleStyle);
                EditorGUILayout.Space(2);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.LabelField("Cost", row.cost.ToString());
                    EditorGUILayout.LabelField("Effect Key", row.effectKey);
                    EditorGUILayout.LabelField("Effect Value", row.effectValue.ToString());
                    EditorGUILayout.LabelField("Prerequisites", row.prerequisiteIds != null && row.prerequisiteIds.Count > 0 ? string.Join(", ", row.prerequisiteIds) : "-");
                    EditorGUILayout.LabelField("Tags", row.tags != null && row.tags.Count > 0 ? string.Join(", ", row.tags) : "-");
                    EditorGUILayout.LabelField("Enabled", row.isEnable.ToString());
                }

                EditorGUILayout.Space(4);

                DrawLocalizedPreview("Name", row.nameKey);
                DrawLocalizedPreview("Desc", row.descKey);
            }
        }

        private void DrawLocalizedPreview(string label, string key)
        {
            if (_masteryTextTable == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            var textRow = _masteryTextTable.Find<LocalizedTextDataTableRow>(key);
            if (textRow == null)
            {
                EditorGUILayout.HelpBox($"'{key}' 텍스트를 DT_MasteryText에서 찾을 수 없습니다.", MessageType.Warning);
                return;
            }

            GUILayout.Label($"{label} (EN)", EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(textRow.EN ?? string.Empty, GUILayout.MinHeight(20));
            }

            GUILayout.Label($"{label} (KR)", EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(textRow.KR ?? string.Empty, GUILayout.MinHeight(20));
            }
        }

        private void SelectAndFocus(MasteryTreeNodeWidget widget)
        {
            _selectedWidget = widget;
            Selection.activeGameObject = widget.gameObject;
            EditorGUIUtility.PingObject(widget.gameObject);
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
            Repaint();
        }

        private void RenameToId(MasteryTreeNodeWidget widget)
        {
            Undo.RecordObject(widget.gameObject, "Rename Mastery Node");
            widget.gameObject.name = widget.id;
            EditorUtility.SetDirty(widget.gameObject);
            MarkSceneDirty(widget);
            Repaint();
        }

        private void CreateNode(string idToAssign)
        {
            if (_canvas == null)
            {
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NodePrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"노드 프리팹을 찾을 수 없습니다: {NodePrefabPath}");
                return;
            }

            Transform parent = _canvas.Root;
            if (parent == null)
            {
                parent = _nodeWidgets.Count > 0 && _nodeWidgets[0] != null
                    ? _nodeWidgets[0].transform.parent
                    : _canvas.transform;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(instance, "Create Mastery Node");

            if (instance.transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }

            var widget = instance.GetComponent<MasteryTreeNodeWidget>();
            if (widget != null && !string.IsNullOrEmpty(idToAssign))
            {
                var so = new SerializedObject(widget);
                so.Update();
                so.FindProperty("id").stringValue = idToAssign;
                so.ApplyModifiedProperties();
            }

            _canvas.EnsureConnectorGraphic();
            MarkSceneDirty(_canvas);
            RescanNodes();

            if (widget != null)
            {
                SelectAndFocus(widget);
            }
        }

        private void DeleteNode(MasteryTreeNodeWidget widget)
        {
            if (widget == null)
            {
                return;
            }

            var label = string.IsNullOrEmpty(widget.id) ? widget.gameObject.name : widget.id;
            bool confirmed = EditorUtility.DisplayDialog(
                "노드 삭제",
                $"'{label}' 노드를 삭제하시겠습니까?",
                "삭제",
                "취소");

            if (!confirmed)
            {
                return;
            }

            var scene = widget.gameObject.scene;
            Undo.DestroyObjectImmediate(widget.gameObject);

            if (_selectedWidget == widget)
            {
                _selectedWidget = null;
            }

            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            RescanNodes();
            Repaint();
        }

        private void ApplyIdToSelected(string id)
        {
            if (_selectedWidget == null)
            {
                return;
            }

            var so = new SerializedObject(_selectedWidget);
            so.Update();
            so.FindProperty("id").stringValue = id;
            if (so.ApplyModifiedProperties())
            {
                MarkSceneDirty(_selectedWidget);
            }
            Repaint();
        }

        private static Sprite GetIconSprite(MasteryTreeNodeWidget widget)
        {
            var image = GetIconImageComponent(widget);
            return image != null ? image.sprite : null;
        }

        private static Image GetIconImageComponent(MasteryTreeNodeWidget widget)
        {
            if (widget == null)
            {
                return null;
            }

            var so = new SerializedObject(widget);
            var imageWidget = so.FindProperty("iconImage").objectReferenceValue as ImageWidget;
            return imageWidget != null ? imageWidget.GetComponent<Image>() : null;
        }

        private static void MarkSceneDirty(Component component)
        {
            if (component == null)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
        }
    }
}
