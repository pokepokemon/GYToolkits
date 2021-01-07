using UnityEngine;
using System.Collections;
using System;

public class AStarNode : IComparable<AStarNode>
{
    public byte status;
    public int x;
    public int y;
    public AStarNode parent;

    public int F;

    public int G;

    public int H;

    public const string POOL_KEY = "AStarNode";

    public void Calc()
    {
        F = G + H;
    }

    public int CompareTo(AStarNode other)
    {
        return this.F.CompareTo(other.F);
    }
}
