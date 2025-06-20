using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MusicCtrl : SoundCtrl
{
    protected override void ResetValue()
    {
        base.ResetValue();
        this.audioSource.loop = true;
    }
}
