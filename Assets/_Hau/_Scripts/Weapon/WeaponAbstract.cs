using UnityEngine;

public abstract class WeaponAbstract : HauMonoBehaviour
{

    [Header("Sound")]
    [SerializeField] protected SoundName shootSFXName = SoundName.LaserOneShot;
    protected virtual void SpawnSound(Vector3 spawnPos)
    {
        SFXCtrl newSfx = HauSoundManager.Instance.CreateSfx(this.shootSFXName);
        newSfx.transform.position = spawnPos;
        newSfx.gameObject.SetActive(true);
    }
}
