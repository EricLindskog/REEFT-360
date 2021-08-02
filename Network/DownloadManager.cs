using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
public abstract class DownloadManager
{
    
    protected ChunkData chunk;
    public abstract void InitDownload(ChunkData chunk);
    public abstract ChunkData Download();
    public bool IsActive()
    {
        return (chunk != null) ? true : false;
    }
}
public class SimpleDownloadManager : DownloadManager
{
    private int toDownload;
    private int overflowData;
    private float overflowTime;
    private float startTime;
    private float downloadTime;
    private float lastDownloadEnd;
    private BandwidthEstimator estimator;
    private StreamReader trace;
    private float initTime;
    private int lineCount;
    private float scaling;
    public SimpleDownloadManager(BandwidthEstimator estimator, StreamReader trace, float scaling)
    {
        this.scaling = scaling;
        lineCount = 0;
        this.estimator = estimator;
        this.trace = trace;
        overflowTime = 0;
        overflowData = 0;
        lastDownloadEnd = Time.time;
        initTime = lastDownloadEnd;

    }
    public override void InitDownload(ChunkData chunk)
    {
        // Remove overflow if not applicable and remove wasted trace lines.
        if (Time.time - lastDownloadEnd >= overflowTime)
        {
            overflowTime = 0;
            overflowData = 0;
            int gap = (int)(Time.time - lineCount);
            for (int i = 0; i < gap; i++) ReadTraceFile();
        }
        // Adjust overflow
        else
        {
            float tmp = overflowTime;
            overflowTime -= (Time.time - lastDownloadEnd);
            overflowData = (int)(overflowData*(overflowTime / tmp));
        }
        this.chunk = chunk;
        toDownload = chunk.size;
        startTime = Time.time;
        downloadTime = 0;
        int lastBandwidth = 0;
        // Nothing to download
        if (toDownload <= 0)
        {
            downloadTime = 0;
            return;
        }
        //Use overflow
        if (overflowData > 0)
        {
            toDownload -= overflowData;
            downloadTime += overflowTime;
            if (toDownload <= 0)
            {
                overflowTime = Math.Abs((float)toDownload / (float)overflowData)*overflowTime;
                overflowData = Math.Abs(toDownload);
                downloadTime -= overflowTime;
                return;
            }
        }

        //Use upcoming time
        while (toDownload > 0)
        {
            lastBandwidth = ReadTraceFile();
            toDownload -= lastBandwidth;
            downloadTime += 1;
        }
        overflowTime = Math.Abs((float)toDownload / lastBandwidth);
        downloadTime -= overflowTime;
        overflowData = Math.Abs(toDownload);
        lastDownloadEnd = startTime + downloadTime;
    }
    public override ChunkData Download()
    {
        float currTime = Time.time;
        if (currTime - startTime > downloadTime && chunk!=null)
        {
            if(chunk.size!=0)
            {
                estimator.setLastBandwidth((int)(chunk.size / downloadTime));
            }
            ChunkData ret = chunk;
            chunk = null;
            return ret;
        }
        return null;
    }
    public void Reset()
    {
        chunk = null;
        downloadTime = 0;
    }
  
    public int GetOverflowData()
    {
        return overflowData;
    }
    public float GetOverflowTime()
    {
        return overflowTime;
    }
    protected virtual int ReadTraceFile()
    {
        string line = trace.ReadLine();
        if (line != null)
        {
            lineCount++;
            return Convert.ToInt32(System.Int32.Parse(line)*scaling);
        }
        else return 0;
    }
}