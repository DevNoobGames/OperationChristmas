using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShoppingPanel : MonoBehaviour
{
    public PlayerController playerCon;
    public int WeaponActive;

    public float walkingSpeedCost = 500;

    public TextMeshProUGUI weaponTitle;
    public GameObject weaponPanel;
    public weapons[] weaponList;

    public AudioSource kaching;
    public AudioSource broke;

    [System.Serializable]
    public class weapons
    {
        public int newWeaponNumber;
        public float cost;
        public int ammo;
    }

    public void WeaponSelector(int weaponNr)
    {
        WeaponActive = weaponNr;
        weaponPanel.SetActive(true);
        weaponTitle.text = "Weapon " + weaponNr;
    }

    public void buySpeed()
    {
        if (playerCon.walkingSpeed < 20 && playerCon.money >= walkingSpeedCost)
        {
            kaching.Play();
            playerCon.walkingSpeed += 2;
            playerCon.addMoney(-walkingSpeedCost);
        }
        else if (playerCon.money < walkingSpeedCost)
        {
            broke.Play();
        }
    }

    public void buyWeapon(int newWeapon)
    {
        //Test
        if (playerCon.money >= weaponList[newWeapon].cost)
        {
            kaching.Play();
            playerCon.addMoney(-weaponList[newWeapon].cost);
            if (WeaponActive == 1)
            {
                if (playerCon.weapon1 == weaponList[newWeapon].newWeaponNumber)
                {
                    //add ammo
                    playerCon.items[playerCon.weapon1].GetComponent<Colt1872>().reserveAmmo += weaponList[newWeapon].ammo;
                    playerCon.items[playerCon.weapon1].setAmmoText();
                }
                else
                {
                    playerCon.weapon1 = weaponList[newWeapon].newWeaponNumber;
                    playerCon.EquipItem(playerCon.weapon1);
                    playerCon.setWeaponImgColor(1);
                    playerCon.imageWeapon1.sprite = playerCon.items[playerCon.weapon1].image;
                }
            }
            else if (WeaponActive == 2)
            {
                if (playerCon.weapon2 == weaponList[newWeapon].newWeaponNumber)
                {
                    //add ammo
                    playerCon.items[playerCon.weapon2].GetComponent<Colt1872>().reserveAmmo += weaponList[newWeapon].ammo;
                    playerCon.items[playerCon.weapon2].setAmmoText();
                }
                else
                {
                    playerCon.weapon2 = weaponList[newWeapon].newWeaponNumber;
                    playerCon.EquipItem(playerCon.weapon2);
                    playerCon.setWeaponImgColor(2);
                    playerCon.imageWeapon2.sprite = playerCon.items[playerCon.weapon2].image;
                }
            }
        }
        else if (playerCon.money < weaponList[newWeapon].cost)
        {
            broke.Play();
        }
    }

    public void Exit()
    {
        playerCon.shoppingMenuOpen(false);
    }
}
