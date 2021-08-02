using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// Logs chunkdata.
/// </summary>
public class ChunkLogger : MonoBehaviour
{
    
    StreamWriter writer;
    string path;
    int lastChunk;
    void Start()
    {
        lastChunk = -1;
        path = Globals.chunkTracePath;
        writer = new StreamWriter(path, false);
        //writer.Close();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Globals.currentChunk == null) return;
        if (lastChunk != Globals.currentChunk.segmentNr)
        {
            lastChunk = Globals.currentChunk.segmentNr;
            //writer = new StreamWriter(path, true);
            string line = Globals.currentChunk.segmentNr.ToString();
            for (int i = 0; i < Globals.tilesY; i++)
                for (int j = 0; j < Globals.tilesX; j++)
                    line += (" " + (int)Globals.currentChunk.tileQuality[i * Globals.tilesX + j]);
            writer.WriteLine(line);
            //writer.Close();
        }
    }
    void OnDestroy()
    {
        writer.Close();
    }
}
