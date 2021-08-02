using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Uses a trace of chunk data to request. Used to replicate image quality.
/// Make sure no settings are changed when using this.
/// </summary>
public class TraceRequester : Requester
{
    private StreamReader trace;
    private int lastSegNr;
    public TraceRequester(BandwidthEstimator estimator, int tilesX, int tilesY, double chunkLength, StreamReader trace) : base(estimator, tilesX, tilesY, chunkLength)
    {
        this.trace = trace;
        lastSegNr = -1;
    }
    public override ChunkData DoRequest(Focus focus, int bufferLength, double chunkStart, int numRequests)
    {
        if (numRequests > 3 || bufferLength > 4) return null;
        if(lastSegNr>=chunkStart/chunkLength) return null;
        lastSegNr = Convert.ToInt32(chunkStart / chunkLength);
        string line = trace.ReadLine();
        string[] data = line.Split(' ');
        ChunkData req = new ChunkData(tilesX, tilesY, chunkStart, chunkLength);
        for(int i = 0; i < data.Length - 1; i++)
        {
            qualityArr[i] = (Quality)System.Int32.Parse(data[i + 1]);
        }
        req.SetTileQualities(qualityArr);
        req.size = (int)(QualityMethods.CalcSize(req) * chunkLength);
        return req;
    }
}
