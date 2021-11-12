using UnityEngine;

// System.Serializable: to change in inspector and save/load
[System.Serializable]
public class PlayerWeapon
{
    public string name = "Glock";

    public int damage = 10;
    public float range = 100f;

    public float fireRate = 0f;

    public GameObject graphics;

    public int maxBullets = 20;
    [HideInInspector]
    public int bullets;

    public PlayerWeapon()
    {
        this.bullets = this.maxBullets;
    }

}
