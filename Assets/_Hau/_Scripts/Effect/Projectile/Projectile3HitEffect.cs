using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile3HitEffect : EffectDamageSender
{
    protected override string GetHitEffectName()
    {
        //return "Hit_3";
        return HitName.Hit_3.ToString();
    }
}
