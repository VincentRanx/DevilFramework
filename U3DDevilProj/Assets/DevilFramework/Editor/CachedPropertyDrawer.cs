using UnityEditor;

namespace DevilEditor
{
    public abstract class CachedPropertyDrawer : PropertyDrawer 
	{
        protected SerializedProperty property;
        protected SerializedProperty[] cachedProperties;

        public CachedPropertyDrawer() : base()
        {
        }

        protected abstract string[] GetRelativeProperties();

        protected virtual void OnValidateProperty()
        {

        }

        public void ValidateProperty(SerializedProperty property)
        {
            if (property != this.property)
            {
                this.property = property;
                var properties = GetRelativeProperties();
                if (cachedProperties == null || cachedProperties.Length != properties.Length)
                {
                    cachedProperties = new SerializedProperty[properties.Length];
                }
                if (property != null)
                {
                    for (int i = 0; i < cachedProperties.Length; i++)
                    {
                        cachedProperties[i] = DevilEditorUtility.FindRelativeProperty(property, properties[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < cachedProperties.Length; i++)
                    {
                        cachedProperties[i] = null;
                    }
                }
                OnValidateProperty();
            }
        }

        protected SerializedProperty Prop(int id)
        {
            return cachedProperties[id];
        }
    }
}