using UnityEngine;
public class SignalChunkDestroy
{
    public SignalChunkDestroy()
    {
    }
}
public class SignalChunkFloatingOriginFixStart
{
    public Vector3 offset { get; }

    public SignalChunkFloatingOriginFixStart(Vector3 offset)
    {
        this.offset = offset;
    }
}
public class SignalChunkFloatingOriginFixEnd
{
    public Vector3 offset { get; }

    public SignalChunkFloatingOriginFixEnd(Vector3 offset)
    {
        this.offset = offset;
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
