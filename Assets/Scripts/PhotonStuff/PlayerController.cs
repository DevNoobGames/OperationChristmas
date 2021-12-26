using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable //IDamageable
{
    //SC FPS CONTROLLER
    [Header("Movement")]
    public float walkingSpeed = 13f;
    public float runningSpeed = 13f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public GameObject mainHolder;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    CharacterController characterController;
    public Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public Animator walkingAnim;

    [HideInInspector]
    public bool canMove = true;
    //------------------------------
    //NEW WEAPON SYSTEM
    [Header("Weapon System")]
    public int weapon1 = 0;
    public int weapon2 = 1;
    public int activeWeapon = 1;
    public Image imageWeapon1;
    public Image imageWeapon2;
    public AudioSource killingRewardSound;
    //-----------------
    //SHOPPING SYSTEM
    [Header("Shopping System")]
    public Image shopActiveWeapon1;
    public Image shopActiveWeapon2;
    public GameObject shoppingMenu;
    public GameObject weaponShoppingMenu;
    public Camera cam;
    public GameObject markusInfo;
    public bool canShop = false;

    [Header("Random")]
    [SerializeField] Image healtbarImage;
    [SerializeField] GameObject ui;
    [SerializeField] GameObject scoreBoard;
    public TextMeshProUGUI scoreBoardText;
    public SkinnedMeshRenderer playerModel;

    [Header ("Level text")]
    public GameObject levelTextOBJ;
    public TextMeshProUGUI levelTextTMP;
    public Animator levelTextANIM;
    public AudioSource nextLevelAudio;

    [SerializeField] public Item[] items;
    int itemIndex;
    int previousItemIndex = -1;

    PublicCanvas publicCanv;

    Vector3 moveAmount;

    Rigidbody rb;

    PhotonView PV;

    const float maxHealth = 100f;
    float currentHealth = maxHealth;
    public int kills = 0;
    public float money;
    public TextMeshProUGUI moneyText;

    PlayerManager playerManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    public void addMoney(float _money)
    {
        money += _money;
        moneyText.text = "$" + money;
    }

    public void TakeDamage(float f, string s, int i, int j)
    {

    }

    public void addReward(float reward)
    {
        Debug.Log("Reward added");
        addMoney(reward);
        killingRewardSound.Play();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            characterController = GetComponent<CharacterController>();
            publicCanv = GameObject.FindGameObjectWithTag("publicCanvas").GetComponent<PublicCanvas>();
            EquipItem(weapon1);
            setWeaponImgColor(1);
            PV.RPC("RPC_AddName", RpcTarget.All, PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            imageWeapon1.sprite = items[weapon1].image;
            imageWeapon2.sprite = items[weapon2].image;
            shopActiveWeapon1.sprite = items[weapon1].image;
            shopActiveWeapon2.sprite = items[weapon2].image;

            playerModel.enabled = false;
        }
        else
        {
            Destroy(GetComponent<CharacterController>());
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        Move();

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (activeWeapon == 1)
            {
                activeWeapon = 2;
                EquipItem(weapon2);
                setWeaponImgColor(2);
            }
            else
            {
                activeWeapon = 1;
                EquipItem(weapon1);
                setWeaponImgColor(1);
            }
        }
        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (activeWeapon == 1)
            {
                activeWeapon = 2;
                EquipItem(weapon2);
                setWeaponImgColor(2);
            }
            else
            {
                activeWeapon = 1;
                EquipItem(weapon1);
                setWeaponImgColor(1);
            }
        }

        //SHOPPING AT MARKUS
        markusInfo.SetActive(false);
        canShop = false;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit, 4))
        {
            if (hit.collider.CompareTag("Markus"))
            {
                markusInfo.SetActive(true);
                canShop = true;
            }
        }


        if (Input.GetKeyDown(KeyCode.E) && canShop)
        {
            if (!shoppingMenu.activeInHierarchy)
            {
                shoppingMenuOpen(true);
            }
            else
            {
                shoppingMenuOpen(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreBoard.SetActive(true);

            //Set scoreboard text
            scoreBoardText.text = "";

            foreach (PublicCanvas.scoreBoard c in publicCanv.scoreB)
            {
                scoreBoardText.text += c.Name + ": " + c.Kills + " kills" + "\n";
            }
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreBoard.SetActive(false);
        }

        if (Input.GetMouseButton(0) && canMove)
        {
            items[itemIndex].Use();
        }

        if (transform.position.y < -10f)
        {
            Die();
        }

        if (Input.GetAxis("Horizontal") > 0 || Input.GetAxis("Horizontal") < 0 || Input.GetAxis("Vertical") > 0 || Input.GetAxis("Vertical") < 0)
        {
            walkingAnim.SetBool("walking", true);
        }
        else
        {
            walkingAnim.SetBool("walking", false);
        }
    }

    public void setWeaponImgColor(int weapon)
    {
        if (weapon == 1)
        {
            imageWeapon1.color = new Color32(255, 255, 255, 255);
            imageWeapon2.color = new Color32(255, 255, 255, 80);
        }
        else if (weapon == 2)
        {
            imageWeapon1.color = new Color32(255, 255, 255, 80);
            imageWeapon2.color = new Color32(255, 255, 255, 255);
        }
    }

    public void shoppingMenuOpen(bool isOpen)
    {
        shoppingMenu.SetActive(isOpen);
        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            weaponShoppingMenu.SetActive(isOpen); //shouldn't be opened just yet!
        }
        Cursor.visible = isOpen;
        canMove = !isOpen;
    }

    void Move()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove && Time.timeScale > 0)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            mainHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            //itemHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    public void EquipItem(int _index)
    {

        if (_index == previousItemIndex)
            return;

        itemIndex = _index;

        //items[itemIndex].itemGameObject.SetActive(true);
        items[itemIndex].gameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            //items[previousItemIndex].itemGameObject.SetActive(false);
            items[previousItemIndex].gameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        items[itemIndex].setAmmoText();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }


    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    /*public void TakeDamage(float damage, string shooterName, int ShooterID)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, shooterName, ShooterID);
    }*/

    public void levelText(int level)
    {
        PV.RPC("RPC_LevelText", RpcTarget.All, level);
        /*levelTextTMP.text = "LEVEL " + (level + 1);
        levelTextANIM.SetTrigger("play");*/
    }

    [PunRPC]
    void RPC_AddName(string playerName, int ID)
    {
        if (!PV.IsMine)
            return;

        publicCanv.addPlayerNameOnJoining(playerName, ID);
    }

    [PunRPC]
    void RPC_AddMoney(int ID)
    {
        if (!PV.IsMine)
            return;
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, string shooterName, int ShooterID)
    {
        Debug.Log("Got hit by " + shooterName);
        if (!PV.IsMine)
            return;

        Debug.Log("took damage: " + damage);

        currentHealth -= damage;

        healtbarImage.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            //publicCanv.addKillScore(shooterName, ShooterID);
            publicCanv.addDeathScore(PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
            //publicCanv.sender(shooterName, PhotonNetwork.NickName);
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }

    [PunRPC]
    void RPC_LevelText(int level)
    {
        if (!PV.IsMine)
            return;

        levelTextTMP.text = "LEVEL " + (level + 1);
        levelTextANIM.SetTrigger("play");
        nextLevelAudio.Play();
    }
}



//OLD WEAPON SYSTEM
/*for (int i = 0; i < items.Length; i++)
{
    if (Input.GetKeyDown((i + 1).ToString()))
    {
        EquipItem(i);
        break;
    }
}

if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
{
    if (itemIndex >= items.Length - 1)
    {
        EquipItem(0);
    }
    else
    {
        EquipItem(itemIndex + 1);
    }
}
if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
{
    if (itemIndex <= 0)
    {
        EquipItem(items.Length - 1);
    }
    else
    {
        EquipItem(itemIndex - 1);
    }
}*/
