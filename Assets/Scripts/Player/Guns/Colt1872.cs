using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class Colt1872 : Gun
{
    [SerializeField] Camera cam;
    [SerializeField] CameraShake camShake;
    [SerializeField] PlayerController playCon;

    PhotonView PV;
    public PhotonView parentPV;

    public Animator shootAnim;
    public bool singleShotDone = false;
    public AudioSource gunShotAudio;
    public TextMeshProUGUI ammoText;
    public Transform shootPos;

    public float shakeDuration;
    public float shakeAmount;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isReloading = false;
        hasShot = false;
        shootAnim.Rebind();
        //shootAnim.Update(0f);
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            gunModel.layer = 7;
            ChangeLayers(gunModel.transform, 7);
            hasShot = false;
        }
    }

    public void ChangeLayers(Transform trans, int layer)
    {
        foreach (Transform child in trans)
        {
            child.gameObject.layer = layer;
        }
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            singleShotDone = false;
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && reserveAmmo > 0 && !hasShot && activeAmmo < clipSize)
        {
            isReloading = true;
            StartCoroutine(Reloading());
        }
    }

    public override void Use()
    {
        Shoot();
    }


    void Shoot()
    {
        if (activeAmmo > 0 && !singleShotDone && !hasShot && !isReloading)
        {
            if (!isAutomatic)
            {
                singleShotDone = true;
            }

            hasShot = true;
            StartCoroutine(TimeBetweenShot());
            
            activeAmmo -= 1;
            setAmmoText();

            camShake.shakeAmount = shakeAmount;
            camShake.shakeDuration = shakeDuration;

            //Shoot bullet
            if (!visualBullet)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                ray.origin = cam.transform.position;
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage, PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber, parentPV.ViewID);
                    PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
                }
            }
            if (visualBullet)
            {
                GameObject project = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ExplosiveGift"), shootPos.position, Quaternion.identity); //spawn bullet
                project.GetComponent<Rigidbody>().AddForce(transform.forward * 3000);
                project.GetComponent<ExplosiveGift>().damage = ((GunInfo)itemInfo).damage;
                project.GetComponent<ExplosiveGift>().shooterName = PhotonNetwork.NickName;
                project.GetComponent<ExplosiveGift>().shooterID = PhotonNetwork.LocalPlayer.ActorNumber;
                project.GetComponent<ExplosiveGift>().viewID = parentPV.ViewID;
            }

            //Play rest
            shootAnim.SetTrigger("shoot");
            gunShotAudio.Play();


        }
        else if (activeAmmo <= 0 && !hasShot)
        {
            if (reserveAmmo > 0 && !isReloading)
            {
                isReloading = true;
                StartCoroutine(Reloading());
            }
        }
    }

    public override void setAmmoText()
    {
        ammoText.text = activeAmmo + "/" + reserveAmmo;
    }

    IEnumerator Reloading()
    {
        isReloading = true;
        shootAnim.SetTrigger("reload");
        reloadAudio.Play();
        yield return new WaitForSeconds(reloadTime);
        if (reserveAmmo >= clipSize)
        {
            activeAmmo = clipSize;
            reserveAmmo -= clipSize;
        }
        else if (reserveAmmo < clipSize)
        {
            activeAmmo = reserveAmmo;
            reserveAmmo = 0;
        }
        isReloading = false;
        hasShot = false;
        setAmmoText();
    }
    IEnumerator TimeBetweenShot()
    {
        yield return new WaitForSeconds(timeBetweenShot);
        hasShot = false;

        if (activeAmmo <= 0 && !hasShot)
        {
            if (reserveAmmo > 0 && !isReloading)
            {
                isReloading = true;
                StartCoroutine(Reloading());
            }
        }
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }
}
