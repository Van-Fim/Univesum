using UnityEngine;

[CreateAssetMenu(fileName = "CursorConfig", menuName = "Cursors/CursorConfig")]
public class CursorConfig : ScriptableObject
{
    public Texture2D customCursor;
    public Vector2 hotspot = Vector2.zero;
}
