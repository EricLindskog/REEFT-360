using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Video;
public class ViewPortQualityLogger : MonoBehaviour
{
    
    StreamWriter writer;
    private VideoPlayer videoPlayer;
    string path;
    string lastLine;
    // Start is called before the first frame update
    void Start()
    {
        path = Globals.viewportQualityPath;
        writer = new StreamWriter(path, false);
        videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>();
        //writer.Close();
        lastLine="0";
        //Debug.Log(Camera.main.fieldOfView);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        if (videoPlayer.isPaused) return;       
        string line = "";
        line = InView();
        if(line=="")
        {
            line = lastLine;
        }
        else lastLine = line;
        //writer = new StreamWriter(path, true);
        writer.WriteLine(line);
        //writer.Close();
        stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        TimeSpan ts = stopWatch.Elapsed;
        //Debug.Log(ts);
    }
    void OnDestroy()
    {
        writer.Close();
    }
    bool CrossesLngLine(Focus p1, Focus p2, float lngLine)
    {
        bool overlaps = ((p1.lat<=lngLine) && (lngLine<=p2.lat));
        if(p1.lat>p2.lat)
                overlaps = overlaps || (((p1.lat<=lngLine) && (lngLine<=360f)) || ((0<=lngLine) && (lngLine<=p2.lat)));
        return overlaps;
    }
    bool CrossesLatLine(Focus p1, Focus p2, float latLine)
    {
        bool overlaps = (p1.lng<=latLine) && (latLine <= p2.lng);
        //if(p1.lng>=p2.lng)
        //    overlaps = overlaps || ((0<=latLine) && (latLine<=p1.lng) || (0<=latLine) && (latLine<=p2.lng));
        return overlaps;
    }
    Focus ScreenPointToLngLat(Vector3 p)
    {
        Ray ray = Camera.main.ScreenPointToRay(p);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        Vector3 rot = Quaternion.LookRotation(ray.direction).eulerAngles;
        return Temp.GetLngLat(rot.x,rot.y);
    }
    /// <summary>
    /// Gets the latitude of where the line between p1 and p2 intersects the longitudal line at lngLine
    /// </summary>
    /// <param name="lngLine"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    float GetIntersectLatOfLineAndLngLine(float lngLine, Focus p1, Focus p2)
    {
        float inter = 0;
        inter = (p1.lat - (p1.lng-lngLine)*(p1.lat-p2.lat)/(p1.lng-p2.lng)); //b_star
        if(inter>360f) inter = inter%360f;
        if(inter<0) inter += 360f;
        return inter;
    }
    /// <summary>
    /// Gets the longitude of where the line between p1 and p2 intersects the latitudal line at latLine
    /// </summary>
    /// <param name="latLine"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    float GetIntersectLngOfLineAndLatLine(float latLine, Focus p1, Focus p2)
    {
        float inter = (p1.lng + (latLine-p1.lat)*(p2.lng-p1.lng)/(p2.lat-p1.lat)); //a_star
        if(inter>180f) inter = 360f-inter;
        if(inter<0) inter = Math.Abs(inter);
        return inter;
    }
    int InBoundsLat(float lat, Focus p1, Focus p2 , float lngB, float lngT)
    {

        if(p1.lat>=p2.lat) {
            float tmp = p2.lat;
            p2.lat = 360f;
            float first = GetIntersectLngOfLineAndLatLine(lat,p1,p2);
            p2.lat = tmp;
            tmp = p1.lat;
            p1.lat = 0f;
            float second = GetIntersectLngOfLineAndLatLine(lat,p1,p2);
            p1.lat = tmp;
            if((lngB<=first && first<=lngT) || (lngB<=second && second<=lngT)){
                return 1;
            }
            return 0;
        }
        float inter = GetIntersectLngOfLineAndLatLine(lat,p1,p2);
        if((lngB<=inter && inter<=lngT))
        {
            return 1;
        }
        return 0;
    }
    int InBoundsLng(float lng, Focus p1, Focus p2, float latL, float latR)
    {
        float inter = GetIntersectLatOfLineAndLngLine(lng,p1,p2);
        if((latL<=inter && inter<=latR))
        {
            return 1;
        }
        return 0;
    }
    string InView()
    {
        string line = "";
        Vector3 c = new Vector3(Camera.main.pixelWidth/2,Camera.main.pixelHeight/2,0);
        Vector3 bl = new Vector3(0,0,0);
        Vector3 br = new Vector3(Camera.main.pixelWidth,0,0);
        Vector3 tl = new Vector3(0,Camera.main.pixelHeight,0);
        Vector3 tr = new Vector3(Camera.main.pixelWidth,Camera.main.pixelHeight,0);
        Vector3 tc  = new Vector3(Camera.main.pixelWidth/2,Camera.main.pixelHeight,0);
        Vector3 bc  = new Vector3(Camera.main.pixelWidth/2,0,0);

        Focus bl_e = ScreenPointToLngLat(bl);
        Focus tl_e = ScreenPointToLngLat(tl);
        Focus br_e = ScreenPointToLngLat(br);
        Focus tr_e = ScreenPointToLngLat(tr);
        Focus bc_e = ScreenPointToLngLat(bc);
        Focus tc_e = ScreenPointToLngLat(tc);
        
        float degPerTileX = 360f / Globals.tilesX;
        float degPerTileY = 180f / Globals.tilesY;
        for(int j = 0; j < Globals.tilesY; j++)
        {
            float a_t = degPerTileY*j;
            float a_tt = degPerTileY*(j+1);
            bool yOverlapsL = CrossesLatLine(bl_e,tl_e,a_t) || CrossesLatLine(bl_e,tl_e,a_tt);
            bool yOverlapsR = CrossesLatLine(br_e,tr_e,a_t) || CrossesLatLine(br_e,tr_e,a_tt);
            bool yOverlapsC = CrossesLatLine(bc_e,tc_e,a_t) || CrossesLatLine(bc_e,tc_e,a_tt);
            bool yOverlapsTL = CrossesLatLine(tc_e,tl_e,a_t) || CrossesLatLine(tc_e,tl_e,a_tt);
            bool yOverlapsBL = CrossesLatLine(bc_e,bl_e,a_t) || CrossesLatLine(bc_e,bl_e,a_tt);
            bool yOverlapsTR = CrossesLatLine(tr_e,tc_e,a_t) || CrossesLatLine(tr_e,tc_e,a_tt);
            bool yOverlapsBR = CrossesLatLine(br_e,bc_e,a_t) || CrossesLatLine(br_e,bc_e,a_tt);

            for(int i = 0; i < Globals.tilesX; i++) {
            
                float b_t = degPerTileX*i;
                float b_tt = degPerTileX*(i+1);

                bool overlapsTL = CrossesLngLine(tl_e,tc_e,b_t) || CrossesLngLine(tl_e,tc_e,b_tt);
                bool overlapsBL = CrossesLngLine(bl_e,bc_e,b_t) || CrossesLngLine(bl_e,bc_e,b_tt);
                bool overlapsTR = CrossesLngLine(tc_e,tr_e,b_t) || CrossesLngLine(tc_e,tr_e,b_tt);
                bool overlapsBR = CrossesLngLine(bc_e,br_e,b_t) || CrossesLngLine(bc_e,br_e,b_tt);
                bool overlapsT = CrossesLngLine(tl_e,tr_e,b_t) || CrossesLngLine(tl_e,tr_e,b_tt);
                bool overlapsB = CrossesLngLine(bl_e,br_e,b_t) || CrossesLngLine(bl_e,br_e,b_tt);
                
                int countx = 0;
                int county = 0;
                
                if(overlapsTL)
                {
                    countx += InBoundsLat(b_t,tl_e,tc_e,a_t,a_tt);
                    countx += InBoundsLat(b_tt,tl_e,tc_e,a_t,a_tt);
                }
                if(overlapsTR)
                {
                    countx += InBoundsLat(b_t,tc_e,tr_e,a_t,a_tt);
                    countx += InBoundsLat(b_tt,tc_e,tr_e,a_t,a_tt);
                }
                if(overlapsBL)
                {
                    countx += InBoundsLat(b_t,bl_e,bc_e,a_t,a_tt);
                    countx += InBoundsLat(b_tt,bl_e,bc_e,a_t,a_tt);
                }
                if(overlapsBR)
                {
                    int tmp = countx;
                    countx += InBoundsLat(b_t,bc_e,br_e,a_t,a_tt);
                    countx += InBoundsLat(b_tt,bc_e,br_e,a_t,a_tt);
                }
                if(overlapsT)
                {
                    countx += InBoundsLat(b_t,tl_e,tr_e,a_t,a_tt);
                    countx += InBoundsLat(b_tt,tl_e,tr_e,a_t,a_tt);
                }
                
                if(overlapsB)
                {
                    int tmp = countx;
                    countx += InBoundsLat(b_t,bl_e,br_e,a_t,a_tt);
                    countx += InBoundsLat(b_tt,bl_e,br_e,a_t,a_tt);
                }
                
                if(yOverlapsL)
                {
                    int tmp = county;
                    county += InBoundsLng(a_t,tl_e,bl_e,b_t,b_tt);
                    county += InBoundsLng(a_tt,tl_e,bl_e,b_t,b_tt);

                }
                if(yOverlapsC)
                {
                    int tmp = county;
                    county += InBoundsLng(a_t,tc_e,bc_e,b_t,b_tt);
                    county += InBoundsLng(a_tt,tc_e,bc_e,b_t,b_tt);
                }
                if(yOverlapsR)
                {
                    int tmp = county;
                    county += InBoundsLng(a_t,tr_e,br_e,b_t,b_tt);
                    county += InBoundsLng(a_tt,tr_e,br_e,b_t,b_tt);
                }
                if(countx+county>0 )countx=0;
                else continue;
                
                if(Globals.currentChunk!=null)
                {
                    line += (int)(Globals.currentChunk.tileQuality[j*Globals.tilesX+i]) + " ";
                }
            }
        }

        
        return line;
    }
}
