using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
public class CappedDownloadManager : SimpleDownloadManager{
    int bandwidthCap;
    public CappedDownloadManager(BandwidthEstimator estimator, StreamReader trace, float scaling, int bandwidthCap) 
        : base(estimator, trace, scaling)
    {
        this.bandwidthCap=bandwidthCap;
    }
    protected override int ReadTraceFile()
    {
        return Math.Min(base.ReadTraceFile(),this.bandwidthCap);
    }
}