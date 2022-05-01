using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerControl : MonoBehaviourPunCallbacks
{
    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;

    public bool invertLook;


    public float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed;
    private Vector3 moveDir, movement;


    public CharacterController charCon;

    private Camera cam;

    public float jumpForce = 12f, gravityMode = 2.5f;

    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayers;


    public GameObject buletImpact;
    //public float timeBetwenShoots = .1f;
    private float shootControler;

    public float muzzleDisplayTime;
    private float muzzleConter;


    public float maxHeat = 10f, /*heatPerShot = 1f,*/ coolRate = 4f, overbeatCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;


    public Gun[] allGuns;
    private int selectedGun;


    public GameObject playerHitImpact;

    public int maxHealth = 100;
    private int currentHealth;

    public Animator animator;
    public GameObject playerModel;
    public Transform modelGunPoint, gunHolder;


    public Material[] allSkins;

    private float adsSpeed = 5f;
    public Transform adsOutPoint, adsInPoint;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeat;

        photonView.RPC("SetGun", RpcTarget.All, selectedGun);

        currentHealth = maxHealth;


        //SwitchGun();

        //Transform newTrans = SpownManager.instance.GetSpawnPoint();
        //transform.position = newTrans.position;
        //transform.rotation = newTrans.rotation;


        if (photonView.IsMine)
        {
            playerModel.SetActive(false);

            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        playerModel.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber % allSkins.Length]; 
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (!UIController.instance.optionScreen.activeInHierarchy)
            {
                mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
                verticalRotStore += mouseInput.y;
                verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);


                if (invertLook)
                {
                    viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

                }
                else
                {
                    viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
                }


                moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));


                if (Input.GetKey(KeyCode.LeftShift))
                {
                    activeMoveSpeed = runSpeed;
                }
                else
                {
                    activeMoveSpeed = moveSpeed;
                }

                float yVel = movement.y;
                movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
                movement.y = yVel;
                if (charCon.isGrounded)
                {
                    movement.y = 0;
                }

                isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

                if (Input.GetButtonDown("Jump") && isGrounded)
                {
                    movement.y = jumpForce;
                }


                movement.y += Physics.gravity.y * Time.deltaTime * gravityMode;

                //transform.position += movement * moveSpeed * Time.deltaTime;
                charCon.Move(movement * Time.deltaTime);


                if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
                {
                    muzzleConter -= Time.deltaTime;
                    if (muzzleConter <= 0)
                    {
                        allGuns[selectedGun].muzzleFlash.SetActive(false);
                    }
                }


                if (!overHeated)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Shoot();
                    }
                    if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
                    {
                        shootControler -= Time.deltaTime;

                        if (shootControler <= 0)
                        {
                            Shoot();
                        }
                    }
                    heatCounter -= coolRate * Time.deltaTime;
                }
                else
                {
                    heatCounter -= overbeatCoolRate * Time.deltaTime;
                    if (heatCounter <= 0)
                    {
                        heatCounter = 0;
                        overHeated = false;
                        UIController.instance.overheatedMessage.gameObject.SetActive(false);
                    }
                }
                if (heatCounter < 0)
                {
                    heatCounter = 0f;
                }

                UIController.instance.weaponTempSlider.value = heatCounter;





                if (Input.GetAxisRaw("MouseScrollWheel") > 0f)
                {
                    selectedGun++;
                    if (selectedGun >= allGuns.Length)
                    {
                        selectedGun = 0;
                    }
                    //SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }
                else if (Input.GetAxisRaw("MouseScrollWheel") < 0f)
                {
                    selectedGun--;
                    if (selectedGun < 0)
                    {
                        selectedGun = allGuns.Length - 1;
                    }
                    //SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }


                for (int i = 0; i < allGuns.Length; i++)
                {
                    if (Input.GetKeyDown((i + 1).ToString()))
                    {
                        selectedGun = i;
                        //SwitchGun();
                        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                    }
                }

                animator.SetBool("grounded", isGrounded);
                animator.SetFloat("speed", moveDir.magnitude);

                if (Input.GetMouseButton(1))
                {
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, allGuns[selectedGun].adsZoom, adsSpeed * Time.deltaTime);
                    gunHolder.position = Vector3.Lerp(gunHolder.position, adsInPoint.position, adsSpeed * Time.deltaTime);
                }
                else
                {
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adsSpeed * Time.deltaTime);
                    gunHolder.position = Vector3.Lerp(gunHolder.position, adsOutPoint.position, adsSpeed * Time.deltaTime);
                }


                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                else if (Cursor.lockState == CursorLockMode.None)
                {
                    if (Input.GetMouseButtonDown(0) && !UIController.instance.optionScreen.activeInHierarchy)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                }
            }
        }
    }


    private void Shoot ()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f)); // Center point of screen
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log("We hit " + hit.collider);

            if (hit.collider.gameObject.tag == "Player")
            {
                Debug.Log("We hit " + hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject buleImpactObject = Instantiate(buletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(buleImpactObject, 10f);
            }
        }


        shootControler = allGuns[selectedGun].timeBetwenSots;

        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleConter = muzzleDisplayTime;
    }

    [PunRPC]
    public void DealDamage(string damager, int damegeAmont, int actor)
    {
        TakeDamage(damager, damegeAmont, actor);
    }

    public void TakeDamage(string damager, int damegeAmont, int actor)
    {
        //Debug.Log(photonView.Owner.NickName + " has been hit " + damager);
        //gameObject.SetActive(false);
        if (photonView.IsMine)
        {
            currentHealth -= damegeAmont;            

            if (currentHealth <= 0)
            {
                PlayerSpawner.instance.Die(damager);

                MatchManeger.instance.UpdateStatSend(actor, 0, 1);
            }

            UIController.instance.healthSlider.value = currentHealth;
        }
    }



    private void LateUpdate()
    {

        if (photonView.IsMine)
        {
            if (MatchManeger.instance.state == MatchManeger.GameState.Playing)
            {
                cam.transform.position = viewPoint.position;
                cam.transform.rotation = viewPoint.rotation;
            }
            else
            {
                cam.transform.position = MatchManeger.instance.mapCamPoint.position;
                cam.transform.rotation = MatchManeger.instance.mapCamPoint.rotation;
            }
        }
    }

    void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);

        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if(gunToSwitchTo < allGuns.Length)
        {
            selectedGun = gunToSwitchTo;
            SwitchGun();
        }
    }
}
