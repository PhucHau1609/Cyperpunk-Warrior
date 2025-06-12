using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile2HitEffect : ProjectileDamageSender
{
    protected override string GetHitEffectName()
    {
        //return "Hit_2";
        return HitName.Hit_2.ToString();
    }
}
