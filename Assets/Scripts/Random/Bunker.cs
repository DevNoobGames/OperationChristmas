using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Bunker : MonoBehaviour
{
    public float health = 100;
    public Slider healthBar;
    PhotonView PV;
    public PublicCanvas pubCan;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public void gotHit(float damage)
    {
        PV.RPC("RPC_gotHit", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_gotHit(float damage)
    {
        health -= damage;
        healthBar.value = health;
        if (health <= 0)
        {
            Time.timeScale = 0;
            pubCan.GameOver();
        }
    }
}
