using UnityEngine;

namespace Devil.Utility
{
    public static class ComponentUtil
    {

        public const string TAG_MAIN_CAMERA = "MainCamera";

        public static Camera ActiveCameraForLayer(int layer)
        {
            Camera[] cams = Camera.allCameras;
            for (int i = 0; i < cams.Length; i++)
            {
                Camera c = cams[i];
                if (c.isActiveAndEnabled && (c.cullingMask & (1 << layer)) != 0)
                {
                    return c;
                }
            }
            return null;
        }

        public static Camera MainCamera
        {
            get
            {
                Camera cam = Camera.main;
                if (cam && cam.isActiveAndEnabled)
                    return cam;
                Camera[] cams = Camera.allCameras;
                int want = 0;
                for (int i = 0; i < cams.Length; i++)
                {
                    int w = cams[i].MostWantedValueForMain();
                    if (w > want)
                    {
                        want = w;
                        cam = cams[i];
                    }
                }
                return cam;
            }
        }

        public static int MostWantedValueForMain(this Camera cam)
        {
            if (!cam)
                return 0;
            int n = 1;
            if (cam.isActiveAndEnabled)
                n |= 0x40000;
            if (cam.targetTexture == null)
                n |= 0x20000;
            if (cam.tag == TAG_MAIN_CAMERA)
                n |= 0x10000;
            if (cam.clearFlags == CameraClearFlags.Skybox)
                n |= 0x8000;
            if (cam.clearFlags == CameraClearFlags.SolidColor)
                n |= 0x4000;
            return n;
        }

        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T com = go.GetComponent<T>();
            if (com == null)
                com = go.AddComponent<T>();
            return com;
        }

        public static T GetComponentInChildren<T>(Transform root, string path) where T : Component
        {
            if (root == null)
                return null;
            var trans = root.Find(path);
            return trans == null ? null : trans.GetComponent<T>();
        }

        public static T GetComponentInParent<T>(Transform trans, bool considerSelf = false) where T : Component
        {
            T cmp = null;
            if (trans)
            {
                Transform root;
                if (considerSelf)
                {
                    root = trans;
                }
                else
                {
                    root = trans.parent;
                }
                while (root)
                {
                    cmp = root.GetComponent<T>();
                    if (cmp)
                        return cmp;
                    root = root.parent;
                }
            }
            return cmp;
        }


        public static Transform MatchRecursive(Transform root, FilterDelegate<Transform> filter)
        {
            if (filter(root))
            {
                return root;
            }
            else
            {
                int len = root.childCount;
                for (int i = 0; i < len; i++)
                {
                    Transform trans = root.GetChild(i);
                    trans = MatchRecursive(trans, filter);
                    if (trans)
                        return trans;
                }
                return null;
            }
        }

        public static T FindComponentInScene<T>() where T : Component
        {
            return Object.FindObjectOfType<T>();
        }
        
        public static void ResetTransform(Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }

        public static bool IsActiveSelection(this GameObject go)
        {
#if UNITY_EDITOR
            var sel = UnityEditor.Selection.activeGameObject;
            if (sel == go)
                return true;
            if (go != null && sel != null && sel.transform.IsChildOf(go.transform))
                return true;
            return false;
#else
            return false;
#endif
        }
    }
}
