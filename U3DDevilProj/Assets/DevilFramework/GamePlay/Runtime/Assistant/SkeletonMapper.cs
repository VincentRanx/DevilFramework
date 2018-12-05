using Devil.Utility;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    [ExecuteInEditMode]
    public class SkeletonMapper : MonoBehaviour
    {
        [System.Serializable]
        public class Skin
        {
            [SerializeField]
            public Transform m_RootBone;
            
            [SerializeField]
            public SkinnedMeshRenderer m_Mesh;
        }
        
        [SerializeField]
        private Transform m_OverrideRoot;

        [SerializeField]
        private Skin[] m_LodSkins = new Skin[0];
        
        [SerializeField]
        private bool m_Prepared;

        public Transform OverrideRootBone
        {
            get
            {
                return m_OverrideRoot;
            }
            set
            {
                if(m_OverrideRoot != value)
                {
                    m_OverrideRoot = value;
                    if(isActiveAndEnabled)
                    {
                        if (value == null)
                            ResetBones();
                        else
                            RemapBones(value);
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (!m_Prepared)
                enabled = false;
            else
                RemapBones(m_OverrideRoot);
        }

        private void OnDisable()
        {
            ResetBones();
        }

        public static string GetBonePath(Transform root, Transform bone)
        {
            if (bone == null || bone == root)
                return "";
            if (bone.parent == root)
                return bone.name;
            var buf = StringUtil.GetBuilder(bone.name);
            var parent = bone.parent;
            while(parent != null && parent != root)
            {
                buf.Insert(0, '/');
                buf.Insert(0, parent.name);
                parent = parent.parent;
            }
            return StringUtil.ReleaseBuilder(buf);
        }
        
        private void ResetBones()
        {
            if (m_Prepared)
            {
                foreach (var skin in m_LodSkins)
                {
                    if (skin == null || skin.m_Mesh == null)
                        continue;
                    var bones = skin.m_Mesh.bones;
                    var root = skin.m_Mesh.rootBone;
                    if (root != skin.m_RootBone && root != null && skin.m_RootBone != null)
                    {
                        for (int i = 0; i < bones.Length; i++)
                        {
                            var bone = bones[i];
                            if (bone == null)
                                continue;
                            var path = GetBonePath(root, bone);
                            var newbone = skin.m_RootBone.Find(path);
                            bones[i] = newbone;
                        }
                        skin.m_Mesh.bones = bones;
                        skin.m_Mesh.rootBone = skin.m_RootBone;
                    }
                }
            }
        }
        
        private bool RemapBones(Transform overrideBone)
        {
            if (!m_Prepared || overrideBone == null)
                return false;
            foreach(var skin in m_LodSkins)
            {
                if (skin == null || skin.m_Mesh == null)
                    continue;
                var bones = skin.m_Mesh.bones;
                var root = skin.m_Mesh.rootBone;
                if (root != overrideBone && root != null)
                {
                    for (int i = 0; i < bones.Length; i++)
                    {
                        var bone = bones[i];
                        if (bone == null)
                            continue;
                        var path = GetBonePath(root, bone);
                        var newbone = overrideBone.Find(path);
                        if (newbone == null)
                            return false;
                        bones[i] = newbone;
                    }
                    skin.m_Mesh.bones = bones;
                    skin.m_Mesh.rootBone = overrideBone;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!m_Prepared)
                enabled = false;
        }

        [ContextMenu("Reset Bones")]
        void ResetBoneState()
        {
            ResetBones();
            m_OverrideRoot = null;
        }

        [ContextMenu("Init SkinnedMeshes")]
        void GetMeshes()
        {
            if(m_Prepared)
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", "Make sure is default state and not check prepared", "OK");
                return;
            }
            UnityEditor.SerializedObject target = new UnityEditor.SerializedObject(this);
            target.Update();
            var skin = target.FindProperty("m_LodSkins");
            skin.ClearArray();
            //if(skin.arraySize == 0)
            {
                var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
                for(int i = 0; i < meshes.Length; i++)
                {
                    skin.InsertArrayElementAtIndex(i);
                    var pro = skin.GetArrayElementAtIndex(i).FindPropertyRelative("m_Mesh");
                    pro.objectReferenceValue = meshes[i];
                    pro = skin.GetArrayElementAtIndex(i).FindPropertyRelative("m_RootBone");
                    pro.objectReferenceValue = meshes[i].rootBone;
                }
            }
            target.ApplyModifiedProperties();
        }
#endif
    }  
}
