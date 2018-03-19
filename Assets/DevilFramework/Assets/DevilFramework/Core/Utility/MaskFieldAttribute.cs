using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class MaskFieldAttribute : PropertyAttribute
{
    public bool IsMask = false;
}
