using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class HeadmovementSimulator : MonoBehaviour
{

    public StreamReader trace;
    private VideoPlayer videoPlayer;
    // Start is called before the first frame update
    void Start()
    {

        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        trace = new StreamReader(Globals.headTracePath);
        trace.ReadLine();
        string line = trace.ReadLine();
        string[] traceLine = line.Split(' ');
        float pitch = float.Parse(traceLine[2]);
        float yaw = float.Parse(traceLine[1]);
        float roll = float.Parse(traceLine[3]);
        /*float w = float.Parse(traceLine[1]);
        float x = float.Parse(traceLine[2]);
        float y = float.Parse(traceLine[3]);
        float z = float.Parse(traceLine[4]);
        Camera.main.transform.rotation = new Quaternion(x,y,z,w);*/
        transform.eulerAngles = new Vector3(pitch, yaw, roll);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (videoPlayer.isPaused) return;
        string line = trace.ReadLine();
        if (line == null) return;
        
        string[] traceLine = line.Split(' ');
        float pitch = float.Parse(traceLine[2]);
        float yaw = float.Parse(traceLine[1]);
        float roll = float.Parse(traceLine[3]);
        
        float time = float.Parse(traceLine[0]);
        /*float w = float.Parse(traceLine[1]);
        float x = float.Parse(traceLine[2]);
        float y = float.Parse(traceLine[3]);
        float z = float.Parse(traceLine[4]);*/
        
        //Camera.main.transform.rotation = new Quaternion(x,y,z,w);
        //Debug.Log("POST time: "+ +"x: "+transform.rotation.x+" y: "+transform.rotation.y+" z: "+transform.rotation.z+" w: "+transform.rotation.w);
        Camera.main.transform.eulerAngles = new Vector3(pitch, yaw, roll);
    }
}
