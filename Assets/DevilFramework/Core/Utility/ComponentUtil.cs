using UnityEngine;

namespace DevilTeam.Utility
{
    public class ComponentUtil
    {

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
                for(int i = 0; i < len; i++)
                {
                    Transform trans = root.GetChild(i);
                    trans = MatchRecursive(trans, filter);
                    if (trans)
                        return trans;
                }
                return null;
            }
        }
        
    }
}
