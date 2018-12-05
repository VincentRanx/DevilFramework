using Devil.GamePlay;
using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DevilEditor
{
    public class GamePlayEditor
    {
        [MenuItem("Devil Framework/Create/UI Root")]
        public static void CreateUIRoot()
        {
            int uilayer = LayerMask.NameToLayer("UI");
            PanelManager mgr = PanelManager.Instance;
            if(mgr == null)
            {
                var go = new GameObject();
                mgr = go.AddComponent<PanelManager>();
            }
            mgr.gameObject.layer = uilayer;
            mgr.gameObject.name = "PanelManager";
            ResetTransform(mgr.transform);
            GameObject eventsys;
            EventSystem es = mgr.GetComponentInChildren<EventSystem>();
            if(es == null)
            {
                eventsys = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                eventsys.transform.SetParent(mgr.transform, false);
            }
            else
            {
                eventsys = es.gameObject;
                if (eventsys.GetComponent<StandaloneInputModule>() == null)
                    eventsys.AddComponent<StandaloneInputModule>();
            }
            eventsys.layer = uilayer;
            ResetTransform(eventsys.transform);
            Camera uiCam = mgr.GetComponentInChildren<Camera>();
            if(uiCam == null)
            {
                GameObject cam = new GameObject("UICamera", typeof(Camera));
                cam.transform.SetParent(mgr.transform, false);
                uiCam = cam.GetComponent<Camera>();
                cam.layer = uilayer;
            }
            uiCam.clearFlags = CameraClearFlags.Depth;
            uiCam.farClipPlane = 100;
            uiCam.nearClipPlane = 0;
            uiCam.cullingMask = 1 << uilayer;
            uiCam.orthographic = true;
            bool set;
            Ref.SetField(mgr, "m_UICamera", uiCam, out set);
            ResetTransform(uiCam.transform);
        }

        static void ResetTransform(Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localScale = Vector3.one;
            trans.localRotation = Quaternion.identity;
        }

        [InitializeOnLoadMethod]
        static void OnUnityLoaded()
        {
            //SerializedObject tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
            //SerializedProperty iter = tags.GetIterator();
            //while (iter.NextVisible(true))
            //{
            //    Debug.LogFormat("name:{0} type:{1}",iter.name, iter.type);
            //}
            
        }
    }  
}
