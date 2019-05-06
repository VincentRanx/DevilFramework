using UnityEditor;

namespace DevilEditor
{
    public class SerializedPropertyCache : System.IDisposable
	{
        SerializedProperty target;
        string[] pNames;
        SerializedProperty[] properties;

        public SerializedPropertyCache()
        {

        }

        public SerializedPropertyCache(SerializedProperty p, string[] props)
        {
            SetProperty(p, props);
        }

        public SerializedProperty Target { get { return target; } }

		public void SetProperty(SerializedProperty property, string[] relativeProperty)
        {
            if(target != property || pNames == relativeProperty)
            {
                target = property;
                pNames = relativeProperty;
                properties = new SerializedProperty[pNames.Length];
                for(int i = 0; i < pNames.Length; i++)
                {
                    properties[i] = target.FindPropertyRelative(pNames[i]);
                }
            }
        }

        public SerializedProperty Prop(int index)
        {
            return properties[index];
        }

        public SerializedProperty Prop(string name)
        {
            return target.FindPropertyRelative(name);
        }

        public void Dispose()
        {
            if (properties != null)
            {
                for (int i = 0; i < properties.Length; i++)
                    properties[i].Dispose();
                properties = null;
            }
        }
    }
}