using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BandwidthEstimator
{
    protected int lastBandwidth;
    protected int oldEstimate;
    protected float weight;
    public BandwidthEstimator(float inWeight)
    {
        weight = inWeight;
        oldEstimate = 0;
        lastBandwidth = 0;
    }
    public abstract void setLastBandwidth(int lastDownloadBandwidth);
    public abstract int getBandwidthEstimate();
}

public class SimpleEstimator : BandwidthEstimator
{
    public SimpleEstimator(float inWeight, int initBW) : base(inWeight)
    {
        oldEstimate = initBW;
        lastBandwidth = initBW;
    } 
    public override int getBandwidthEstimate()
    {
        int est = (int)(oldEstimate * (1-weight) + (weight) * lastBandwidth);
        oldEstimate = est;
        return est;
    }

    public override void setLastBandwidth(int lastDownloadBandwidth)
    {
        lastBandwidth = lastDownloadBandwidth;
    }
}