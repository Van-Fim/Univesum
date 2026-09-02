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
        float maxSpeed = sp_object.engine.maxSpeed;
        float sp = 1;
        if (maxSpeed > 0)
        {
            sp = Mathf.Lerp(0.3f, 1.0f, speed / maxSpeed);
        }
        idleSource.pitch = sp;
        idleSource.volume = sp / 20;
    }

    public void InstallSounds(EngineConfig engine)
    {
        idleSource = gameObject.AddComponent<AudioSource>();
        idleSource.clip = Resources.Load<AudioClip>("Sounds/Engines/" + engine.soundIdle); // без расширения
        idleSource.loop = true; // чтобы звук играл постоянно
        idleSource.playOnAwake = true; // можно включить автозапуск
        idleSource.spatialBlend = 1.0f;
        idleSource.dopplerLevel = 0;
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
