using UnityEngine;
using System;
using System.IO;
using UnityEngine.Video;

/// <summary>
/// Implementation of waterfill requester. Only works with tilesY = 0;
/// </summary>
public class WaterfillRequester : Requester
{
    public VideoPlayer videoPlayer;
    private int incentive;
    public WaterfillRequester(BandwidthEstimator estimator, int tilesX, int tilesY, double chunkLength, int incentive) : base(estimator, tilesX, tilesY, chunkLength)
    { 
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        this.incentive = incentive;
    }
    public override ChunkData DoRequest(Focus focus, int bufferLength, double chunkStart, int numRequests)
    {
        if (numRequests + bufferLength > Globals.maxBuffer) return null;
        ChunkData req = new ChunkData(tilesX,tilesY,chunkStart,chunkLength);
        double timeLeft = chunkStart*chunkLength - videoPlayer.time;
        //Debug.Log(timeLeft);
        timeLeft = timeLeft%chunkLength;
        //Debug.Log(timeLeft);
        if(timeLeft==0)timeLeft=1;
        
        //Debug.Log(timeLeft);
        double bwMultiplier = timeLeft/chunkLength;
        //Debug.Log("bwm:"+bwMultiplier);
        int bandwidthEst = (int)(estimator.getBandwidthEstimate()*bwMultiplier);
        //Debug.Log("bwe: "+bandwidthEst+" mincost: "+ Quality.Low.GetFullScreenBitrate()/8);
        int currCost = (int)(QualityMethods.CalcSize(req) * chunkLength);
        qualityArr = new Quality[tilesX];
        float degPerTileX = 360f / Globals.tilesX;
        //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        //stopWatch.Start();

        while(minUpgradeCost(qualityArr) + currCost < bandwidthEst)
        {
            
            int upgradeI = -1;
            float upgradeU = 0;
            for(int i = 0; i < qualityArr.Length; i++ )
            {
                int upgradeCost = getUpgradeCost(qualityArr[i]);
                
                if(qualityArr[i]==Quality.Full)continue;
                
                if(upgradeCost + currCost < bandwidthEst)
                {
                    
                    float focusLat = focus.lat;
                    float tileLat = i*degPerTileX+degPerTileX/2;
                    float distLat = Math.Abs(focusLat-tileLat);
                    distLat = Math.Min(distLat,Math.Abs(360f-distLat));
                    int u = upgradeCost;
                    if(qualityArr[i]==Quality.None) u += incentive;
                    float w = 2-distLat/180f;
                    if(upgradeU < w*u/upgradeCost)
                    {
                        upgradeU = w*u/upgradeCost;
                        upgradeI = i;
                    }
                }
            }
            if(upgradeI==-1)continue;
            qualityArr[upgradeI] = qualityArr[upgradeI].GetNextQuality();
            currCost = (int)(QualityMethods.CalcSize(qualityArr));
        }
        
        //stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        //TimeSpan ts = stopWatch.Elapsed;
        
        req.SetTileQualities(qualityArr);
        req.size = (int)(QualityMethods.CalcSize(req) * chunkLength);
        //Debug.Log("reqsize:"+req.size+" unused: "+(bandwidthEst-req.size));
        //if(req.size==0)return null;
        return req;
    }
    private int getUpgradeCost(Quality q)
    {
        if(q==Quality.Full)return 999999;
        int locCost = q.GetFullScreenBitrate()/(tilesX);
        int upgradeCost =  q.GetNextQuality().GetFullScreenBitrate()/(tilesX) - locCost;
        return upgradeCost;        
    }
    private int minUpgradeCost(Quality[] quals)
    {
       
        int mDq = 999999;
        for(int i = 0; i < quals.Length; i++)
        {
            if(quals[i]!=Quality.Full)
            {
                int upgradeCost = getUpgradeCost(quals[i]);
                if(upgradeCost<mDq) mDq = upgradeCost;
            }
        }
        return mDq;
    }

}