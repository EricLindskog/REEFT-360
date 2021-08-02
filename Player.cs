using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Video;

public class Player : MonoBehaviour
{
    public float lastUpdate;
    public float lastDownloadTime;
    public StreamReader trace;
    public StreamReader chunkTrace;
    public DownloadManager downloadManager;
    public BandwidthEstimator estimator;
    public VideoPlayer videoPlayer;
    public Requester requester;
    public Focus lastFocus;
    public Focus currentFocus;
    private int frame;
    void Awake () {
        
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate =  30;
        Debug.Log(Application.targetFrameRate);
    }
    void Start()
    {
        frame=0;
        estimator = new SimpleEstimator(0.3f,2000);
        trace = new StreamReader(Globals.networkTracePath);
        StreamReader trace2 = new StreamReader(Globals.networkTracePath);
        if (videoPlayer == null) Debug.Log("player is null");
        videoPlayer.frame = 0;
        videoPlayer.playbackSpeed = 1F;
        videoPlayer.EnableAudioTrack(0, false);
        lastUpdate = Time.time;
        lastDownloadTime = Time.time;
        Debug.Log("Player running in: "+Globals.playerMode.ToString());
        switch (Globals.playerMode)
        {
            case PlayerMode.Baseline:
                Camera.main.gameObject.AddComponent(typeof(HeadmovementSimulator));
                requester = new ConstantRequester(estimator, Globals.tilesX,
                    Globals.tilesY, Globals.chunkLength, Quality.Full);
                downloadManager = new SimpleDownloadManager(estimator, trace, 20000f);

                break;
            case PlayerMode.Eval:
                Camera.main.gameObject.AddComponent(typeof(HeadmovementLogger));
                Camera.main.gameObject.AddComponent(typeof(ChunkLogger));
                Camera.main.gameObject.AddComponent(typeof(PlaybackLogger));
                Camera.main.gameObject.AddComponent(typeof(ViewPortQualityLogger));
                Camera.main.gameObject.AddComponent(typeof(TileFocusLogger));
                requester = new WaterfillRequester(estimator, Globals.tilesX, Globals.tilesY, Globals.chunkLength, 200);
                downloadManager = new SimpleDownloadManager(estimator, trace, 2.5f);
                break;
            case PlayerMode.Testing:
                Camera.main.gameObject.AddComponent(typeof(HeadmovementSimulator));
                Camera.main.gameObject.AddComponent(typeof(ChunkLogger));
                Camera.main.gameObject.AddComponent(typeof(PlaybackLogger));
                
                Camera.main.gameObject.AddComponent(typeof(TileFocusLogger));
                Camera.main.gameObject.AddComponent(typeof(ViewPortQualityLogger));
                

                requester = new WaterfillRequester(estimator, Globals.tilesX, Globals.tilesY, Globals.chunkLength, 200);
                downloadManager = new SimpleDownloadManager(estimator, trace, 2.5f);
                break;
            case PlayerMode.Recording:
                Camera.main.gameObject.AddComponent(typeof(HeadmovementSimulator));
                chunkTrace = new StreamReader(Globals.chunkTracePath);
                requester = new TraceRequester(estimator, Globals.tilesX, Globals.tilesY, Globals.chunkLength, chunkTrace);
                downloadManager = new SimpleDownloadManager(estimator, trace, 20000f);
                break;
            default:
                throw new System.ArgumentException("Must be of a valid value (0, 1, 2)", "playerMode");

        };
        ChunkData req = new ChunkData(Globals.tilesX, Globals.tilesY, 0, Globals.chunkLength);
        for (int i = 0; i < Globals.tilesY; i++)
            for (int j = 0; j < Globals.tilesX; j++)
                req.tileQuality[j + i * Globals.tilesX] = Quality.None;


        req.size = (int)(QualityMethods.CalcSize(req) * Globals.chunkLength);
        Globals.currentChunk = req;
        lastFocus = Globals.GetFocus();
        currentFocus = lastFocus;
    }
    
    // Update is called once per frame
    void Update()
    {
        int locFrame = frame;
        frame++;
        ManageRequests();
        ManageDownload();
        ManagePlayback();
    }
    /// <summary>
    /// Stops and starts playback when applicable
    /// </summary>
    void ManagePlayback()
    {
        if (Globals.currentChunk == null)
        {
            if (Globals.segNrBuffer.Count==0)
            {
                if (videoPlayer.isPaused) return;
                videoPlayer.Pause();

                Debug.Log("pausing");
                return;
            }
            ChunkData next = Globals.dictBuffer[Globals.segNrBuffer.Dequeue()];
            Globals.dictBuffer.Remove(next.segmentNr);
            Globals.currentChunk = next;
            
            videoPlayer.Play();
        }
        if (videoPlayer.time >= Globals.currentChunk.startTime + Globals.currentChunk.duration)
        {
            Globals.currentChunk = null;
        }
        else return;
    }
    void ManageRequests()
    {
        currentFocus = Globals.GetFocus();
        ChunkData request = requester.DoRequest(currentFocus,Globals.segNrBuffer.Count,(Globals.lastRequestedSegment+1)*Globals.chunkLength, Globals.requests.Count);
        lastFocus = currentFocus;
        if (request == null)
            return;

        if (request.segmentNr>Globals.lastRequestedSegment)
            Globals.lastRequestedSegment=request.segmentNr;
        Globals.requests.Enqueue(request);
    }
    
    void ManageDownload()
    {
        if (!downloadManager.IsActive())
        {
            if (Globals.requests.Count == 0) return;
            downloadManager.InitDownload(Globals.requests.Dequeue()); 

        }
        float currTime = Time.time;
        
        lastDownloadTime = currTime;
        if (!downloadManager.IsActive()) return;
        ChunkData chunk = downloadManager.Download();
        if (chunk == null) return;

        if (!Globals.dictBuffer.ContainsKey(chunk.segmentNr))
        {
            Globals.dictBuffer.Add(chunk.segmentNr, chunk);
            Globals.segNrBuffer.Enqueue(chunk.segmentNr);
        }
        else Globals.dictBuffer[chunk.segmentNr].UpgradeTileQualities(chunk.tileQuality);
        
    }

}