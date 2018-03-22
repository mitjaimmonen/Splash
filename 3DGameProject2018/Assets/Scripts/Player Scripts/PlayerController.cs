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


    [HideInInspector]
    public float rotationV = 0, rotationH, maxRotV = 80f, minRotV = -60f;
    private int currentHealth;
    private int clipSize, currentAmmo, globalAmmo, maxGlobalAmmo = 150;
    private int deaths = 0, kills = 0, damageTake = 0, damageDelt = 0;
    private float playerSpeed;
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

    private CameraHandler cameraHandler;
    private MatchController controller;


    public Animator playerAnim;
    public LayerMask raycastLayerMask;
    [FMODUnity.EventRef] public string HitmarkerSE, jumpSE;

    //Movement Variables
    public float lookSensV = 0.8f, lookSensH = 1f;
    public bool invertSensV = false;
    public Vector3 tempVel;
    public float JumpVelocity = 1f;
    public float gravity = 5f;
    public float maxVelocity;
    private bool isGrounded = true, isRunning = false;
    public float walkSpeed = 7f;
    public float runMultiplier = 1.5f;
    public float maxSlope = 60;
    public float stepHeight = 0;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private Transform prevPosition;
    private Transform thisPosition;
    private CapsuleCollider capsule;
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
        capsule = GetComponent<CapsuleCollider>();
    }

    //apply Gravity
    //do a sphere check for drops
    //visually apply all current effects
    private void Update()
    {
        //Timers
        runningTimer += Time.deltaTime;
        velocity += tempVel * Time.deltaTime;
        tempVel = Vector3.zero;
        movingTimer += Time.deltaTime;

        if (!controller.isPaused)
        {
            if (transform.position.y < -50f)
                controller.Spawn(playerNumber);

            prevPosition = thisPosition;
            thisPosition = transform;
            /*Problems left:(/ == think its fixxed X = definitly fixed)
             * [/]when hitting a 90degree meeting while jumping with the sphere of the collider it slows down y velocity
             * []very occasional clipping when jumping into 90 with head and then pushing into the object
             * []windowsils always clip???
             * []some occasional thru floor(seems only infront of the first spawn when walking towards the house, possibly also only when looking down)
             * []no real step function
            */


            /*GRAVITY*/
            velocity.y -= gravity * Time.deltaTime;


            isGrounded = false;
            Vector3 TopSphere = transform.position + new Vector3(0, (capsule.height / 2 - capsule.radius) + 0.5f, 0) + capsule.center; //0.5f is additional offset because capsule does not reach head.
            Vector3 BotSphere = transform.position - new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center;
            /*Collision Calc*/
            {
                int ohshitcounter = 0;
                RaycastHit hit;
                Vector3 lastMoveVec;
                while(Physics.CapsuleCast(
                        TopSphere,//Capsule top sphere center
                        BotSphere,//bottom sphere center
                        capsule.radius,//Capsule Radius
                        velocity.normalized,//Direction vector
                        out hit,
                        Mathf.Abs(velocity.magnitude),
                        raycastLayerMask
                        ))//Distance to cast
                        
                {
                    ohshitcounter++;//Loop count Tracker
                    lastMoveVec = velocity;

                    //if we loop too much just exit and set velocity to just before collision and exit
                    if(ohshitcounter == 10)
                    {
                        velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                        break;
                    }

                    Vector3 XZ = new Vector3(velocity.x, 0, velocity.z);
                    if(hit.normal.y > maxSlope)
                    {//Traversable slope
                     //currently will step over anything less than 1/4th the radius of capsule
                        Vector3 perpPlaneDir = Vector3.Cross(hit.normal, XZ);//this is a vector that will be parrallel to the slope but it will be perpindicular to the direction we want to go
                        Vector3 planeDir = Vector3.Cross(perpPlaneDir, hit.normal);//This will be the an axis line of were we are walking, but we dont know if its forwards or backwards right now
                        planeDir = planeDir * Mathf.Sign(Vector3.Dot(planeDir, XZ));//dot returns pos if they are headed in the same direction. so multiplying the planedir by its sign will give us the correct direction on the vector
                        velocity = velocity.normalized * (hit.distance);//this will set velocity to go the un obstructed amount of the cast
                        XZ -= new Vector3(velocity.x, 0, velocity.z);//this makes xv the remainder xv distance
                        velocity += planeDir.normalized * XZ.magnitude;// / Mathf.Cos(Vector3.Angle(XV, planeDir.normalized)));//adds our plane direction of lenght xv if it were strethced to cover the same xv distance on our plane(so it doesnt slow down on slopes)
                        velocity.y += Mathf.Sign(hit.normal.y) * .001f;
                        isGrounded = true;
                    //} else if(hit.normal.y < 0)) caused problems and it works fine without now because of changes so idk
                    //{//jumping up into things just caps you there
                    //    velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                    //    velocity.y -= gravity * Time.deltaTime;
                    } else
                    {//Wall or too steep slope
                        Vector3 parallelSide = Vector3.Cross(Vector3.up, hit.normal);
                        Vector3 parallelDown = Vector3.Cross(parallelSide, hit.normal);
                        parallelSide = parallelSide * Mathf.Sign(Vector3.Dot(parallelSide, XZ));
                        float angle = Vector3.Angle(-new Vector3(velocity.x, 0, velocity.z).normalized, hit.normal);
                        float VerticalRemainder = velocity.y;
                        float XZPlaneRemainder = XZ.magnitude;
                        velocity = velocity.normalized * (hit.distance);
                        VerticalRemainder -= velocity.y;
                        XZPlaneRemainder -= new Vector3(velocity.x, 0, velocity.z).magnitude;
                        if(velocity.y <= 0)
                        {
                            velocity += parallelDown.normalized * Mathf.Abs(VerticalRemainder);
                        } else
                        {
                            velocity.y += VerticalRemainder;
                        }
                        velocity += parallelSide.normalized * (XZPlaneRemainder * (angle / 90));
                        velocity += hit.normal * .001f;
                        isGrounded = false;
                    }
                    //if last position velocity was the same as the new calculation set velocity to just before collision and exit
                    if(lastMoveVec == velocity)
                    {
                        velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                        break;
                    }
                }
                playerAnim.SetBool("isGrounded", isGrounded);
            }

            if(Physics.CheckCapsule(TopSphere, BotSphere, capsule.radius, raycastLayerMask))
            {
                Debug.Log("Triggered");
                velocity = -velocity;
            }
            /*Apply Velocity*/
            transform.position += velocity;


            /*Next frame vertical velocity calculation & reset*/
            {
                velocity.y = Mathf.Clamp(velocity.y, -maxVelocity, maxVelocity);
                velocity.x = 0;
                velocity.z = 0;
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
                tempVel += new Vector3(transform.forward.z, 0, -transform.forward.x) * magnitude * playerSpeed;
                    playerAnim.SetBool("isMoving", true);
                    playerAnim.SetFloat("sideways", magnitude);
                movingTimer = 0;
                break;

            case "LeftVertical":
                tempVel += -transform.forward * magnitude * playerSpeed;
                playerAnim.SetBool("isMoving", true);
                playerAnim.SetFloat("forward", Mathf.Clamp(-magnitude, -0.9f, 0.9f));
                if (isRunning)
                    playerAnim.SetFloat("forward", -magnitude);
                movingTimer = 0;
                break;                
            case "A":
                if(isGrounded)
                {
                    playerAnim.SetTrigger("isJumping");
                    velocity.y += JumpVelocity;
                    FMODUnity.RuntimeManager.PlayOneShotAttached(jumpSE, gameObject);
                    isGrounded = false;
                }
                break;

            default:
                break;
        }
    }


    public void PlatformJump(float multiplier) {
        Debug.Log(multiplier);
        velocity.y = JumpVelocity * multiplier;
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