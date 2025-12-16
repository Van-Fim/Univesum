using UnityEngine;

public class EngineSoundController : MonoBehaviour
{
    public AudioSource idleSource;
    public AudioSource boostSource;
    public AudioSource shutdownSource;
    public SpaceObject sp_object;

    void Update()
    {
        float speed = sp_object.rigidbody.linearVelocity.magnitude;

        // Idle звук: громкость и питч растут с ускорением
        idleSource.pitch = Mathf.Lerp(0.8f, 2.0f, speed / 3000f);
        idleSource.volume = Mathf.Lerp(0.1f, 0.25f, speed / 3000f)/20;
    }

    public void InstallSounds(EngineConfig engine)
    {
        idleSource = gameObject.AddComponent<AudioSource>();
        idleSource.clip = Resources.Load<AudioClip>("Sounds/Engines/" + engine.soundIdle); // без расширения
        idleSource.loop = true; // чтобы звук играл постоянно
        idleSource.playOnAwake = true; // можно включить автозапуск
        idleSource.spatialBlend = 1.0f;
        idleSource.minDistance = 2f;
        idleSource.maxDistance = 4f;
        idleSource.Play();
    }

    public void PlayBoost()
    {
        boostSource.PlayOneShot(boostSource.clip);
    }

    public void PlayShutdown()
    {
        shutdownSource.PlayOneShot(shutdownSource.clip);
    }
}
