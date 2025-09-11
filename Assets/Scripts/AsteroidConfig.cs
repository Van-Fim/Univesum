using UnityEngine;

[CreateAssetMenu(fileName = "AsteroidConfig", menuName = "Asteroids/Config")]
public class AsteroidConfig : ScriptableObject
{
    public string id;
    public GameObject prefab;
    public int poolSize = 20;
    public float scale = 1f;
    public Color tint = Color.white;
    // можно добавить поведение, материалы, звук и т.д.
}
