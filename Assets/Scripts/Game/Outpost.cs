using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outpost
{
    public int roadIndex;
    public int positionIndex;
    public ulong ownerIndex;
    public bool castle;

    public int infantryCount;
    public int artilleryCount;
    public int bearCount;
    public int mamoothCount;

    public Outpost(int roadIndex, int positionIndex, ulong ownerIndex, bool castle, int infantryCount, int artilleryCount, int bearCount, int mamoothCount)
    {
        this.roadIndex = roadIndex;
        this.positionIndex = positionIndex;
        this.ownerIndex = ownerIndex;
        this.castle = castle;

        this.infantryCount = infantryCount;
        this.artilleryCount = artilleryCount;
        this.bearCount = bearCount;
        this.mamoothCount = mamoothCount;
    }

    public Outpost(int roadIndex, int positionIndex, ulong ownerIndex, bool castle)
    {
        this.roadIndex = roadIndex;
        this.positionIndex = positionIndex;
        this.ownerIndex = ownerIndex;
        this.castle = castle;

        this.infantryCount = 0;
        this.artilleryCount = 0;
        this.bearCount = 0;
        this.mamoothCount = 0;
    }
}
