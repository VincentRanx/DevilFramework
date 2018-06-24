using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCam : MonoBehaviour {

    RawImage mImg;

	// Use this for initialization
	void Start () {
        mImg = GetComponent<RawImage>();
		if(WebCamTexture.devices.Length > 0)
        {
            string deviceName = WebCamTexture.devices[0].name;
            Debug.Log(deviceName);
            WebCamTexture tex = new WebCamTexture(deviceName);
            mImg.texture = tex;
            tex.Play();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
