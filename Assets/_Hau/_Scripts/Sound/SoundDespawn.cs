using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDespawn : Despawn<SoundCtrl>
{
    protected override void ResetValue()
    {
        base.ResetValue();
        this.timeLife = 1f;
        this.currentTime = 1f;
    }
}
