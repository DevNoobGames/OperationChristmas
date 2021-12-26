using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Linq;
using UnityEngine.SceneManagement;

public class PublicCanvas : MonoBehaviour
{
    List<string> input = new List<string>();
    //public TextMeshProUGUI actionText;

    //public TextMeshProUGUI scoreText;
    public List<scoreBoard> scoreB = new List<scoreBoard>();

    public GameObject gameFinObj;
    public GameObject finalResObj;
    public GameObject quitGame;
    public TextMeshProUGUI finalResult;

    public GameObject levelTextOBJ;
    public TextMeshProUGUI levelTextTMP;
    public Animator levelTextANIM;

    [System.Serializable]
    public class scoreBoard
    {
        public string Name;
        public string NameID;
        public float Kills;
        public float Deaths;
    }

    public void levelText(int level)
    {
        GetComponent<PhotonView>().RPC("RPC_LevelText", RpcTarget.All, level);
    }

    public void sender(string shooterName, string victim)
    {
        GetComponent<PhotonView>().RPC("addString", RpcTarget.All, shooterName + " killed " + victim);
    }

    public void addKillScore(string shooterName, int ShooterID)
    {
        GetComponent<PhotonView>().RPC("AddkillAndSort", RpcTarget.All, shooterName, ShooterID);
    }

    public void addDeathScore(string victimName, int victimID)
    {
        GetComponent<PhotonView>().RPC("addDeath", RpcTarget.All, victimName, victimID);
    }

    public void addPlayerNameOnJoining(string playerName, int playerID)
    {
        GetComponent<PhotonView>().RPC("addName", RpcTarget.All, playerName, playerID);
    }

    public void GameOver()
    {
        GetComponent<PhotonView>().RPC("winScreen", RpcTarget.All);
    }


    [PunRPC]
    public void AddkillAndSort(string shooterName, int ShooterID)
    {
        foreach (scoreBoard b in scoreB)
        {
            if (b.NameID == shooterName + ShooterID)
            {
                b.Kills += 1;
                scoreB = scoreB.OrderByDescending(x => x.Kills).ToList();

                //add reward to shooter

                return;
            }
        }

        //if user didn't register in the scoreboard yet
        scoreBoard a = new scoreBoard();
        a.NameID = shooterName + ShooterID;
        a.Name = shooterName;
        a.Kills = 1;
        scoreB.Add(a);

        scoreB = scoreB.OrderByDescending(x => x.Kills).ToList();
    }

    [PunRPC]
    public void addDeath(string victimName, int victimID)
    {
        foreach (scoreBoard b in scoreB)
        {
            if (b.NameID == victimName + victimID)
            {
                b.Deaths += 1;
                return;
            }
        }

        scoreBoard a = new scoreBoard();
        a.NameID = victimName + victimID;
        a.Name = victimName;
        a.Deaths = 1;
        scoreB.Add(a);
    }

    [PunRPC]
    public void addName(string playerName, int playerID)
    {
        foreach (scoreBoard b in scoreB)
        {
            if (b.NameID == playerName + playerID)
            {
                return;
            }
        }

        scoreBoard a = new scoreBoard();
        a.NameID = playerName + playerID;
        a.Name = playerName;
        a.Deaths = 0;
        scoreB.Add(a);
    }


    [PunRPC]
    public void addString(string textToAdd)
    {
        input.Add(textToAdd);
        if (input.Count > 3)
        {
            input.RemoveAt(0);
        }

        /*actionText.text = "";
        foreach (string txt in input)
        {
            actionText.text += txt + "\n";
        }*/
    }

    [PunRPC]
    public void RPC_LevelText(int level)
    {
        levelTextTMP.text = "LEVEL " + (level + 1);
        levelTextANIM.SetTrigger("play");
    }

    [PunRPC]
    public void winScreen()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameFinObj.SetActive(true);
        scoreB = scoreB.OrderByDescending(x => x.Kills).ToList();

        finalResult.text = "";
        foreach (scoreBoard b in scoreB)
        {
            finalResult.text += b.Name + ": " + b.Kills + " kills" + "\n";
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1;
        Application.Quit();
    }
}





/*
 * 
 * 
    public void sendWinnerScreen()
    {
        GetComponent<PhotonView>().RPC("winScreen", RpcTarget.All);
    }
 
 
[PunRPC]
public void winScreen()
{
    gameFinObj.SetActive(true);
    finalResObj.SetActive(true);
    quitGame.SetActive(true);
    actionText.text = "";
    scoreText.text = "";

    scoreB = scoreB.OrderByDescending(x => x.Kills).ToList();

    finalResult.text = "";
    foreach (scoreBoard b in scoreB)
    {
        finalResult.text += b.Name + ": " + b.Kills + " kills & " + b.Deaths + " deaths" + "\n";
    }
}*/