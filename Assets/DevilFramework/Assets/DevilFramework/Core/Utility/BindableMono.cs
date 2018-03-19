using UnityEngine;
using Devil.Utility;

namespace Devil
{
    public class BindableMono : MonoBehaviour
    {
#if UNITY_EDITOR

    [ContextMenu("绑定同名对象")]
    protected void BindProperties()
    {
        this.BindPropertyiesByName();
    }

    [ContextMenu("重命名已绑定对象")]
    protected void RenameBundProperties()
    {
        this.RenameBindableProperties();
    }
#endif
    }
}