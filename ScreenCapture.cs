using UnityEngine;
using System.Collections;
using System;

public class ScreenCapture : MonoBehaviour
{
    public RenderTexture overviewTexture;
    GameObject OVcamera;
    public string path = "";

    void Start()
    {
        OVcamera = GameObject.FindGameObjectWithTag("OverviewCamera");
    }

    void LateUpdate()
    {
        //if (Input.GetKeyDown("f9"))
        //{
            StartCoroutine(TakeScreenShot());
        //}
    }

    // return file name
    string fileName(int width, int height)
    {
        return string.Format("screen_{0}x{1}_{2}.png",
                              width, height,
                              System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public IEnumerator TakeScreenShot()
    {
        yield return new WaitForEndOfFrame();

        Camera camOV = Camera.main;
        if (camOV.targetTexture == null) Debug.Log("null");
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = camOV.targetTexture;
        camOV.Render();
        Texture2D imageOverview = new Texture2D(camOV.targetTexture.width, camOV.targetTexture.height, TextureFormat.RGB24, false);
        imageOverview.ReadPixels(new Rect(0, 0, camOV.targetTexture.width, camOV.targetTexture.height), 0, 0);
        imageOverview.Apply();
        RenderTexture.active = currentRT;

        // Encode texture into PNG
        byte[] bytes = imageOverview.EncodeToPNG();

        // save in memory
        string filename = fileName(Convert.ToInt32(imageOverview.width), Convert.ToInt32(imageOverview.height));
        path = "D:/Recordings/test1/" + filename;
        System.IO.File.WriteAllBytes(path, bytes);
    }
}