using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test2 : Text
{
    readonly UIVertex[] mQuadVerts = new UIVertex[4];
    
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        TextGenerator gen = cachedTextGenerator;
        int index = text.IndexOf("<quad />");
        if(index >= 0)
        {
            int off = index << 2;
            mQuadVerts[0] = gen.verts[off];
            mQuadVerts[1] = gen.verts[off + 1];
            mQuadVerts[2] = gen.verts[off + 2];
            mQuadVerts[3] = gen.verts[off + 3];
            toFill.AddUIVertexQuad(mQuadVerts);
        }
    }
}
