using UnityEngine;
using System;
using System.IO;
public abstract class Requester
{
    protected int tilesX;
    protected int tilesY;
    protected double chunkLength;
    protected Quality[] qualityArr;
    protected BandwidthEstimator estimator;
    public Requester(BandwidthEstimator estimator, int tilesX, int tilesY, double chunkLength)
    {
        this.estimator = estimator;
        this.tilesX = tilesX;
        this.tilesY = tilesY;
        this.chunkLength = chunkLength;
        this.qualityArr = new Quality[tilesX * tilesY];
    }
    public abstract ChunkData DoRequest(Focus focus,int bufferLength, double chunkStart,int numRequests);
}



