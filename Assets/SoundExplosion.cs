using UnityEngine;

public class SoundExplosion : MonoBehaviour
{
    public AudioClip clip;

    void Start()
    {
        if (clip != null)
        {
            AudioSource source = GetComponent<AudioSource>();
            source.clip = clip;
            source.Play();
            Destroy(gameObject, clip.length);
        }
    }
}
