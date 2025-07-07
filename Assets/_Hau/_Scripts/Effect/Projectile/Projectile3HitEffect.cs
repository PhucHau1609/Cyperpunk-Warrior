using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile3HitEffect : ProjectileDamageSender
{
    protected override string GetHitEffectName()
    {
        //return "Hit_3";
        return _hitName.ToString();
    }
}
