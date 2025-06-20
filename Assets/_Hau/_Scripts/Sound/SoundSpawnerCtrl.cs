using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSpawnerCtrl : HauSingleton<SoundSpawnerCtrl>
{
    [SerializeField] protected SoundSpawner soundSpawner;
    public SoundSpawner SoundSpawner => soundSpawner;

    [SerializeField] protected SoundPrefab soundPrefabs;
    public SoundPrefab SoundPrefabs => soundPrefabs;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadSoundSpawner();
        this.LoadSoundPrefabs();
    }

    protected virtual void LoadSoundSpawner()
    {
        if (this.soundSpawner != null) return;
        this.soundSpawner = GetComponent<SoundSpawner>();
        Debug.LogWarning(transform.name + ": LoadSoundSpawner: " + gameObject);

    }

    protected virtual void LoadSoundPrefabs()
    {
        if (this.soundPrefabs != null) return;
        this.soundPrefabs = GetComponentInChildren<SoundPrefab>();
        Debug.LogWarning(transform.name + ": LoadSoundPrefabs: " + gameObject);

    }
}
