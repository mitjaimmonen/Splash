using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/********************************************
* PlayerController
* 
* Controls everything related to player
*   - Hud & canvas
*   - Weapon
*   - Inputs
*   - Transforms
*   - Camera
*   - ...
*
*   NOTE:
*   Move canvasOverlay to whichever class instantiates players.
*/

public class PlayerController : MonoBehaviour
{

    // Used for testing canvasoverlay and setting camera viewport.
    [HideInInspector]
    public int currentPlayers, playerNumber; 


    private int currentHealth;
    private int clipSize, currentAmmo, globalAmmo, maxGlobalAmmo = 150;
    private int deaths = 0, kills = 0, damageTake = 0, damageDelt = 0;
    private float playerSpeed;
    private float rotationV = 0, rotationH, maxRotV = 80f, minRotV = -60f;
    private Vector3 aimWorldPoint; // Where gun rotates towards.
    private bool isAimRaycastHit = false;
    private int currentDamage = 0;
    private float headshotMultiplier;
    private float runningTimer = 0, movingTimer = 0;
    

    private int maxHealth = 100;


    //Classes
    public CanvasOverlayHandler canvasOverlay;     
    public GameObject playerHead, gunsParent; // Takes vertical rotation, also parents all guns.
    public HudHandler hud; //Draws player-specific hud inside camera viewport
    public Weapon currentWeapon;
    public GameObject cameraPrefab;
    private CameraHandler cameraHandler;
    private MatchController controller;


    public Animator playerAnim;
    public LayerMask raycastLayerMask;
    [FMODUnity.EventRef] public string HitmarkerSE, jumpSE;

    //Movement Variables
    public float lookSensV = 0.8f, lookSensH = 1f;
    public bool invertSensV = false;
    public float JumpVelocity = 1f;
    public float gravity = 5f;
    public float maxVelocity;
    private float currentVerticalVelocity = 0;
    private bool isGrounded = true, isRunning = false;
    public float walkSpeed = 7f;
    public float runMultiplier = 1.5f;
    //possibly a list of who damaged you as well so we could give people assists and stuff
    //wed have to run off a points system that way so id rather keep it to k/d right now or time because they are both easy

    private List<Effects> currentEffects;


    #region Getters & Setters

        //Setters now trigger functions in hud when values change.
        //by including values, hud doesnt need to have reference on this script.
        public int ClipSize
        {
            get { return clipSize; }
            set 
            {
                if (value == clipSize)
                    return;
                else if (value < 0)
                    clipSize = 0;
                else
                    clipSize = value;

                hud.UpdateAmmo();
                
            }
        }
        public int CurrentAmmo
        {
            get { return currentAmmo; }
            set 
            {
                if (value == currentAmmo)
                    return;
                else if (value >= clipSize)
                    currentAmmo = clipSize;
                else if (value < 0)
                    currentAmmo = 0;
                else
                    currentAmmo = value;

                hud.UpdateAmmo();
            }
        }
        public int GlobalAmmo
        {
            get {return globalAmmo; }
            set
            {
                if (value == globalAmmo)
                    return;
                else if (value >= maxGlobalAmmo + (ClipSize - CurrentAmmo))
                    globalAmmo = maxGlobalAmmo + (ClipSize - CurrentAmmo);
                else if (value < 0)
                    globalAmmo = 0;
                else
                    globalAmmo = value;

                hud.UpdateAmmo();
            }
        }
        public int CurrentDamage
        {
            get { return currentDamage; }
            set { currentDamage = value; }
        }

        public int CurrentHealth
        {
            get{ return currentHealth; }
            set
            {
                if (value == currentHealth)
                    return;
                else if (value >= maxHealth)
                    currentHealth = maxHealth;
                else if (value < 0)
                    currentHealth = 0;
                else
                    currentHealth = value;

                hud.UpdateHealth();
            }
        }
        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }

        public Vector3 AimWorldPoint
        {
            get { return aimWorldPoint; }
            set
            {
                aimWorldPoint = value;
            }
        }
        public bool IsAimRaycastHit
        {
            get { return isAimRaycastHit; }
            set { isAimRaycastHit = value; }
        }

        public MatchController Controller
        {

            set {
                controller = value;
            }
        }

    #endregion


    private void Awake()
    {
        if (!GameObject.FindGameObjectWithTag("HUD Overlay"))
        {
            canvasOverlay = Instantiate(canvasOverlay, Vector3.zero, Quaternion.Euler(0,0,0));
            canvasOverlay.SetOverlay(currentPlayers);
        }
        
        hud = Instantiate(hud, Vector3.zero, Quaternion.Euler(0,0,0));
        hud.playerController = this;

        GlobalAmmo = maxGlobalAmmo;
        CurrentHealth = maxHealth;
        playerSpeed = walkSpeed;

        GameObject playerCamera = null;
        Transform[] trans = GameObject.Find("Cameras").GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trans) {
            if (t.gameObject.name == "Main Camera" + (playerNumber)) {
                playerCamera = t.gameObject;
                playerCamera.SetActive(true);
                var canvas = hud.GetComponent<Canvas>();
                canvas.worldCamera = playerCamera.GetComponent<Camera>();
                hud.playerNumberText.text = "<HUD debug messages>";
            }
        }
        cameraHandler = playerCamera.GetComponent<CameraHandler>();
        cameraHandler.playerController = this;
        cameraHandler.target = playerHead; // Camera gets rotation from this.
        
        

        cameraHandler.SetViewport(currentPlayers, playerNumber);

        currentWeapon.gameObject.SetActive(true);
        
    }

    private void Start() 
    {
        rotationH = transform.localEulerAngles.y;
        hud.UpdateAmmo();
    }

    //apply Gravity
    //do a sphere check for drops
    //visually apply all current effects
    private void Update()
    {
        //Timers
        runningTimer += Time.deltaTime;
        movingTimer += Time.deltaTime;



        if (!controller.isPaused)
        {
            if (transform.position.y < -50f)
                controller.Spawn(playerNumber);            

            if(!isGrounded)
            {
                currentVerticalVelocity -= gravity * Time.deltaTime;
                Mathf.Clamp(currentVerticalVelocity, -maxVelocity, maxVelocity);
                RaycastHit hit;
                if(!Physics.Raycast(transform.position, new Vector3(0, currentVerticalVelocity, 0), out hit, 1,raycastLayerMask))
                {
                    transform.position += new Vector3(0, currentVerticalVelocity * Time.deltaTime, 0);
                } else
                {
                    currentVerticalVelocity = 0;
                    isGrounded = true;
                }

            } else
            {
                if(!Physics.Raycast(transform.position, new Vector3(0, 1, 0), 2, raycastLayerMask))
                {
                    isGrounded = false;
                }
            }

            if (runningTimer > 0.1f && isRunning)
            {
                isRunning = false;
                cameraHandler.NewFov(1); // 1 = original fov
                playerSpeed = walkSpeed;
            }
            if (movingTimer > 0.1f)
            {
                playerAnim.SetBool("isMoving", false);
            }
        }
        
    }

    //shoots, moves, interacts, shows scores, pauses if pressed button
    public void InputHandle(string[] input)
    {
        if(input[1] == "LeftHorizontal" || input[1] == "LeftVertical" || input[1] == "A" || input[1] == "L3")
        {
            Move(input[1], float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
        }
        if(input[1] == "RightHorizontal" || input[1] == "RightVertical")
        {
            Rotate(input[1],float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
        }
        if (input[1] == "R2")
        {
            currentWeapon.Shoot(float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
        }
        if (input[1] == "L1")
        {
            currentWeapon.Reload();
        }
    }

    private void Rotate(string axis, float magnitude) 
    {
        switch(axis)
        {
            case "RightHorizontal":
                //Player only rotates horizontally
                rotationH += magnitude * lookSensH * Time.deltaTime;
                transform.eulerAngles = new Vector3(transform.localEulerAngles.x, rotationH, 0);
                break;
            case "RightVertical":
                //Face gameObject only rotates vertically.            
                if (invertSensV)
                    magnitude *= -1;
                rotationV += magnitude * lookSensV * Time.deltaTime;
                rotationV = Mathf.Clamp(rotationV, minRotV, maxRotV);
                playerHead.transform.localEulerAngles = new Vector3(rotationV, 0, 0);
                gunsParent.transform.localEulerAngles = new Vector3(rotationV, 0, 0);

                break;
        }
    }

    //apply nongravity movements
    private void Move(string axis, float magnitude)
    {
        switch(axis)
        {
            case "L3":
                    runningTimer = 0;            
                    if (!isRunning)
                    {
                        isRunning = true;
                        cameraHandler.NewFov(1.2f);
                        playerSpeed = walkSpeed * runMultiplier;
                    }
                break;
            case "LeftHorizontal":
                movingTimer = 0;
                if(!Physics.Raycast(transform.position, new Vector3(transform.forward.z, 0, -transform.forward.x) * magnitude * playerSpeed * Time.deltaTime,1))
                {
                    playerAnim.SetBool("isMoving", true);
                    playerAnim.SetFloat("sideways", magnitude);
                    transform.position += new Vector3(transform.forward.z, 0, -transform.forward.x) * magnitude * playerSpeed * Time.deltaTime;
                }
                break;
            case "LeftVertical":
                movingTimer = 0;
                if(!Physics.Raycast(transform.position, -transform.forward * magnitude * playerSpeed * Time.deltaTime, 1))
                {
                    playerAnim.SetBool("isMoving", true);
                    playerAnim.SetFloat("forward", magnitude);
                    transform.position += -transform.forward * magnitude * playerSpeed * Time.deltaTime;
                }
                
                break;
            case "A":
                if(isGrounded)
                {
                    FMODUnity.RuntimeManager.PlayOneShotAttached(jumpSE, gameObject);
                    currentVerticalVelocity = JumpVelocity;
                    isGrounded = false;
                }
                break;
            default:
                break;
        }
    }


    public void PlatformJump(float multiplier) {
            currentVerticalVelocity = JumpVelocity * multiplier;
            isGrounded = false;
    }


    //looks if its in a collision sphere with a weapon
    //if yes swap weapon
    //if weapons are the same the one on ground disapears and you get full ammo
    private void Interact()
    {

    }


    //simple take damage
    //if health is zero call respawn
    public void TakeDamage(int damage, Vector3 origin)
    {

        hud.TakeDamage(origin);

        CurrentHealth -= damage;
        if(currentHealth<1)
        {
            controller.Spawn(playerNumber);
        }
    }

    //Gets called on particle collision
    //Can be used for score system later on.
    public void DealDamage()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(HitmarkerSE, gameObject);
        hud.DealDamage();
    }
    public void Reset()
    {
        GlobalAmmo = maxGlobalAmmo + (ClipSize - CurrentAmmo);
        CurrentHealth = maxHealth;
        rotationH = transform.localEulerAngles.y;
    }

    //add effect to effects list and then process the effect
    //call hud display effect
    private void Effect()
    {

    }
    //undo effect shit
    private void ReleaseEffect()
    {

    }

}