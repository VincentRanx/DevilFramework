using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class MaskFieldAttribute : PropertyAttribute
{
    //public bool IsMask = false;
    public string[] Names { get; set; }
}
