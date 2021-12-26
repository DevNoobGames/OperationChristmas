using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Item
{
    //ONLY WHAT EVERY GUN HAS!
    public abstract override void Use();
    public abstract override void setAmmoText();

    public int clipSize;
    public int activeAmmo;
    public int reserveAmmo;
    public float reloadTime;
    public bool isReloading;
    public float timeBetweenShot;
    public bool hasShot;
    public bool isAutomatic;
    public bool visualBullet;
    public AudioSource reloadAudio;

    public GameObject bulletImpactPrefab;
    public GameObject gunModel;
}
