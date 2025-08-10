using System.Collections.Generic;
using UnityEngine;

public class FallingBlockManager : MonoBehaviour
{
    public static List<FallingBlock> allBlocks = new List<FallingBlock>();

    private void Awake()
    {
        allBlocks.Clear();
    }

    public static void Register(FallingBlock block)
    {
        if (!allBlocks.Contains(block))
            allBlocks.Add(block);
    }

    public static void ResetAllBlocks()
    {
        foreach (var block in allBlocks)
        {
            if (block != null)
                block.ResetBlock();
        }
    }
}
