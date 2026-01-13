using UnityEngine;
using Zenject;

public class StarSystem : PSpace
{
    public int galaxyId;
    public class Factory : PlaceholderFactory<StarSystem> { }
    void Update()
    {

    }
}
