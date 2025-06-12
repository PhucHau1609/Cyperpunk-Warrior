using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile4HitEffect : EffectDamageSender
{
    protected override string GetHitEffectName()
    {
        //return "Hit_4"; //turn into enum this one
        return HitName.Hit_4.ToString();
    }
}
