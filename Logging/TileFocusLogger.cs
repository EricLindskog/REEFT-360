using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

//Saves the quality of all tiles and the tile in focus 
public class TileFocusLogger : MonoBehaviour
{
    // Start is called before the first frame update
    StreamWriter writer;
    private VideoPlayer videoPlayer;
    string path;
    void Start()
    {
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        path = Globals.tileFocusTracePath;
        writer = new StreamWriter(path, false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (videoPlayer.isPaused) return;
        if (Globals.currentChunk == null) return;
        Focus f = Globals.GetFocus();
        int tileX = f.tilex+f.tiley*Globals.tilesX;
    
        string line = (f.tilex+f.tiley*Globals.tilesX).ToString();
        for (int i = 0; i < Globals.tilesY; i++)
            for (int j = 0; j < Globals.tilesX; j++)
                line += (" " + (int)Globals.currentChunk.tileQuality[i * Globals.tilesX + j]);
        writer.WriteLine(line);
    }
    void OnDestroy()
    {
        writer.Close();
    }
}
