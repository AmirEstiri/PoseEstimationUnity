using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCam : MonoBehaviour
{
    WebCamTexture webCam;
    void Start()
    {
        if (webCam == null)
            webCam = new WebCamTexture();

        webCam.deviceName = WebCamTexture.devices[1].name;
        GetComponent<Renderer>().material.mainTexture = webCam;

        if (!webCam.isPlaying)
            webCam.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
