using UnityEngine;
using Zenject;
public class CursorManager : MonoBehaviour
{
    public Texture2D customCursor;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;
    public void SwitchCursor(string cursorName)
    {
        CursorConfig config = Resources.Load<CursorConfig>($"Configs/Cursors/{cursorName}");
        if (config != null)
        {
            customCursor = config.customCursor;
            hotspot = config.hotspot;
            Cursor.SetCursor(customCursor, hotspot, cursorMode);
        }
    }
    public void Start()
    {
        SwitchCursor("Default");
    }
}
