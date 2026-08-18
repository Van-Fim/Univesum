using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UpgradeItem : MonoBehaviour
{
    public bool isDestroyed;
    public virtual void Destroy()
    {
        isDestroyed = true;
        GameObject.Destroy(gameObject);
    }
}
