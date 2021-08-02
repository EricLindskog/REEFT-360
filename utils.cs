using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public struct Int2
{
    public int x;
    public int y;
    public Int2(int p1, int p2)
    {
        x = p1;
        y = p2;
    }
}
public struct float2
{
    public float x;
    public float y;
    public float2(float x, float y)
    {
        this.x=x;
        this.y=y;
    }
}
public struct Focus
{
    public int tilex;
    public int tiley;
    public float lat;
    public float lng;
    public Focus(int tilex, int tiley, float lat, float lng)
    {
        this.tilex = tilex;
        this.tiley = tiley;
        this.lat = lat;
        this.lng = lng;
    }
}
static class Globals
{
    public static PlayerMode playerMode = PlayerMode.Eval;
    public static int tilesX = 8;
    public static int tilesY = 1;
    public static double chunkLength = 1;
    public static int iFrameDistance = 2;
    public static int maxBuffer = 2;
    public static float2 fov = new float2(100,80);
    public static ChunkData currentChunk;
    public static Queue<int> segNrBuffer = new Queue<int>();
    public static Queue<ChunkData> requests = new Queue<ChunkData>();
    public static Dictionary<int, ChunkData> dictBuffer = new Dictionary<int, ChunkData>();
    public static int lastRequestedSegment = -1;
    public static string headTracePath = "D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/traces/headmovementTrace.txt";
    public static string tileFocusTracePath = "D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/traces/tileFocusTrace.txt";
    public static string networkTracePath = "D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/traces/networkTrace.txt";
    public static string chunkTracePath = "D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/traces/chunkTrace.txt";
    public static string playbackLogPath ="D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/traces/playbackLog.txt";
    public static string viewportQualityPath ="D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/traces/viewPortQuality.txt";
    public static string cacheHeadTracePath = "D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/cacheTraces/headmovementTrace.txt";
    public static string cachePreloadsPath = "D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/cacheTraces/";
    public static string cacheLogPath = "D:/Exjobb-grejer/Default/Assets/Resources/360-video-emulator/traces/cacheLogTrace.txt";
    public static Focus GetFocus()
    {
        Focus pos;
        //Vector3 euler = InputTracking.GetLocalRotation(XRNode.CenterEye).eulerAngles;  //
        Vector3 euler = Camera.main.transform.eulerAngles;

        float degPerTileX = 360f / tilesX;
        int tileX = Convert.ToInt32(Math.Floor(euler.y / degPerTileX));
        float rotY = 0;
        if (euler.x <= 360f && euler.x >= 270f) rotY = euler.x - 270f;
        else if (euler.x >= 0 && euler.x <= 90f) rotY = euler.x + 90f;
        rotY = 180f - rotY;
        float degPerTileY = 180f / tilesY;
        int tileY = Convert.ToInt32(Math.Floor(rotY / degPerTileY));
        
        pos.tilex = tileX;
        pos.tiley = tileY;
        pos.lat = euler.y;
        pos.lng = rotY;
        return pos;
    }
    
}

public class Temp
{

    public static Focus GetLngLat(float x, float y)
    {
        float rotY = 0;
        if (x <= 360f && x >= 270f) {
            rotY = x - 270f;
        }
        else if (x>= 0 && x <= 90f) {
            rotY = x + 90f;
        }
        rotY = 180f -rotY;
        return new Focus(0,0,y,rotY);
    }
}
public enum PlayerMode
{
    // This is used as a reference to all other algorithms. 
    // Removes effects from network and plays full quality 
    // as well as records headmovement
    Baseline = 0,
    // This is used to get the playback data as well as the chunk data.
    // It simulates network and uses headmovement data in order to evaluate an algorithm.
    Eval = 1,
    // Uses headmovement data and chunk data and records the video. 
    // It disregards network in order to allow comparison to baseline
    Recording = 2,
    // No recording or logging, just testing from a headtrace and networktrace
    Testing = 3,
}

public enum Quality
{
    None = 0,   //Nothing
    //Trash = 6,
    Low = 1,    //480p 
    Medium = 2, //720p
    High = 3,   //1080p
    Full = 4    //4k
}
static class QualityMethods
{
    /// <summary>
    /// Gets the fullscreen resolution for the given quality level
    /// </summary>
    /// <param name="q">quality level</param>
    /// <returns></returns>
    public static int GetFullScreenBitrate(this Quality q)
    {
        switch (q)
        {
            case Quality.None:
                return 0;
            case Quality.Low:
                return 2500;
            case Quality.Medium:
                return 5000;
            case Quality.High:
                return 8000;
            case Quality.Full:
                return 35000;
            default:
                return 0;
        }
    }
    public static Quality GetNextQuality(this Quality q)
    {
        switch (q)
        {
            case Quality.None:
                return Quality.Low;
            case Quality.Low:
                return Quality.Medium;
            case Quality.Medium:
                return Quality.High;
            case Quality.High:
                return Quality.Full;
            case Quality.Full:
                return Quality.None;
            default:
                return Quality.None;
        }
    }
    public static int GetBlockSize(this Quality q)
    {
        switch (q)
        {
            case Quality.None:
                return 0;
            case Quality.Low:
                return 6;
            case Quality.Medium:
                return 4;
            case Quality.High:
                return 2;
            case Quality.Full:
                return 1;
            default:
                return 0;
        }
    }
    public static int GetThreshold(this Quality q)
    {
        switch (q)
        {
            case Quality.None:
                return 0;
            case Quality.Low:
                return 0;
            case Quality.Medium:
                return 0;
            case Quality.High:
                return 0;
            case Quality.Full:
                return 0;
            default:
                return 0;
        }
    }
    public static int CalcSize(ChunkData data)
    {
        int tmp = 0;
        for (int i = 0; i < data.tileQuality.Length; i++)
        {
            tmp += data.tileQuality[i].GetFullScreenBitrate() / (data.tilesX * data.tilesY);
        }
        return tmp;
    }
    public static int CalcSize(Quality[] data)
    {
        int tmp = 0;
        for (int i = 0; i <data.Length; i++)
        {
            tmp += data[i].GetFullScreenBitrate() / (Globals.tilesX * Globals.tilesY);
        }
        return tmp;
    }
    public static string ToString(this PlayerMode p)
    {
        string[] strs = new string[4] { "Baseline", "Eval", "Recording", "Testing"};

        return strs[(int)p];
    }
}