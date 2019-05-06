using UnityEngine;

namespace Devil
{
    public interface ICulling
	{
        BoundingSphere Bounding { get; }
        void OnCulling(bool visible, int lv);
    }
}