using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
public class PlaybackLogger : MonoBehaviour
{
    // Start is called before the first frame update
    StreamWriter writer;
    private VideoPlayer videoPlayer;
    string path;
    bool stalled;
    float clockStallStart;
    float videoStallStart;
    void Start()
    {
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        path = Globals.playbackLogPath;
        writer = new StreamWriter(path, false);
        //Put metadata here
        writer.WriteLine("stall-start stall-duration");
        //writer.Close();
        stalled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(videoPlayer.isPaused && !stalled){
            stalled=true;
            clockStallStart = Time.time;
            videoStallStart = (float)videoPlayer.time;
        }
        else if(!videoPlayer.isPaused && stalled) {
            stalled=false;
            //writer = new StreamWriter(path, true);
            string s = "";
            s+=(videoStallStart+" "+(Time.time-clockStallStart));
            
            writer.WriteLine(s);
            //writer.Close();
        }
        
    }
    void OnDestroy()
    {
        writer.Close();
    }
}
