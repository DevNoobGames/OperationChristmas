using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Explosion : MonoBehaviour
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

    private void Start()
    {
        if (!PV.IsMine)
            return;

        StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(1);
        if (PV.IsMine)
        { 
            PhotonNetwork.Destroy(gameObject); //Not the error
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PV.IsMine)
            return;

        if (other.CompareTag("enemy"))
        {
            other.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, shooterName, shooterID, viewID);
        }
    }


}
