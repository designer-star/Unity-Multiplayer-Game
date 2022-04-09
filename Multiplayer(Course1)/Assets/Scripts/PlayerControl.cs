﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
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


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeat;
        

        //SwitchGun();

        //Transform newTrans = SpownManager.instance.GetSpawnPoint();
        //transform.position = newTrans.position;
        //transform.rotation = newTrans.rotation;
    }

    // Update is called once per frame
    void Update()
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
        movement = ((transform.forward * moveDir.z)+ (transform.right * moveDir.x)).normalized * activeMoveSpeed;
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
       charCon.Move( movement * Time.deltaTime);


        if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleConter -= Time.deltaTime;
            if(muzzleConter <= 0)
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
            if(heatCounter <= 0)
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
            SwitchGun();
        }
        else if (Input.GetAxisRaw("MouseScrollWheel") < 0f)
        {
            selectedGun--;
            if (selectedGun < 0)
            {
                selectedGun = allGuns.Length - 1;
            }
            SwitchGun();
        }


        for (int i = 0; i < allGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                SwitchGun();
            }
        }



        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }


    private void Shoot ()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f)); // Center point of screen
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("We hit " + hit.collider);

            GameObject buleImpactObject = Instantiate(buletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Destroy(buleImpactObject, 10f);
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


    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
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
}
