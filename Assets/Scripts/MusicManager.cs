using UnityEngine;
using Zenject;
public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public static MusicManager singleton;
    public void Start()
    {
        singleton = this;
    }
}
