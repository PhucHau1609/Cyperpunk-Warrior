using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HauSoundManager : HauSingleton<HauSoundManager>
{
    [SerializeField] protected SoundName bgName = SoundName.LastStand;
    [SerializeField] protected MusicCtrl bgMusic;
    [SerializeField] protected SoundSpawnerCtrl ctrl;

    [Range(0f, 1f)]
    [SerializeField] protected float volumeMusic = 1f;

    [Range(0f, 1f)]
    [SerializeField] protected float volumeSfx = 1f;
    [SerializeField] protected List<MusicCtrl> listMusic;
    [SerializeField] protected List<SFXCtrl> listSfx;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    protected override void Start()
    {
        base.Start();
        //this.StartBackgroundMusic();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadSoundSpawnerCtrl();
    }

    private void LoadSoundSpawnerCtrl()
    {
        if (this.ctrl != null) return;
        this.ctrl = GameObject.FindAnyObjectByType<SoundSpawnerCtrl>();
        Debug.LogWarning(transform.name + ": LoadSoundSpawnerCtrl: " + gameObject);
    }

    public virtual void StartBackgroundMusic()
    {
        if (this.bgMusic == null) this.bgMusic = this.CreatePrefabMusic(this.bgName);
        this.bgMusic.gameObject.SetActive(true);
    }

    protected virtual MusicCtrl CreatePrefabMusic(SoundName soundName)
    {
        MusicCtrl musicPrefab = (MusicCtrl)this.ctrl.SoundPrefabs.GetPrefabByName(soundName.ToString());
        return CreateSpawnMusic(musicPrefab);
    }

    protected virtual MusicCtrl CreateSpawnMusic(MusicCtrl musicPrefab)
    {
        MusicCtrl musicSpawn = (MusicCtrl)this.ctrl.SoundSpawner.Spawn(musicPrefab,Vector3.zero);
        this.AddMusic(musicSpawn);
        return musicSpawn;
    }

    public virtual void AddMusic(MusicCtrl music)
    {
        if(this.listMusic.Contains(music)) return;
        this.listMusic.Add(music);
    }

    public virtual SFXCtrl CreateSfx(SoundName soundName)
    {
        SFXCtrl soundPrefab = (SFXCtrl)this.ctrl.SoundPrefabs.GetPrefabByName(soundName.ToString());
        return this.CreateSfx(soundPrefab);
    }

    public virtual SFXCtrl CreateSfx(SFXCtrl sfxPrefab)
    {
        SFXCtrl newSound = (SFXCtrl)this.ctrl.SoundSpawner.Spawn(sfxPrefab, Vector3.zero);
        this.AddSfx(newSound);
        return newSound;
    }

    public virtual void AddSfx(SFXCtrl newSound)
    {
        if (this.listSfx.Contains(newSound)) return;
        this.listSfx.Add(newSound);
    }

    public virtual void VolumeMusicUpdating(float volume)
    {
        this.volumeMusic = volume;
        foreach(MusicCtrl music in this.listMusic)
        {
            music.AudioSource.volume = this.volumeMusic;
        }
    }

    public virtual void VolumeSfxUpdating(float volume)
    {
        this.volumeSfx = volume;
        foreach (SFXCtrl sfx in this.listSfx)
        {
            sfx.AudioSource.volume = this.volumeSfx;
        }
    }

    public virtual void ToogleMusic()
    {
        if(this.bgMusic == null)
        {
            this.StartBackgroundMusic();
            return;
        }

        bool status = this.bgMusic.gameObject.activeSelf;
        this.bgMusic.gameObject.SetActive(!status);
    }

    public virtual void SpawnSound(Vector3 spawnPos, SoundName SFXName)
    {
        SFXCtrl newSfx = this.CreateSfx(SFXName);
        newSfx.transform.position = spawnPos;
        newSfx.gameObject.SetActive(true);
    }
}
