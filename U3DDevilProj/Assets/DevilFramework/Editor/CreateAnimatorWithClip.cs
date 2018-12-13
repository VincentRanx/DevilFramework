using Devil.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    using AnimCtrl = UnityEditor.Animations.AnimatorController;

    public class CreateAnimatorWithClip : EditorWindow 
	{
        [MenuItem("Assets/Utils/Add (Delete) Animation Clips")]
        static void OpenCreater()
        {
            OpenCreater(Selection.activeObject);
        }

        public static void OpenCreater(Object assset)
        {
            var path = AssetDatabase.GetAssetPath(assset);
            if (string.IsNullOrEmpty(path))
                return;
            OpenCreater(path);
        }

        public static void OpenCreater(string path)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimCtrl>(path);
            if (controller == null)
                return;
            var win = GetWindow<CreateAnimatorWithClip>(true, "Create Animator", true);
            win.path = path;
            win.controller = controller;
            win.maxSize = new Vector2(250, 200);
            win.minSize = new Vector2(250, 200);
            win.ShowUtility();
        }

        string clips = "";
        List<string> arr = new List<string>();
        string path = "";
        AnimCtrl controller;

        private void OnGUI()
        {
            var rect = new Rect(10, 10, 230, 20);
            EditorGUI.SelectableLabel(rect, "Animation Clips");
            rect = new Rect(10, 30, 230, 130);
            clips = DevilEditorUtility.TextArea(rect, clips, " List-out Animation Clips");
            rect = new Rect(40, 165, 80, 30);
            if (GUI.Button(rect, "Create"))
            {
                GUI.FocusControl(null);
                arr.Clear();
                if (string.IsNullOrEmpty(clips) || !StringUtil.ParseArray(clips, arr, '\n') )
                    return;
                if (controller == null || string.IsNullOrEmpty(path))
                {
                    EditorUtility.DisplayDialog("Error", "Can't find controller.", "OK");
                    return;
                }
                var oldassets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var t in arr)
                {
                    if (string.IsNullOrEmpty(t))
                        continue;
                    var old = GlobalUtil.Find(oldassets, (x) => x.name == t);
                    if (old != null)
                        continue;
                    var clip = AnimCtrl.AllocateAnimatorClip(t);
                    AssetDatabase.AddObjectToAsset(clip, controller);
                }
                AssetDatabase.ImportAsset(path);
                Close();
            }
            rect = new Rect(130, 165, 80, 30);
            if(GUI.Button(rect, "Delete"))
            {
                GUI.FocusControl(null);
                arr.Clear();
                if (string.IsNullOrEmpty(clips) || !StringUtil.ParseArray(clips, arr, '\n'))
                    return;
                if (controller == null || string.IsNullOrEmpty(path))
                {
                    EditorUtility.DisplayDialog("Error", "Can't find controller.", "OK");
                    return;
                }
                var oldassets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach(var t in arr)
                {
                    if (string.IsNullOrEmpty(t))
                        continue;
                    var old = GlobalUtil.Find(oldassets, (x) => x is AnimationClip && x.name == t);
                    if (old == null)
                        continue;
                    Object.DestroyImmediate(old, true);
                }
                AssetDatabase.ImportAsset(path);
                Close();
            }
        }
    }
}