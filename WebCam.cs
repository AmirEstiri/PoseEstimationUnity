using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WebCam : MonoBehaviour
{
    WebCamTexture wct;
    void Start()
    {
        if (wct == null)
            wct = new WebCamTexture();

        wct.deviceName = WebCamTexture.devices[1].name;
        GetComponent<Renderer>().material.mainTexture = wct;

        if (!wct.isPlaying)
            wct.Play();

        //StartCoroutine(RunCam());
    }

    IEnumerator RunCam()
    {
        while (true)
        {
            Debug.Log("Start");
            Test();
            Debug.Log("Done");
            yield return new WaitForSeconds(5);
        }
    }

    void Test()
    {
        int oldW = wct.width;
        int oldH = wct.height;
        Texture2D photo = new Texture2D(oldW, oldH, TextureFormat.ARGB32, false);
        photo.SetPixels(0, 0, oldW, oldH, wct.GetPixels());
        photo.Apply();
        int newW = 640;
        int newH = 360;
        TextureScale.Bilinear(photo, newW, newH);
        byte[] _bytes = photo.EncodeToPNG();
        File.WriteAllBytes("Assets/Images/out_cam.png", _bytes);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
