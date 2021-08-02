using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{
    public int tilesX;
    public int tilesY;
    public Quality[] tileQuality;
    public double startTime;
    public double duration;
    public int size;
    public int segmentNr;
    public ChunkData(int inTilesX, int inTilesY, double inStartTime, double inDuration)
    {
        this.tilesX = inTilesX;
        this.tilesY = inTilesY;
        this.startTime = inStartTime;
        this.duration = inDuration;
        this.segmentNr = Convert.ToInt32(startTime / duration);
        tileQuality = new Quality[inTilesX * inTilesY];

    }
    public void SetTileQualities(Quality[] inQual)
    {
        int length = (inQual.Length > tileQuality.Length) ? tileQuality.Length : inQual.Length;
        for (int i = 0; i < inQual.Length; i++)
        {
            tileQuality[i] = inQual[i];
        }
    }
    public void SetTileQuality(int x, int y, Quality qual)
    {
        if (x >= tilesX || x < 0 || y >= tilesY || y < 0) return;
        tileQuality[y * tilesX + x] = qual;
    }
    public Quality GetTileQuality(int x, int y)
    {
        if (x >= tilesX || x < 0 || y >= tilesY || y < 0) return 0;
        return tileQuality[y * tilesX + x];
    }
    /// <summary>
    /// Upgrades a chunk with the given qualities
    /// </summary>
    /// <param name="upgrade">The array describing the new qualities. Make sure to only send in higher qualities than before or "None".</param>
    public void UpgradeTileQualities(Quality[] upgrade)
    {
        for (int i = 0; i < upgrade.Length; i++)
        {
            if(upgrade[i]!=Quality.None)tileQuality[i] = upgrade[i];
        }
    }
}