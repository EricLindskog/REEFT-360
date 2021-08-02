using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.XR;
using UnityEngine.Video;
//using UnityEngine.VR;

public class PreProcess : MonoBehaviour
{
    public ComputeShader shader;
    public RenderTexture res;
    public RenderTexture res2;
    public RenderTexture input;
    public Player player;
    Texture2D tex;
    public Material skybox;
    public int height;
    public int width;
    public int tilesX;
    public int tilesY;
    public int lastChunkhash;
    public int[] qualityArr;
    public int[] defaultQuality;
    public int frame;
    public ComputeBuffer tiles;
    public ComputeBuffer blocksizeMap;
    public ComputeBuffer thresholdMap;
    private VideoPlayer videoPlayer;
    public int kernelHandle;
    private long lastFrame;


    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        lastFrame = videoPlayer.frame;
        tilesX = Globals.tilesX;
        tilesY = Globals.tilesY;
        frame = 0;
        qualityArr = new int[tilesX * tilesY];
        defaultQuality = new int[tilesX * tilesY];
        for (int i = 0; i < tilesX * tilesY; i++)
        {
            qualityArr[i] = 0;
            defaultQuality[i] = 0;
        }
        player = GameObject.Find("Video Player").GetComponent<Player>();
        int numQualities = Enum.GetNames(typeof(Quality)).Length;
        int[] blocksizes = new int[numQualities];
        int[] thresholds = new int[numQualities];
        blocksizeMap = new ComputeBuffer(numQualities,4);
        thresholdMap = new ComputeBuffer(numQualities,4);
        for(int i = 0; i < numQualities; i++)
        {
            blocksizes[i] = ((Quality)i).GetBlockSize();
            thresholds[i] = ((Quality)i).GetThreshold();
        }
        blocksizeMap.SetData(blocksizes);
        thresholdMap.SetData(thresholds);
        tiles = new ComputeBuffer(tilesX * tilesY, 4);
        tiles.SetData(qualityArr);
        skybox = Resources.Load("360-video-emulator/skybox", typeof(Material)) as Material;
        if (skybox is null) Debug.Log("skybox is null");
        input = Resources.Load("360-video-emulator/video", typeof(RenderTexture)) as RenderTexture;
        height = input.height;
        width = input.width;
        
        
        res = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        res.enableRandomWrite = true;
        res.Create();
        
        res2 = Resources.Load("360-video-emulator/res", typeof(RenderTexture)) as RenderTexture;
       
        tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        
        skybox.mainTexture = res;
        
        kernelHandle = shader.FindKernel("Blockify");
    }
    void FixedUpdate()
    {
        UpdateQuality();
        SetAndDispatch();
    }
    //TODO this should only be called on a change of current chunk
    void UpdateQuality()
    {
        if (Globals.currentChunk == null)
        {
            qualityArr = defaultQuality;
            return;
        }
        else if (Globals.currentChunk.GetHashCode() == lastChunkhash) return;

        for (int i = 0; i < qualityArr.Length; i++)
        {
            qualityArr[i] = (int)Globals.currentChunk.tileQuality[i];
        }
    }

    void SetAndDispatch()
    {
        if(lastFrame==videoPlayer.frame)
            return;
        
        lastFrame=videoPlayer.frame;
        tiles.SetData(qualityArr);
        shader.SetInt("drawTileBorders", 0);
        shader.SetInt("width", width);
        shader.SetInt("height", height);
        shader.SetInt("numTilesX", tilesX);
        shader.SetInt("numTilesY", tilesY);
        //shader.SetFloat("threshold", compressionThreshold);
        shader.SetInt("frame", frame); //Compression is of. This needs to be updated every time you want an iFrame
        shader.SetBuffer(kernelHandle, "blocksizeMap", blocksizeMap);
        shader.SetBuffer(kernelHandle, "thresholdMap", thresholdMap);
        shader.SetTexture(kernelHandle, "Result", res);
        shader.SetTexture(kernelHandle, "ImageInput", input);
        shader.SetBuffer(kernelHandle, "tileMatrix", tiles);
        shader.Dispatch(kernelHandle, width / 8, height / 8, 1);
        frame++;
        if (frame > Globals.iFrameDistance) frame = 0;
    }
    void PrintPix(String msg, int pixX, int pixY)
    {
        RenderTexture.active = res;
        tex.ReadPixels(new Rect(0, 0, res.width, res.height), 0, 0);
        tex.Apply();

        Debug.Log(msg +" : "+ tex.GetPixel(pixX, pixY));
        RenderTexture.active = null;
    }
}
