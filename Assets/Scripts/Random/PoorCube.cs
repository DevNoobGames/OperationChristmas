using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using System.IO;

public class PoorCube : MonoBehaviour, IDamageable
{
    public float health = 100;
    public float reward = 200;

    PublicCanvas publicCanv;
    public PhotonView PV;

    NavMeshAgent agent;
    public Transform target;

    public enemySpawnManager enemySpawnMan;
    public GameObject bunkerObj;

    private void Start()
    {
        if (PV.IsMine)
        {
            publicCanv = GameObject.FindGameObjectWithTag("publicCanvas").GetComponent<PublicCanvas>();
            agent = GetComponent<NavMeshAgent>();
            target = FindClosestEnemy().transform;
            enemySpawnMan = GameObject.FindGameObjectWithTag("enemySpawnManager").GetComponent<enemySpawnManager>();
            bunkerObj = GameObject.FindGameObjectWithTag("Bunker");
        }
    }

    private void Update()
    {
        if (target)
        {
            agent.SetDestination(target.position);

            if (Vector3.Distance(transform.position, target.position) < 2)
            {
                GameObject explo = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "enemyExplosion"), transform.position, Quaternion.identity);
                bunkerObj.GetComponent<Bunker>().gotHit(10);
                Die();
            }
        }
    }

    public void addReward(float r)
    {
    }

    public void TakeDamage(float damage, string shooterName, int ShooterID, int viewID)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, shooterName, ShooterID, viewID);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, string shooterName, int ShooterID, int viewID)
    {
        if (!PV.IsMine)
            return;

        Debug.Log("took damage: " + damage);

        health -= damage;

        if (health <= 0)
        {
            publicCanv.addKillScore(shooterName, ShooterID);
            PV.RPC("RPC_sendReward", RpcTarget.All, viewID);

            Die();
        }
    }

    [PunRPC]
    void RPC_sendReward(int viewID)
    {
        Debug.Log("sending reward to " + viewID);
        PhotonView.Find(viewID).gameObject.GetComponent<IDamageable>()?.addReward(reward);
    }

    void Die()
    {
        if (enemySpawnMan)
        {
            enemySpawnMan.killedEnemy();
        }

        PhotonNetwork.Destroy(gameObject);
    }

    public GameObject FindClosestEnemy()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("enemyWalkToPoint");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

}
