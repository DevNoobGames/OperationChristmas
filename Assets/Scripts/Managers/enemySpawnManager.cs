using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class enemySpawnManager : MonoBehaviour
{
    public LevelText levelText;

    public GameObject[] playercon;

    public GameObject[] spawnPoints;

    public int currentLevel;
    public levelSettings[] levelSet;
    public GameObject BasicEnemy;
    public int enemyAmount;


    [System.Serializable]
    public class levelSettings
    {
        public int amountOfEnemies;


        [Header("value starts at 0 and ends at 1")]
        public EnemyChances[] enemies;

        [System.Serializable]
        public class EnemyChances
        {
            public GameObject enemy;
            public float Value = 1;
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Destroy(gameObject);
        }

        spawnPoints = GameObject.FindGameObjectsWithTag("enemySpawnPoint");
        foreach (GameObject sp in spawnPoints)
        {
            sp.GetComponent<Renderer>().enabled = false;
        }

        StartCoroutine(startLevel(currentLevel));
    }

    public IEnumerator startLevel(int level)
    {
        //add starting text
        playercon = GameObject.FindGameObjectsWithTag("playerController");
        foreach (GameObject pl in playercon)
        {
            pl.GetComponent<PlayerController>().levelText(level);
        }

        yield return new WaitForSeconds(2);

        if (level < levelSet.Length)
        {
            for (int i = 0; i < levelSet[level].amountOfEnemies; i++)
            {
                int spawnPos = Random.Range(0, spawnPoints.Length); //Find random spawn pos

                //Find which enemy to deploy
                float enemyValue = Random.Range(0f, 1f);
                string toSpawn = BasicEnemy.name;
                foreach (levelSettings.EnemyChances fc in levelSet[level].enemies)
                {
                    if (enemyValue <= fc.Value)
                    {
                        toSpawn = fc.enemy.name;
                        break;
                    }
                }

                //spawn the enemy
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", toSpawn), spawnPoints[spawnPos].transform.position, Quaternion.identity); //spawn player
                enemyAmount += 1;
            }
        }
        //else replay last level
        else
        {
            for (int i = 0; i < levelSet[(levelSet.Length - 1)].amountOfEnemies; i++)
            {
                int spawnPos = Random.Range(0, spawnPoints.Length); //Find random spawn pos

                //Find which enemy to deploy
                float enemyValue = Random.Range(0f, 1f);
                string toSpawn = BasicEnemy.name;
                foreach (levelSettings.EnemyChances fc in levelSet[levelSet.Length - 1].enemies)
                {
                    if (enemyValue <= fc.Value)
                    {
                        toSpawn = fc.enemy.name;
                        break;
                    }
                }

                //spawn the enemy
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", toSpawn), spawnPoints[spawnPos].transform.position, Quaternion.identity); //spawn player
                enemyAmount += 1;
            }
        }
    }

    public void killedEnemy()
    {
        enemyAmount -= 1;
        if (enemyAmount <= 0)
        {
            currentLevel += 1;
            StartCoroutine(startLevel(currentLevel));
        }
    }


}
