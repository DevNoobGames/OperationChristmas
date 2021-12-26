using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class ExplosiveGift : MonoBehaviour
{
    public float damage;
    public string shooterName;
    public int shooterID;
    public int viewID;

    PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PV.IsMine)
            return;

        //Instantiate(Resources.Load("Explosion"), transform.position, transform.rotation
        GameObject explo = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Explosion"), transform.position, Quaternion.identity); //spawn explosion
        explo.GetComponent<Explosion>().shooterName = shooterName;
        explo.GetComponent<Explosion>().shooterID = shooterID;
        explo.GetComponent<Explosion>().damage = damage;
        explo.GetComponent<Explosion>().viewID = viewID;
        
        PhotonNetwork.Destroy(gameObject);
    }
}
