using UnityEngine;
using Zenject;
public class WeaponConfig
{
    public string weaponName;
    public float damage;
    public float fireRate;
    public float energyCost;
    public string pathToModel;
    public GameObject projectilePrefab;
    public AudioClip fireSound;
}