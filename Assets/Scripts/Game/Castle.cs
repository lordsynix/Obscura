using UnityEngine;

public class Castle
{
    public int infantryCount;
    public int artilleryCount;
    public int bearCount;
    public int mamoothCount;
    public ulong ownerIndex;

    public Castle(int infantryCount, int artilleryCount, int bearCount, int mamoothCount, ulong ownerIndex)
    {
        this.infantryCount = infantryCount;
        this.artilleryCount = artilleryCount;
        this.bearCount = bearCount;
        this.mamoothCount = mamoothCount;
        this.ownerIndex = ownerIndex;
    }
}
