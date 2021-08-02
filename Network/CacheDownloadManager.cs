using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
public class CacheDownloadManager :  DownloadManager
{
    private int toDownload;
    private int overflowDataProxy;
    private int overflowDataServer;
    private float overflowTime;
    private float startTime;
    private float downloadTime;
    private float lastDownloadEnd;
    private BandwidthEstimator estimator;
    private StreamReader serverTrace;
    private StreamReader proxyTrace;
    private float initTime;
    private int lineCount;
    private float scalingP;
    private float scalingS;
    private int nTiles;
    int[, ,] cache;    
    string path = Globals.cacheLogPath;
    StreamWriter writer;
    
    public CacheDownloadManager(BandwidthEstimator estimator, StreamReader proxyTrace, StreamReader serverTrace, int preloadCount, float scalingP, float scalingS)
    {
        
        this.scalingP = scalingP;
        this.scalingS = scalingS;
        lineCount = 0;
        this.estimator = estimator;
        this.serverTrace = serverTrace;
        this.proxyTrace = proxyTrace;
        overflowTime = 0;
        overflowDataProxy = 0;
        lastDownloadEnd = Time.time;
        initTime = lastDownloadEnd;
        this.nTiles = Globals.tilesX*Globals.tilesY;
        cache = new int[300,Globals.tilesX*Globals.tilesY,Enum.GetNames(typeof(Quality)).Length];
        preloadCache(preloadCount);
        
        writer = new StreamWriter(path,false);
        string header = "serverDl proxyDl nHits nTiles";
        writer.WriteLine(header);
        writer.Close();

    }
    public void preloadCache(int preloadCount)
    {
        //Add each file from cacheTraces to cache depending on preloadCount. 
        float startT = Time.time;
        for(int i = 1; i <= preloadCount; i++){
            string path = Globals.cachePreloadsPath + i.ToString()+".txt";
            StreamReader trace = new StreamReader(path);
            
            while(trace.Peek()>=0){
                string line = trace.ReadLine();
                string[] data = line.Split(' ');
                int chunkNr = Convert.ToInt32(data[0]);
                for(int j = 1; j < data.Length; j++){
                    int tileNr = j-1;
                    int qualLvl = Convert.ToInt32(data[j]);
                    cache[chunkNr,tileNr,qualLvl]=1;
                }
            }
        }
    }
    public override void InitDownload(ChunkData chunk)
    {
        Debug.Log("INIT, line count: "+lineCount+" Time: "+Time.time);
        AdjustOverflow();
        this.chunk = chunk;
        toDownload = chunk.size;

        startTime = Time.time;
        downloadTime = 0;
        int lastBandwidth = 0;
        int[] hits = getCacheHits(chunk);
        int serverDl = 0;
        int proxyDl = 0;
        int nHits = 0;
        for(int i = 0; i < nTiles; i++){
            int dl = chunk.tileQuality[i].GetFullScreenBitrate()/nTiles;
            if(hits[i]==1){
                proxyDl += dl;
                nHits++;
            }
            else{
                serverDl += dl;
            }
        }
        // Nothing to download
        if (toDownload <= 0)return;
        Debug.Log("sdl: "+serverDl+" pdl: "+proxyDl);
        logCacheMetrics(serverDl, proxyDl, nHits);
        //Use overflow
        if(overflowTime > 0)
        {
            Debug.Log("OF left: "+overflowTime);
            int bwPerTileProxy = 0;
            int bwPerTileServer = 0;
            int totalProxyDl = 0;
            if(serverDl > 0 && proxyDl > 0){
                float bwFrac = 1f;
                if(overflowDataProxy<=overflowDataServer){
                    bwPerTileProxy = overflowDataProxy/nTiles;
                    bwPerTileServer = overflowDataProxy/nTiles;
                }
                else{
                    bwFrac = overflowDataServer/overflowDataProxy;
                    bwPerTileProxy = overflowDataServer/nTiles + (overflowDataProxy-overflowDataServer)/nHits;
                    bwPerTileServer = overflowDataServer/nTiles;
                }
                serverDl-=bwPerTileServer*(nTiles-nHits);
                proxyDl-=bwPerTileProxy*nHits;
                
                
                downloadTime+=overflowTime;
                if(serverDl<0 && proxyDl>0){
                    proxyDl+=serverDl;
                    serverDl=0;
                    overflowTime=Math.Abs((float)proxyDl/ (float)overflowDataServer)*overflowTime;
                }
                else if(proxyDl<0 && serverDl>0){
                    serverDl+=(int)(proxyDl*bwFrac);
                    proxyDl=0;
                    overflowTime=Math.Abs((float)serverDl/ (float)overflowDataProxy)*overflowTime;
                }
                else{
                    float otp= (float)proxyDl/(float)overflowDataProxy*overflowTime;
                    float ots= (float)serverDl/(float)overflowDataServer*overflowTime;
                    overflowTime = Math.Abs(Math.Min(otp,ots));
                }
            }
            else if(serverDl <= 0 && proxyDl > 0){
                proxyDl-=overflowDataProxy;
                downloadTime+=overflowTime;
                overflowTime = Math.Abs((float)proxyDl/ (float)overflowDataProxy)*overflowTime;
            }
            else if(proxyDl <= 0 && serverDl > 0){
                serverDl-=Math.Min(overflowDataProxy,overflowDataServer);
                downloadTime+=overflowTime;
                overflowTime = Math.Abs((float)serverDl/ (float)Math.Min(overflowDataProxy,overflowDataServer))*overflowTime;
            }
            else{
                Debug.Log("should not occurr");
            }
            if(serverDl <= 0 && proxyDl <= 0){
                //overflowTime should be the same for both.

                overflowDataProxy = Math.Abs(proxyDl);
                overflowDataServer = Math.Abs(serverDl);
                
                downloadTime-=overflowTime;
                lastDownloadEnd = startTime + downloadTime;
                Debug.Log("OF - DT: "+downloadTime+" OT: "+overflowTime+" OFDP: "+overflowDataProxy+" OFDS: "+overflowDataServer);
                Debug.Log("only used OT");
                return;
            }
            else{
                overflowTime=0;
            }
        }
        Debug.Log("Used up overflow");
        int lastProxyBW = overflowDataProxy;
        int lastServerBW = overflowDataServer;
        //Use upcoming time
        overflowTime = 0;
        while(serverDl > 0 || proxyDl > 0)
        {
            
            downloadTime+=1;
            lastProxyBW = ReadProxyTraceFile();
            lastServerBW = ReadServerTraceFile();
            //Debug.Log("bw diff: "+(lastProxyBW-lastServerBW));
            if(serverDl > 0 && proxyDl > 0){
                Debug.Log("Both have left");
                float bwFrac = 1;
                if(lastProxyBW<=lastServerBW){
                    serverDl-=(nTiles-nHits)*lastProxyBW/nTiles;
                    proxyDl-=nHits*lastProxyBW/nTiles;
                }
                else{
                    serverDl-=(lastServerBW/nTiles)*(nTiles-nHits);
                    proxyDl-= (lastServerBW/nTiles)*nHits;
                    bwFrac = lastServerBW/lastProxyBW;
                    if(nHits>0){
                        proxyDl-= (lastProxyBW-lastServerBW);
                    }
                }
                //Debug.Log("serverDl: "+serverDl+" proxyDl: "+proxyDl);
                if(serverDl<=0 && proxyDl<=0){
                    float ots =  Math.Abs((float)serverDl / lastServerBW);
                    float otp =  Math.Abs((float)proxyDl / lastProxyBW);
                    overflowTime = Math.Min(ots,otp);
                    overflowDataProxy = (int)(overflowTime*lastProxyBW);
                    overflowDataServer = (int)(overflowTime*lastServerBW);
                    //Debug.Log("both less than");
                    Debug.Log("Done at the same time");
                    break;
                }
                else if(serverDl<=0){
                    proxyDl-= Math.Abs(serverDl);
                    serverDl=0;
                    Debug.Log("Server done");
                    if(proxyDl<=0){
                        overflowTime = Math.Abs((float)proxyDl / lastProxyBW);
                        overflowDataProxy = (int)(overflowTime*lastProxyBW);
                        overflowDataServer = (int)(overflowTime*lastServerBW);
                        Debug.Log("proxy now done.");
                        break;
                    }
                }
                else if(proxyDl<=0){
                    serverDl-= (int) (Math.Abs(proxyDl)*bwFrac);
                    proxyDl=0;
                    Debug.Log("Proxy done");
                    if(serverDl<=0){
                        overflowTime = Math.Abs((float)serverDl / lastServerBW);
                        overflowDataProxy = (int)(overflowTime*lastProxyBW);
                        overflowDataServer = (int)(overflowTime*lastServerBW);
                        Debug.Log("server now done.");
                        break;
                    }
                }
            }
            else if(serverDl <= 0 && proxyDl > 0){
                Debug.Log("only proxy have left");
                proxyDl-=lastProxyBW;
                if(serverDl<0){
                    //proxyDl-= Math.Abs(serverDl);
                    serverDl=0;
                }
                if(proxyDl<=0){
                    overflowTime = Math.Abs((float)proxyDl / lastProxyBW);
                    overflowDataProxy = (int)(overflowTime*lastProxyBW);
                    overflowDataServer = (int)(overflowTime*lastServerBW);
                    Debug.Log("proxy done last");
                    break;
                }
                //serverDl-=lastProxyBW; //This is to guarantee correct overflow time
            }
            else if(proxyDl <= 0 && serverDl > 0){
                Debug.Log("only server have left");
                serverDl -= Math.Min(lastProxyBW,lastServerBW);
                if(proxyDl<0){
                    //serverDl-= Math.Abs(serverDl);
                    //serverDl-= (int) (Math.Abs(proxyDl)*bwFrac);
                    proxyDl=0;
                }
                if(serverDl<=0){
                    overflowTime = Math.Abs((float)serverDl / lastServerBW);
                    //Debug.Log("server done last");
                    overflowDataProxy = (int)(overflowTime*lastProxyBW);
                    overflowDataServer = (int)(overflowTime*lastServerBW);
                    Debug.Log("server done last");
                    break;
                }
                //proxyDl -= Math.Min(lastProxyBW,lastServerBW); //This is to guarantee correct overflow time
            }
            else{
                Debug.Log("Should never occurr");
            }
            Debug.Log("iteration");
        }
        
        //float ots =  Math.Abs((float)serverDl / lastServerBW);
        //float otp =  Math.Abs((float)proxyDl / lastProxyBW);
        //overflowTime = Math.Min(ots,otp);
        //overflowDataProxy = Convert.ToInt32(overflowTime*lastProxyBW);
        //overflowDataServer = Convert.ToInt32(overflowTime*lastServerBW);
        downloadTime-=overflowTime;
    
        lastDownloadEnd = startTime + downloadTime;
        Debug.Log("DT: "+downloadTime+" OT: "+overflowTime+" OFDP: "+overflowDataProxy+" OFDS: "+overflowDataServer);
    }
    private int[] getCacheHits(ChunkData chunk){
        int[] ret = new int[this.nTiles];
        for(int i = 0; i < this.nTiles; i++){
            int reqQual = (int)chunk.tileQuality[i];
            int available = cache[chunk.segmentNr,i,reqQual];
            ret[i]=available;
        }
        return ret;
    }
    private void AdjustOverflow(){
        // Remove overflow if not applicable and remove wasted trace lines.
        if (Time.time - lastDownloadEnd >= overflowTime)
        {
            overflowTime = 0;
            overflowDataProxy = 0;
            overflowDataServer = 0;
            int gap = (int)(Time.time - lineCount);
            for (int i = 0; i < gap; i++) {
                ReadProxyTraceFile();
                ReadServerTraceFile();
            }
        }
        // Adjust overflow
        else
        {
            float tmp = overflowTime;
            overflowTime -= (Time.time - lastDownloadEnd);
            overflowDataProxy = Convert.ToInt32(overflowDataProxy*(overflowTime / tmp));
            overflowDataServer = Convert.ToInt32(overflowDataServer*(overflowTime / tmp));
        }
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
    int ReadProxyTraceFile()
    {
        string line = proxyTrace.ReadLine();
        if (line != null)
        {
            lineCount++;
            return Convert.ToInt32(System.Int32.Parse(line)*scalingP);
        }
        else return 0;
    }
    int ReadServerTraceFile()
    {
        string line = serverTrace.ReadLine();
        if (line != null)
        {
            return Convert.ToInt32(System.Int32.Parse(line)*scalingS);
        }
        else return 0;
    }
    void logCacheMetrics(int serverDL, int proxyDl, int nHits)
    {
        string line = serverDL + " " + proxyDl + " " + nHits +" "+ nTiles;
        writer = new StreamWriter(path,true);
        writer.WriteLine(line);
        writer.Close();
    }
}