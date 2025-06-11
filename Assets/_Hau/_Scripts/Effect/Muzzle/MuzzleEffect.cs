using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleEffect : HauMonoBehaviour
{
    [SerializeField] protected MuzzleCode muzzle;

    protected override void OnEnable()
    {
        this.SpawnMuzzle();
    }

    protected virtual void SpawnMuzzle()
    {
        if (this.muzzle == MuzzleCode.NoMuzzle) return;
        EffectSpawner effectSpawner = EffectSpawnerCtrl.Instance.EffectSpawner;
        EffectCtrl prefab = effectSpawner.PoolPrefabs.GetPrefabByName(this.muzzle.ToString());
        EffectCtrl newEffect = effectSpawner.Spawn(prefab, transform.position);
        newEffect.gameObject.SetActive(true);
    }
}
