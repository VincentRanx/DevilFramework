using UnityEngine;
using UnityEditor;
using System.IO;

public class ABBuilder
{
    static string SelectAndSavePath()
    {
        string path = EditorPrefs.GetString("buildPath", Path.GetDirectoryName(Application.dataPath));
        if (!Directory.Exists(path))
        {
            path = Application.dataPath;
        }
        path = EditorUtility.SaveFolderPanel("Save AssetBundles", Path.GetDirectoryName(path), Path.GetFileName(path));
        return path;
    }

    [MenuItem("Devil Framework/AssetBundles/Build For Windows", true)]
    private static bool CanCreateAB()
    {
        GameObject obj = Selection.activeObject as GameObject;
        return obj && obj.activeInHierarchy;
    }

    [MenuItem("Devil Framework/AssetBundles/Build For Windows")]
    public static void BuildABForWin32()
    {
        string path = SelectAndSavePath();
        if (!string.IsNullOrEmpty(path))
        {
            EditorPrefs.SetString("buildPath", path);
            //BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            
        }

    }

    [MenuItem("CONTEXT/CurvePath/TestItem")]
    private static void NewMenuOption(MenuCommand menuCommand)
    {
        // The RigidBody component can be extracted from the menu command using the context field.
    }

    static Transform root;

    //[MenuItem("GameObject/Collepse", true)]
    //static bool CanCollepseObjs()
    //{
    //    GameObject obj = Selection.activeGameObject;
    //    root = obj && obj.activeInHierarchy ? obj.transform.parent : null;
    //    for (int i = 0; i < Selection.gameObjects.Length; i++)
    //    {
    //        obj = Selection.gameObjects[i];
    //        if (obj.activeInHierarchy)
    //        {
    //            if(obj.transform.parent != root)
    //            {
    //                root = null;
    //                return false;
    //            }
    //        }
    //    }
    //    return true;
    //}

    //[MenuItem("GameObject/Collepse", false, -1)]
    //static void CollepseObjs()
    //{
    //    GameObject folder = new GameObject("Folder");
    //    Transform trans = folder.transform;
    //    trans.SetParent(root, false);
    //    trans.localPosition = Vector3.zero;
    //    trans.localRotation = Quaternion.identity;
    //    trans.localScale = Vector3.one;
    //    for (int i = 0; i < Selection.gameObjects.Length; i++)
    //    {
    //        GameObject obj = Selection.gameObjects[i];
    //        obj.transform.SetParent(trans, true);
    //    }
    //    Debug.Log("active" + Selection.activeGameObject,Selection.activeGameObject);
    //}
}
