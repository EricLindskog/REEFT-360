using UnityEngine;
using System;
using System.IO;
public class ConstantRequester : Requester
{
    Quality qual;
    public ConstantRequester(BandwidthEstimator estimator, int tilesX, int tilesY, double chunkLength, Quality qual) : base(estimator, tilesX, tilesY, chunkLength)
    {
        this.qual = qual;
    }
    public override ChunkData DoRequest(Focus focus, int bufferLength, double chunkStart, int numRequests)
    {
        if (numRequests > 1 || bufferLength > 2) return null;
        int bandwidthEst = estimator.getBandwidthEstimate();
        ChunkData req = new ChunkData(tilesX, tilesY, chunkStart, chunkLength);
        for (int i = 0; i < tilesY; i++)
            for (int j = 0; j < tilesX; j++)
                qualityArr[j + i * tilesX] = qual;

        req.SetTileQualities(qualityArr);
        req.size = (int)(QualityMethods.CalcSize(req) * chunkLength);
        return req;
    }
}