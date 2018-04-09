using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HumanController : PlayerController
{

    protected override void Update()
    {
        base.Update();
        Transform cam = Camera.main.transform;
        Vector3 dir = new Vector3();
        dir.x = Input.GetAxis("Horizontal");
        dir.z = Input.GetAxis("Vertical");
        float len = dir.magnitude;
        dir = cam.localToWorldMatrix.MultiplyVector(dir);
        dir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
        if(len > 0.1f)
            Move(dir * len);
    }
}
