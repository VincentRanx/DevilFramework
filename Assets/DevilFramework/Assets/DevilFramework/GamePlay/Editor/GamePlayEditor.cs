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
            PanelManager mgr = PanelManager.GetOrNewInstance();
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
            }
            uiCam.clearFlags = CameraClearFlags.Depth;
            uiCam.farClipPlane = 100;
            uiCam.nearClipPlane = 0;
            uiCam.cullingMask = 1 << uilayer;
            uiCam.orthographic = true;
            Ref.SetField(mgr, "m_UICamera", uiCam);
            ResetTransform(uiCam.transform);
        }

        static void ResetTransform(Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localScale = Vector3.one;
            trans.localRotation = Quaternion.identity;
        }
    }  
}
