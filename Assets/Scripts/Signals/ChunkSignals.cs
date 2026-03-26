using UnityEngine;
public class SignalChunkDestroy
{
    public SignalChunkDestroy()
    {
    }
}
public class SignalChunkFloatingOriginFix
{
    public Vector3 offset { get; }

    public SignalChunkFloatingOriginFix(Vector3 offset)
    {
        this.offset = offset;
    }
}
