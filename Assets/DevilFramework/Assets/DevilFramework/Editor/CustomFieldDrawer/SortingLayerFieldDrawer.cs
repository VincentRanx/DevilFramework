using UnityEngine;
using UnityEditor;
using System.Text;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(SortingLayerFieldAttribute))]
    public class SortingLayerFieldDrawer : PropertyDrawer
    {
        //StringBuilder builder = new StringBuilder();
        string[] mSortingLayers;
        int[] mIds;

        int GetIndex(int id)
        {
            if (mIds == null)
                return -1;
            for (int i = 0; i < mIds.Length; i++)
            {
                if (mIds[i] == id)
                    return i;
            }
            return -1;
        }

        void GetSortingLayers()
        {
            int len = mSortingLayers == null ? 0 : mSortingLayers.Length;
            SortingLayer[] layers = SortingLayer.layers;
            if(layers.Length != len)
            {
                mSortingLayers = new string[layers.Length];
                mIds = new int[layers.Length];
            }
            for(int i = 0; i < layers.Length; i++)
            {
                mSortingLayers[i] = layers[i].name;
                mIds[i] = layers[i].id;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            int lv = EditorGUI.indentLevel;
            //builder.Remove(0, builder.Length);
            //for (int i = 0; i < lv; i++)
            //{
            //    builder.Append("    ");
            //}
            //EditorGUI.indentLevel = 0;
            //builder.Append(label.text);
            //label.text = builder.ToString();
            position = EditorGUI.PrefixLabel(position, label);
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                GetSortingLayers();
                int index = EditorGUI.Popup(position, GetIndex(property.intValue), mSortingLayers);
                if (index >= 0 && index < mIds.Length)
                    property.intValue = mIds[index];
            }
            else
            {
                EditorGUI.LabelField(position, "Use Integer Instead of " + property.propertyType);
            }
            EditorGUI.indentLevel = lv;

            EditorGUI.EndProperty();
        }
    }
}