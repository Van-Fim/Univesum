using UnityEngine;

public class Chunk : MonoBehaviour
{
    public bool isDestroyed;
    public void Destroy()
    {
        isDestroyed = true;
    }
}
