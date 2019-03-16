using Devil.AI;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    using AssetBinder = BehaviourTreeRunner.AssetBinder;
    public class BehaviourTreeAssetBinder : System.IDisposable
    {
        private BehaviourTreeRunner runner;
        private BehaviourTreeAsset tree; // 持久化资源
        private BehaviourTreeAsset tmpAsset; // 编辑器编辑对象
        private AssetBinder runtimeBinder;

        public BehaviourTreeAsset source { get { return tree; } }
        public BehaviourLooper looper
        {
            get
            {
                return runtimeBinder == null ? null : runtimeBinder.Looper;
            }
        }
        public BehaviourTreeAsset runtime
        {
            get
            {
                return runtimeBinder == null ? null : runtimeBinder.RuntimeTree;
            }
        }

        public AssetBinder RuntimeBinder { get { return runtimeBinder; } }
        
        public BehaviourTreeAsset targetTree
        {
            get
            {
                if (tmpAsset == null && tree != null)
                {
                    tmpAsset = source.Clone();
                    if (updateTreeAsset != null)
                        updateTreeAsset();
                }
                return tmpAsset;
            }
        }
        public BehaviourTreeRunner targetRunner { get { return runner; } }

        public event System.Action updateTreeAsset;

        public string ObjName { get { return runner == null ? "[NO Runner]" : runner.name; } }

        public string AssetName
        {
            get {
                if (tree != null && !string.IsNullOrEmpty(tree.name))
                    return tree.name;
                else if (runner != null)
                    return string.Format("{0}_AIRES", runner.name);
                else
                    return "Unknown";
            }
        }
        
        public BehaviourTreeAssetBinder()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange stat)
        {
            if (stat == PlayModeStateChange.EnteredPlayMode || stat == PlayModeStateChange.EnteredEditMode)
            {
                if (runner == null)
                    runner = null;
                if (tree == null)
                    tree = null;
                if (tmpAsset != null)
                {
                    BehaviourTreeAsset.DestroyAsset(tmpAsset, true);
                    tmpAsset = null;
                }
                if (tree == null)
                {
                    if (SetSelectedAsset())
                        return;
                }
                else
                {
                    var go = Selection.activeGameObject;
                    var tmpru = go == null ? null : go.GetComponent<BehaviourTreeRunner>();
                    if (tmpru != null && tmpru.SourceAsset == tree)
                        runner = tmpru;
                }
                runtimeBinder = runner == null ? null : runner.ActiveBinder;
                if (updateTreeAsset != null)
                    updateTreeAsset();
            }
        }

        public bool SetSelectedAsset()
        {
            BehaviourTreeRunner tmprunner = null;
            BehaviourTreeAsset tmpasset = null;
            if (Selection.activeGameObject != null)
                tmprunner = Selection.activeGameObject.GetComponent<BehaviourTreeRunner>();
            else if (Selection.activeObject != null)
                tmpasset = Selection.activeObject as BehaviourTreeAsset;
            if (tmprunner != null)
            {
                SetBehaviourTreeRunner(tmprunner);
                return true;
            }
            if (tmpasset != null)
            {
                SetBehaviourTreeAsset(tmpasset);
                return true;
            }
            return false;
        }

        public void SetBehaviourBinder(AssetBinder binder)
        {
            if (binder == null || runtimeBinder == binder)
                return;
            if (tmpAsset != null)
            {
                BehaviourTreeAsset.DestroyAsset(tmpAsset, true);
                tmpAsset = null;
            }
            runner = binder.Runner;
            tree = binder.Source;
            runtimeBinder = binder;
            if (tree != null)
                tmpAsset = tree.Clone();
            if (updateTreeAsset != null)
                updateTreeAsset();
        }

        public void SetBehaviourTreeAsset(BehaviourTreeAsset asset)
        {
            if (asset == tree)
                return;
            if (tmpAsset != null)
            {
                BehaviourTreeAsset.DestroyAsset(tmpAsset, true);
                tmpAsset = null;
            }
            tree = asset;
            var bind = runner == null  || asset == null ? null : runner.GetBinder(asset);
            if (bind != null)
                runner = bind.Runner;
            runtimeBinder = bind;
            if (tree != null)
                tmpAsset = tree.Clone();
            if (updateTreeAsset != null)
                updateTreeAsset();
        }
        
        public void SetBehaviourTreeRunner(BehaviourTreeRunner runner)
        {
            if (runner == null)
            {
                this.runner = null;
                this.runtimeBinder = null;
                return;
            }
            if (this.runner == null)
                this.runner = null;
            var asset = runner == null ? tree : runner.SourceAsset;
            if (asset == null)
                asset = tree;
            var bind = runner == null ? null : runner.GetBinder(asset);
            if (asset == tree && this.runner == runner && runtimeBinder == bind)
                return;
            this.runner = runner;
            this.tree = asset;
            runtimeBinder = bind;
            if (tmpAsset != null)
            {
                BehaviourTreeAsset.DestroyAsset(tmpAsset, true);
                tmpAsset = null;
            }
            if (tree != null)
            {
                if (tree != null)
                    tmpAsset = tree.Clone();
                if (updateTreeAsset != null)
                    updateTreeAsset();
            }
        }

        public void Reset()
        {
            if (runner != null && tree == null)
            {
                tree = runner.SourceAsset;
            }
            if (tree != null)
            {
                runtimeBinder = runner == null ? null : runner.GetBinder(tree);
                if (tmpAsset == null)
                    tmpAsset = tree.Clone();
                else
                    tree.EditorMergeTo(null, tmpAsset);
                if (updateTreeAsset != null)
                    updateTreeAsset();
            }
        }

        public bool IsRunning()
        {
            return runtimeBinder != null && runtimeBinder.RuntimeTree != null && runtimeBinder.Runner.isActiveAndEnabled;
        }

        public bool IsActiveSelection()
        {
            if (Selection.activeGameObject != null)
                return runner != null && runner.gameObject == Selection.activeGameObject;
            if (Selection.activeObject != null)
                return tree != null && tree == Selection.activeObject;
            return false;
        }

        public void PingTarget()
        {
            if (runner != null)
                EditorGUIUtility.PingObject(runner.gameObject);
            else if (tree != null)
                EditorGUIUtility.PingObject(tree);
        }

        public void SelectTarget()
        {
            if (runner != null)
                Selection.activeGameObject = runner.gameObject;
            else if (tree != null)
                Selection.activeObject = tree;
        }

        public void Dispose()
        {
            if (tmpAsset != null)
            {
                BehaviourTreeAsset.DestroyAsset(tmpAsset, true);
                tmpAsset = null;
            }
            runner = null;
            tree = null;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        public void SaveAsset()
        {
            if (tmpAsset == null)
                return;
            List<BTNode> children = new List<BTNode>();
            var path = tree == null ? null : AssetDatabase.GetAssetPath(tree);
            tmpAsset.EditorResort();
            if (string.IsNullOrEmpty(path))
            {
                path = EditorUtility.SaveFilePanelInProject("Save Behaviour Tree", AssetName, "asset", "Save behaviour asset.");
                if (string.IsNullOrEmpty(path))
                    return;
                if (File.Exists(path))
                    AssetDatabase.DeleteAsset(path);
                tree = tmpAsset.Clone();
                var name = Path.GetFileName(path);
                if (name.EndsWith(".asset"))
                    name = name.Substring(0, name.Length - 6);
                tree.name = name;
                AssetDatabase.CreateAsset(tree, path);
                tree.GetAllNodes(children);
                foreach (var t in children)
                {
                    AssetDatabase.AddObjectToAsset(t.Asset, path);
                }
                AssetDatabase.ImportAsset(path);
            }
            else
            {
                tmpAsset.EditorMergeTo(path, tree);
                tree.GetAllNodes(children);
                foreach (var t in children)
                {
                    AssetDatabase.AddObjectToAsset(t.Asset, path);
                }
                AssetDatabase.ImportAsset(path);
            }
            //AssetDatabase.SaveAssets();
        }
        
    }

}