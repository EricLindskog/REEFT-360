using UnityEngine;
using System;
using System.IO;
public class HighestPossibleRequester : Requester
{
    public HighestPossibleRequester(BandwidthEstimator estimator, int tilesX, int tilesY, double chunkLength): base(estimator, tilesX, tilesY, chunkLength)
    {

    }
    public override ChunkData DoRequest(Focus focus, int bufferLength, double chunkStart, int numRequests)
    {
        //PreProcess processing = Camera.main.GetComponent<PreProcess>();
        //Player player = GameObject.Find("Video Player").GetComponent<Player>();
        if (numRequests + bufferLength > Globals.maxBuffer) return null;
        int bandwidthEst = estimator.getBandwidthEstimate();
        Quality reqQual = Quality.Low;
        foreach (Quality qual in Enum.GetValues(typeof(Quality)))
        {
            if (qual.GetFullScreenBitrate() < bandwidthEst && (int)qual > (int)reqQual) reqQual = qual;
        }
        //Debug.Log("est: "+bandwidthEst+" req size: "+reqQual.GetFullScreenBitrate()+" req qual: "+(int)reqQual);
        ChunkData req = new ChunkData(tilesX,tilesY,chunkStart,chunkLength);
        for(int i = 0; i<tilesY;i++)
        {
            for(int j = 0; j < tilesX; j++)
            {
                qualityArr[j + i * tilesX] = reqQual;// qual;
            }
        }
        req.SetTileQualities(qualityArr);
        req.size = (int)(QualityMethods.CalcSize(req) * chunkLength);

        return req;
    }
}


