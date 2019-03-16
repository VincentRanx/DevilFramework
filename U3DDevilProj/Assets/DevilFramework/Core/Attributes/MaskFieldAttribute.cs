using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class MaskFieldAttribute : PropertyAttribute
{
    public bool IsToggle { get; set; }
    public bool MultiSelectable { get; set; }
    public string[] Names { get; set; }

    public MaskFieldAttribute() { MultiSelectable = true; IsToggle = true; }

    public MaskFieldAttribute(params string[] names)
    {
        Names = names;
        MultiSelectable = true;
        IsToggle = true;
    }
}
