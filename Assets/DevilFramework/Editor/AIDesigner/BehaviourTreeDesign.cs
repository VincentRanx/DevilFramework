using UnityEngine;
using UnityEditor;
using DevilTeam.AI;
using System.IO;

namespace DevilTeam.Editor
{

    public class BehaviourTreeDesign : EditorWindow
    {
        Rect clipRect;
        Vector2 viewPos;
        bool repaint;
        bool focusCenter;
        Vector2 mousePos;
        Vector2 mouseDeltaPos;
        Vector2 scroll;
        bool mouseDrag;
        Rect tmpRect;
        Vector2 viewHalfSize = new Vector2(5000, 5000);
        Vector2 viewOffset;
        Vector2 centerOffset;
        bool onFocus;

        uint dirtyFlags;

        string viewportTitle = @"<size=30><b>BEHAVIOUR DESIGN</b></size><size=23> (ver.1.0.0)</size>";

        BTDesignToolkit toolkit;

        [MenuItem("Devil Framework/BehaviourTree Design")]
        public static void OpenBTDesigner()
        {
            BehaviourTreeDesign window = GetWindow<BehaviourTreeDesign>();
            //window.minSize = new Vector2(600, 480);
            window.Show();
        }

        // 计算鼠标移动量
        void ProcessMouseDeltaPos()
        {
            if (Event.current.type == EventType.mouseDown)
            {
                mousePos = Event.current.mousePosition;
                mouseDeltaPos = Vector2.zero;
            }
            else if (Event.current.type == EventType.mouseDrag)
            {
                Vector2 pos = Event.current.mousePosition;
                mouseDeltaPos = pos - mousePos;
                mousePos = pos;
            }
            else if (Event.current.type == EventType.mouseUp)
            {
                mouseDeltaPos = Vector2.zero;
            }
        }

        // 响应鼠标事件
        void ProcessMouseEvent()
        {
            bool intercept = clipRect.Contains(Event.current.mousePosition);
            if (!intercept)
            {
                return;
            }
            repaint |= true;
            if (Event.current.isKey && Event.current.control && Event.current.keyCode == KeyCode.F)
            {
                focusCenter = true;
            }

            if (Event.current.type == EventType.mouseDown && Event.current.button == 2)
            {
                mouseDrag = true;
            }
            else if (mouseDrag && Event.current.type == EventType.mouseDrag)
            {
                viewPos -= mouseDeltaPos;
                viewPos.x = Mathf.Clamp(viewPos.x, -viewHalfSize.x, viewHalfSize.x);
                viewPos.y = Mathf.Clamp(viewPos.y, -viewHalfSize.y, viewHalfSize.y);
            }
            if (Event.current.type == EventType.mouseUp)
            {
                mouseDrag = false;
                if (Event.current.button != 2 && toolkit != null && !toolkit.useMouseEvent)
                    toolkit.SelectControl();
            }
                //GUI.FocusControl(BTDesignToolkit.ELE_NAME);
        }

        //聚焦状态图中心
        private void ProcessFocusCenter()
        {
            if (focusCenter && Vector2.Distance(Vector2.zero, viewPos) > 1)
            {
                repaint |= true;
                viewPos = Vector2.Lerp(viewPos, Vector2.zero, 0.1f);
            }
            else
            {
                focusCenter = false;
            }
        }

        private void OnEnable()
        {
            if(toolkit == null)
            {
                toolkit = new BTDesignToolkit();
            }
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playmodeStateChanged += OnSelectionChanged;
            toolkit.InitGraph();
            toolkit.LoadPrefab();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playmodeStateChanged -= OnSelectionChanged;
        }

        private void OnFocus()
        {
            onFocus = true;
        }

        private void OnLostFocus()
        {
            onFocus = false;
        }

        private void OnSelectionChanged()
        {
            toolkit.InitGraph();
            Repaint();
        }

        private void OnGUI()
        {
            ProcessMouseDeltaPos();
            GUI.skin.label.richText = true;
            if (toolkit != null)
            {
                toolkit.PrepareUpdate();
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal("TE toolbar");
            dirtyFlags |= GUILayout.Button("Reload", "TE toolbarbutton", GUILayout.Width(50)) ? BTDesignToolkit.DIRTY_RELOAD : 0;
            dirtyFlags |= GUILayout.Button("Apply", "TE toolbarbutton", GUILayout.Width(50)) ? BTDesignToolkit.DIRTY_SAVE_GRAPH | BTDesignToolkit.DIRTY_SAVE_PREFAB : 0;
            dirtyFlags |= GUILayout.Button("Apply Prefab", "TE toolbarbutton", GUILayout.Width(100)) ? BTDesignToolkit.DIRTY_SAVE_PREFAB : 0;
            if(Selection.activeGameObject != null)
                dirtyFlags |= GUILayout.Button("Apply Graph", "TE toolbarbutton", GUILayout.Width(100)) ? BTDesignToolkit.DIRTY_SAVE_GRAPH : 0;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUI.SetNextControlName(BTDesignToolkit.ELE_NAME);

            QuickGUI.ReportView(ref clipRect, viewPos, OnDrawCallback, position.height, 100, viewportTitle);
            viewOffset = -viewPos + clipRect.size.y * 0.5f * Vector2.up;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            ProcessMouseEvent();
            ProcessFocusCenter();

            if (toolkit != null)
            {
                dirtyFlags |= toolkit.dirtyFlag;

                if(Application.isPlaying && (dirtyFlags & (BTDesignToolkit.DIRTY_ADD_OR_DELETE_NODE | BTDesignToolkit.DIRTY_MODIFY_NODE)) != 0)
                {
                    dirtyFlags |= BTDesignToolkit.DIRTY_SAVE_GRAPH;
                }

                if ((dirtyFlags & BTDesignToolkit.DIRTY_SAVE_PREFAB) != 0)
                {
                    BTNode.ExportToXML(toolkit.prefabRoot, Path.Combine(Installizer.InstallRoot, "DevilFramework/Editor/AIDesigner/BTRepository.xml"));
                    AssetDatabase.Refresh();
                }

                if ((dirtyFlags & BTDesignToolkit.DIRTY_SAVE_GRAPH) != 0)
                {
                    toolkit.ApplyBehaviourGraph();
                }
            }
            if ((dirtyFlags & BTDesignToolkit.DIRTY_MASK_FOR_RELOAD) != 0)
            {
                toolkit.LoadPrefab();
                OnSelectionChanged();
                repaint = true;
            }
            else if ((dirtyFlags & BTDesignToolkit.DIRTY_MASK_APPLY_UPDATE) != 0)
            {
                repaint = true;
            }

            if (repaint || onFocus)
            {
                Repaint();
            }
            repaint = false;
            dirtyFlags &= ~BTDesignToolkit.DIRTY_MASK_CLEAR_VIR_FRAME;
        }

        void OnDrawCallback()
        {
            if (toolkit != null)
            {
                DrawAllNodes();
            }
        }

        void DrawAllNodes()
        {
            BTNode edit = toolkit.activeNode;

            for (int i = 0; i < toolkit.NodeCount; i++)
            {
                BTNode node = toolkit.NodeAt(i);
                if (!node.IsCollectedByParent && node.Parent != null)
                {
                    Color c = Color.white;
                    float width = 3;
                    if (Application.isPlaying && toolkit.graph)
                    {
                        BehaviourGraph.BehaviourNode nd = toolkit.graph.GetNodeById(node.Id);
                        if(nd != null)
                        {
                            Color c2 = nd.__resultStat == EBTState.failed ? Color.red : (nd.__resultStat == EBTState.running) ? Color.blue : Color.green;
                            c = Color.Lerp(c2, c, Mathf.Clamp01(nd.__deltaTime * 0.5f));
                            width = 5;
                        }
                    }
                    toolkit.ConnectNode(node, node.Parent, viewOffset, c, width);
                }
            }

            for (int i = 0; i < toolkit.NodeCount; i++)
            {
                BTNode node = toolkit.NodeAt(i);
                if (!node.collapsedByParent)
                {
                    toolkit.OnPaint(node, ref viewOffset);
                }
            }

            for (int i = 0; i < toolkit.NodeCount; i++)
            {
                BTNode node = toolkit.NodeAt(i);
                if (!node.collapsedByParent)
                {
                    toolkit.DrawComment(node, viewOffset);
                }
            }
            toolkit.DrawNodeEditor(viewOffset);
            toolkit.DrawPastePanel(clipRect, viewOffset);
        }

    }
}