using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile1HitEffect : ProjectileDamageSender
{
    protected override string GetHitEffectName()
    {
        //return "Hit_1";
        return HitName.Hit_1.ToString();
    }
}
