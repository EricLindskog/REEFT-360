using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class HeadmovementLogger : MonoBehaviour
{
    // Start is called before the first frame update
    StreamWriter writer;
    private VideoPlayer videoPlayer;
    string path;
    void Start()
    {
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        path = Globals.headTracePath;
        writer = new StreamWriter(path, false);
        writer.WriteLine("Time  Yaw  Pitch  Roll");

        //writer.Close();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (videoPlayer.isPaused) return;
        //writer = new StreamWriter(path, true);
        Vector3 angles = Camera.main.transform.eulerAngles;
        string line = (Time.time + 
            " " + Camera.main.transform.rotation.w + 
            " " + Camera.main.transform.rotation.x + 
            " " + Camera.main.transform.rotation.y + 
            " " + Camera.main.transform.rotation.z
        );
        string lineEuler = (Time.time + " " + angles.y + " " + angles.x + " " + angles.z);
        writer.WriteLine(lineEuler);
        //writer.Close();
    }
    void OnDestroy()
    {
        writer.Close();
    }
}
