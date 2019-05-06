using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class RangeFieldAttribute : PropertyAttribute
{
    public float Min { get; set; }
    public float Max { get; set; }

    public RangeFieldAttribute() { Min = float.MinValue; Max = float.MaxValue; }
}