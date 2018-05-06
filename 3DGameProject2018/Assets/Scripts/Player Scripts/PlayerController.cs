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
[RequireComponent(typeof(CollisionBehaviour))]
public class PlayerController : MonoBehaviour, IWater
{

    /******************/
    /*Member Variables*/

    [HideInInspector] public int currentPlayers, playerNumber;
     public float rotationV = 0, rotationH, maxRotV = 65f, minRotV = -60f;


    #region Weapon

        [SerializeField, Tooltip ("Switches to picked up weapon instantly.")]
        private bool switchOnPickup;
        [SerializeField, Tooltip ("Picks up new weapon instantly if space available")]
        private bool autoPickup;
        [Tooltip("Does weapon reload automatically when empty.")]
        public bool autoReload;
        [Tooltip ("Add initial weapon(s) from prefabs. Should always have at least one weapon.")]
        public bool toggleRun;
        public List<Weapon> carriedWeapons; //All current weapons under gunsParent.
        [SerializeField]
        private int maxWeapons = 2, maxGlobalAmmo = 150;

        private Weapon currentWeapon; //Active weapon
        private Weapon defaultWeapon; //Reference to a prefab, taken from first carriedWeapons list item.
        private Drops pickupDrop; //Updates every time weapon drop is nearby
        private WeaponData pickupData; //Updates every time weapon drop is nearby
        private int clipSize, currentAmmo, globalAmmo;
        private int weaponIndex = 0; //Current weapon in carriedWeapons
        private bool pickupAllowed = false;

    #endregion
    #region Player Data

        [HideInInspector]public bool hasJumped = false;
        [HideInInspector]public bool hasShot = false;
        [HideInInspector]public bool hasReloaded = false;
        [HideInInspector]public bool hasSprinted = false;
        [HideInInspector]public bool hasSwitchedWeapon = false;

    #endregion

    private int currentHealth;
    private int deaths = 0, kills = 0;
    private float playerSpeed;
    private Vector3 aimWorldPoint; // Where gun rotates towards.

    private bool isAlive = true;
    private bool isAimRaycastHit = false; //Used for aiming the center of screen
    private float currentDamage = 0;
    private float runningTimer = 0, movingTimer = 0, interactTimer = 0, swapTimer = 0;
    private bool isSwapped = false; //Was weapon swapped already
    private int maxHealth = 100;
    //Classes
    public CanvasOverlayHandler canvasOverlay;
    public GameObject playerHead; //Has head collider for headshots.
    public GameObject playerTorso;
    public GameObject gunsParent; //Position always same as camera. Weapon script uses this to parent guns.
    public HudHandler hud; //Draws player-specific hud inside camera viewport
    public RigController rigController; //Controls player model animator state & physics (ragdoll)
    public ArmRigController armRigController; //Controls first-person arms.
    private CameraHandler cameraHandler;
    [HideInInspector]
    public MatchController controller;
    public PlayerStats stats;
    public Animator playerAnim;

    public LayerMask raycastLayerMask;
    [FMODUnity.EventRef] public string jumpSE;
    //Movement Variables
    public float lookSensV = 0.8f, lookSensH = 1f;
    public bool invertSensV = false;
    public Vector3 tempVel;
    public float jumpVelocity = 1f;
    public float gravity = 5f;
    public float maxVelocity;
    private bool isGrounded = true, isRunning = false;
    public float walkSpeed = 7f;
    public float runMultiplier = 1.5f;
    public float maxSlope = 60;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private float prevVelocity;
    private Vector2 movementMagnitude; //Store leftHorizontal & rightHorizontal inputs
    public bool helpfulTips = true;

    private List<Collider> damageColliders; //Colliders that take damage
    private CapsuleCollider capsule;
    //possibly a list of who damaged you as well so we could give people assists and stuff
    //wed have to run off a points system that way so id rather keep it to k/d right now or time because they are both easy
    private List<Effects> currentEffects;
    private float acceleration;
    private Collider[] m_colliders = new Collider[15];
    public int maximumPhysicsLoops = 10;
    //Variable for transforming platforms
    private Collider lastCollider;
    private Vector3 lastColliderPos;
    private Quaternion lastColliderRot;
    private RaycastHit lastColliderHit;
    private Vector3 VelocityBuffer;
    public float airFriction = 1;
    private int addVelocity = 0;

    #region Getters & Setters

    //Setters now trigger functions in hud when values change.
    //by including values, hud doesnt need to have reference on this script.

    public Weapon CurrentWeapon
        {
            get{return currentWeapon;}
        }
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
                else if (value >= maxGlobalAmmo)
                    globalAmmo = maxGlobalAmmo;
                else if (value < 0)
                    globalAmmo = 0;
                else
                    globalAmmo = value;

                hud.UpdateAmmo();
            }
        }
        public float CurrentDamage
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
        public bool IsAlive
        {
            get { return isAlive; }
        }

        public MatchController Controller
        {

            set {
                controller = value;
            }
        }

        public Recoil RecoilScript
        {
            get {return currentWeapon.RecoilScript;}
        }

    #endregion


    /************************/
    /*MonoBehavior Functions*/
    private void Awake()
    {
        if (!GameObject.FindGameObjectWithTag("HUD Overlay"))
        {
            canvasOverlay = Instantiate(canvasOverlay, Vector3.zero, Quaternion.Euler(0,0,0));
            canvasOverlay.SetOverlay(currentPlayers);
        }
        damageColliders = new List<Collider>();
        foreach(var col in playerHead.GetComponents<Collider>())
            damageColliders.Add(col);
        foreach (var col in playerTorso.GetComponents<Collider>())
            damageColliders.Add(col);

        hud = Instantiate(hud, Vector3.zero, Quaternion.Euler(0,0,0));
        hud.playerController = this;

        GlobalAmmo = maxGlobalAmmo;
        CurrentHealth = maxHealth;
        playerSpeed = walkSpeed;

        //Set own camera for players by their number. Scene already has 4 inactive cameras ready.
        GameObject playerCamera = null;
        Transform[] trans = GameObject.Find("Cameras").GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trans) {
            if (t.gameObject.name == "Main Camera" + (playerNumber)) {
                playerCamera = t.gameObject;
                playerCamera.SetActive(true);
                var canvas = hud.GetComponent<Canvas>();
                canvas.worldCamera = playerCamera.GetComponent<Camera>();
            }
        }
        cameraHandler = playerCamera.GetComponent<CameraHandler>();
        cameraHandler.playerController = this;
        cameraHandler.target = playerHead;
        cameraHandler.SetViewport(currentPlayers, playerNumber);
        hud.transform.parent = cameraHandler.transform;
        rigController = GetComponentInChildren<RigController>();
        armRigController = GetComponentInChildren<ArmRigController>();
        collisionBehaviour = GetComponent<CollisionBehaviour>();

        stats = new PlayerStats {
            player = playerNumber
        };

        if (carriedWeapons.Count == 0)
            Debug.LogError("CarriedWeapons count should always be at least 1!");
        else
        {
            weaponIndex = 0;
            defaultWeapon = carriedWeapons[weaponIndex]; //Default weapon always asset reference, not instantiated object

            for(int i = 0; i < carriedWeapons.Count; i++)
            {
                string name = carriedWeapons[i].name; //Get asset name before replacing with instatiated object.
                carriedWeapons[i] = Instantiate(carriedWeapons[i], transform.position, transform.rotation);
                carriedWeapons[i].name = name; //Assign asset name. Prevents being named as "(clone)"
                carriedWeapons[i].playerController = this;
                carriedWeapons[i].Initialize();
                carriedWeapons[i].Deactivate();
            }

            //Create gun from the first item in carriedWeapons list & give parameters
            currentWeapon = carriedWeapons[weaponIndex];

        }

    }

    private void Start()
    {
        ActivateWeapon();
        rotationH = transform.localEulerAngles.y;
        hud.UpdateAmmo();
        capsule = GetComponent<CapsuleCollider>();
        gunsParent.transform.localPosition += cameraHandler.offsetFromHead;

    }
    //Physics
    private void FixedUpdate()
    {

        if(!controller.IsPaused && isAlive)
        {

            acceleration = (acceleration - gravity * Time.deltaTime);//calculate acceleration


            //After jumping off a moving platform keep velocity, and deselerate at speed of airFriction
            if(!isGrounded)
            {
                VelocityBuffer = VelocityBuffer.normalized * Mathf.Max(0, VelocityBuffer.magnitude - airFriction * Time.deltaTime);
                addVelocity = 1;
            }


            velocity = transform.forward * tempVel.x;//Add the current relative forward or backwards input
            velocity += Vector3.Cross(transform.forward, Vector3.up) * tempVel.z;//Add the current relative left or right input
            tempVel = Vector3.zero;//Zero temp to wait for new input
            velocity.y = acceleration * Time.deltaTime;//add curent acceleration
            velocity += VelocityBuffer * addVelocity * Time.deltaTime;//add the velocity buffer only is in effect when addvelocity is set to 1
            addVelocity = 0;//set our int flag for adding velocityBuffer back to 0
            velocity.y = Mathf.Clamp(velocity.y, -maxVelocity, maxVelocity);
            isGrounded = false;//set grounded false until collision proves it should be

            Pushout();

            /*Colission Loop variables*/
            int maxLoops = 0;
            Vector3 loopStartVec;
            RaycastHit hit;
            //Top and bottom sphere of player capsule used for capsule checks
            Vector3 TopSphere = transform.position + new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center;
            Vector3 BotSphere = transform.position - new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center;
            /*Main Collision Detection*/
            while(Physics.CapsuleCast(
                    TopSphere, BotSphere, capsule.radius,
                    velocity.normalized,//Direction vector
                    out hit,
                    Mathf.Abs(velocity.magnitude),//Direction Length
                    raycastLayerMask))
            {
                maxLoops++;//Loop count Tracker
                loopStartVec = velocity;

                Vector3 XZ = new Vector3(velocity.x, 0, velocity.z);
                if((-hit.normal.y + 1) * 90 < maxSlope)//hit normal is 1 for flat and 0 for wall so i invert that then times it by 90 to get the proper degree then compare it
                {
                    /*Traversable slope*/
                    //currently will step over anything less than 1/4th the radius of capsule
                    Vector3 perpPlaneDir = Vector3.Cross(hit.normal, XZ);//this is a vector that will be parrallel to the slope but it will be perpindicular to the direction we want to go
                    Vector3 planeDir = Vector3.Cross(perpPlaneDir, hit.normal);//This will be the an axis line of were we are walking, but we dont know if its forwards or backwards right now
                    planeDir = planeDir * Mathf.Sign(Vector3.Dot(planeDir, XZ));//dot returns pos if they are headed in the same direction. so multiplying the planedir by its sign will give us the correct direction on the vector
                    velocity = velocity.normalized * (hit.distance);//this will set velocity to go the un obstructed amount of the cast
                    XZ -= new Vector3(velocity.x, 0, velocity.z);//this makes xv the remainder xv distance
                    velocity += planeDir.normalized * XZ.magnitude;// / Mathf.Cos(Vector3.Angle(XV, planeDir.normalized)));//adds our plane direction of lenght xv if it were strethced to cover the same xv distance on our plane(so it doesnt slow down on slopes)
                    velocity.y += Mathf.Sign(hit.normal.y) * .001f;//sets the final position slightly offset from the ground so we dont clip through next frame
                    //ground and remove acceleration
                    isGrounded = true;
                    acceleration = 0;
                    //Set last transform information
                    lastCollider = hit.collider;
                    lastColliderPos = hit.collider.transform.position;
                    lastColliderRot = hit.collider.transform.rotation;
                    lastColliderHit = hit;
                } else
                {
                    //get the vectors of the plane we hit
                    Vector3 parallelSide = Vector3.Cross(Vector3.up, hit.normal);
                    Vector3 parallelDown = Vector3.Cross(parallelSide, hit.normal);
                    //Correct the direction of the xz vector on our plane
                    parallelSide = parallelSide * Mathf.Sign(Vector3.Dot(parallelSide, XZ));
                    float angle = Vector3.Angle(-new Vector3(velocity.x, 0, velocity.z).normalized, hit.normal);//get our angle between our direction and the colliders normal
                    float VerticalRemainder = velocity.y;
                    float XZPlaneRemainder = XZ.magnitude;
                    velocity = velocity.normalized * (hit.distance);//set our vector to the hit
                    VerticalRemainder -= velocity.y;//Get the acurate remainder of y from our origional y
                    XZPlaneRemainder -= new Vector3(velocity.x, 0, velocity.z).magnitude;//Get our xz remainder from initial xz vector
                    if(velocity.y <= 0)
                    {
                        velocity += parallelDown.normalized * Mathf.Abs(VerticalRemainder);//if we are heading down slide down the slope
                    } else
                    {
                        velocity.y -= VerticalRemainder * .1f;// if we hit a slope while going up reflect the remaining velocity down 
                    }
                    velocity += parallelSide.normalized * (XZPlaneRemainder * (angle / 90));//get our xzremainder and add it reduced by the angle we hit the collider at
                    velocity += hit.normal * .001f;//offset our velocity a tad so it doesnt jitter or clip through next frame
                    acceleration = velocity.y / Time.deltaTime;// set our acceleration either back to origional acceleration or smaller if we hit something, so its ready for the next acceleration calculation
                    isGrounded = false;
                }


                /*Loop Breaks*/
                //if we loop too much just exit and set velocity to just before collision and exit
                if(maxLoops == maximumPhysicsLoops)
                {
                    Debug.Log("max loops");
                    velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                    break;
                }
                //if last position velocity was the same as the new calculation set velocity to just before collision and exit
                if(loopStartVec == velocity)
                {
                    Debug.Log("Same vector");
                    velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                    break;
                }

            }

            /*Fall Back Fill Check*/
            if(Physics.CheckCapsule(TopSphere + velocity, BotSphere + velocity, capsule.radius, raycastLayerMask))
            {
                Debug.Log("End Capsule Fail");
                //Ditch Velocity
                velocity = Vector3.zero;
            }
            transform.position += velocity;

            playerAnim.SetBool("isGrounded", isGrounded);


            if(((isRunning && !toggleRun) || (movementMagnitude.magnitude < 0.5f)) && runningTimer > 0.1f)
            {
                isRunning = false;
                cameraHandler.NewFov(1); // 1 = original fov
                playerSpeed = walkSpeed;
            }
            if(movingTimer > 0.1f)
            {
                playerAnim.SetBool("isMoving", false);
            }
        }
    }
    private void Update()
    {
        //Timers
        runningTimer += Time.deltaTime;
        movingTimer += Time.deltaTime;

        gunsParent.transform.rotation = playerHead.transform.rotation;
    }
    private void LateUpdate()
    {
        if(!controller.IsPaused && isAlive && lastCollider != null)
        {
            //if we are grounded translate and rotate the same as our object
            //                                                                          /////////////////////////////////////////////
            //--------------------------------------------------------------------------//Doesnt stop translating after jumping off//
            //                                                                          /////////////////////////////////////////////
            Vector3 posDifference = lastCollider.transform.position - lastColliderPos;
            if(posDifference != Vector3.zero)
            {
                Debug.Log("Ground Translate");
                transform.position += posDifference;
                VelocityBuffer = posDifference;
            }
            if(Quaternion.Angle(lastCollider.transform.rotation, lastColliderRot) != 0)
            {
                Debug.Log("Ground Rotate");
                Quaternion relative = Quaternion.Inverse(lastColliderRot) * lastCollider.transform.rotation;// get the difference quaternion between our old and new rot
                // get a new position from a vector to the bottom of the capsule from the origion of the object, rotate by our difference quat, then put it back to world space and add offset to origin
                //calculate were the origional intersect point is on the plane
                Vector3 groundingpoint = transform.position - new Vector3(0, capsule.height / 2 - capsule.radius, 0) - lastColliderHit.normal.normalized * capsule.radius - lastColliderPos;
                //rotate a normal based on that hit to were the planes new normal is
                Vector3 newNormal = relative * lastColliderHit.normal;
                //take the new ground point then add were the new position of the capsule should be relative to where the capsule is tangent with the collision
                Vector3 trans = (relative * (groundingpoint)) + newNormal.normalized * capsule.radius + new Vector3(0, capsule.height / 2 - capsule.radius, 0) + lastColliderPos;
                transform.position = trans;
            }
            transform.position += new Vector3(0, .002f, 0);//buffer the y slightly
            // update our last transform information
            lastColliderPos = lastCollider.transform.position;
            lastColliderRot = lastCollider.transform.rotation;
        }
    }

    #region Implementations

    public float psSplashSizeMultiplier = 10f;
        public ParticleSplash psSplash;

        public ParticleSplash ParticleSplash
        {
            get{ return psSplash;}
            set{ psSplash = value; }
        }

        public CollisionBehaviour collisionBehaviour;
        public CollisionBehaviour ColBehaviour
        {
            get{ return collisionBehaviour; }
            set{ collisionBehaviour = value; }
        }
        public float SplashSizeMultiplier
        {
            get{ return psSplashSizeMultiplier; }
            set{ psSplashSizeMultiplier = value; }
        }
        public void WaterInteraction(){
            //do interaction
            if (isAlive)
            {
                velocity.y = 0;
                Die(null);
                collisionBehaviour.soundBehaviour.WaterSplash();
            }
        }

        //shoots, moves, interacts, shows scores, pauses if pressed button
        public void InputHandle(string[] input)
        {
            if (isAlive)
            {
                if(input[1] == "LeftHorizontal" || input[1] == "LeftVertical"|| input[1] == "L3")
                {
                    Move(input[1], float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
                }
                if(input[1] == "RightHorizontal" || input[1] == "RightVertical")
                {
                    Rotate(input[1],float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
                }
                if (input[1] == "A" || input[1] == "L2")
                {
                    Jump();
                }
                if (input[1] == "Y") //Used for world interactions such as pickup and swapping weapons
                {
                    if (interactTimer < Time.time - 0.1f) // If button has been released
                    {
                        swapTimer = Time.time;
                        isSwapped = false;
                        if (carriedWeapons.Count < maxWeapons)
                        {
                            PickupWeapon();
                        }
                    }
                    else // If button is being held down
                    {
                        if (carriedWeapons.Count >= maxWeapons && !isSwapped && pickupAllowed)
                        {
                            if (swapTimer < Time.time - 1f)
                            {
                                isSwapped = true;
                                SwapWeapon();
                            }
                            else
                            {
                                float uiTime = Mathf.Clamp((Time.time - swapTimer),0f,1f);
                                hud.UpdateCircleTimer(uiTime);
                            }
                        }
                        else
                        {
                            swapTimer = Time.time;
                        }
                    }

                    interactTimer = Time.time;
                    
                }
                if (input[1] == "B")
                {
                    if (Time.time - interactTimer > 0.1f)
                    {
                        DropWeapon(false);
                    }
                    interactTimer = Time.time;
                }
                if (input[1] == "R1")
                {
                    if (Time.time - interactTimer > 0.1f)
                    {
                        SwitchWeapon(1+weaponIndex, true);
                    }
                    interactTimer = Time.time;
                }
                if (input[1] == "R2")
                {
                    currentWeapon.Shoot(float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
                    hasShot = true;
                }
                if (input[1] == "L1")
                {
                    currentWeapon.Reload();
                    hasReloaded = true;
                }
            }

        }

    #endregion

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
                // gunsParent.transform.localEulerAngles = new Vector3(rotationV, 0, 0);

                break;
        }
    }
    //apply nongravity movements

    private void Jump()
    {
        if(isGrounded)
        {
            playerAnim.SetTrigger("isJumping");
            acceleration += jumpVelocity;
            FMODUnity.RuntimeManager.PlayOneShotAttached(jumpSE, gameObject);
            isGrounded = false;
            hasJumped = true;
        }
    }
    private void Move(string axis, float magnitude)
    {
        switch(axis)
        {
            case "L3":

                if(!isRunning && runningTimer > 0.1f)
                {
                    isRunning = true;
                    cameraHandler.NewFov(1.2f);
                    playerSpeed = walkSpeed * runMultiplier;
                } else if(toggleRun && runningTimer > 0.1f && isRunning)
                {
                    isRunning = false;
                    cameraHandler.NewFov(1f);
                    playerSpeed = walkSpeed * runMultiplier;
                }

                hasSprinted = true;
                runningTimer = 0;
                break;

            case "LeftHorizontal":
                movementMagnitude.x = magnitude;
                tempVel.z -= playerSpeed * magnitude * Time.deltaTime;
                playerAnim.SetBool("isMoving", true);
                playerAnim.SetFloat("sideways", magnitude);
                movingTimer = 0;
                break;

            case "LeftVertical":
                Debug.Log("vert");
                movementMagnitude.y = magnitude;
                tempVel.x -= playerSpeed * magnitude * Time.deltaTime;
                playerAnim.SetBool("isMoving", true);
                playerAnim.SetFloat("forward", Mathf.Clamp(-magnitude, -0.9f, 0.9f));
                if(isRunning)
                    playerAnim.SetFloat("forward", -magnitude);
                movingTimer = 0;
                break;


            default:
                break;
        }
    }


    public void PlatformJump(float multiplier) {

        acceleration = jumpVelocity * multiplier;
        isGrounded = false;
    }
    private void Pushout()
    {
        /*Checks that the player doesnt start in something*/
        int numOverlaps = 1;
        numOverlaps = Physics.OverlapCapsuleNonAlloc(transform.position + new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center, transform.position - new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center, capsule.radius, m_colliders, raycastLayerMask, QueryTriggerInteraction.UseGlobal);
        for(int i = 0; i < numOverlaps; i++)
        {
            Debug.Log("Start Capsule Fail");
            Vector3 direction;
            float distance;
            if(Physics.ComputePenetration(capsule, transform.position, transform.rotation, m_colliders[i], m_colliders[i].transform.position, m_colliders[i].transform.rotation, out direction, out distance))
            {
                Vector3 penetrationVector = direction * distance;
                Vector3 velocityProjected = Vector3.Project(velocity, -direction);
                transform.position = transform.position + penetrationVector;
                velocity -= velocityProjected;
            }
        }
    }

    #region Weapon Functions

    //Gets called when triggered near a gun pickup.
    public void AllowPickup(Drops drop, bool isAllowed, WeaponData data)
    {
        // Debug.Log("AllowPickup called. drop: " + drop + ", isAllowed: " + isAllowed + ", weapondata: " + data);
        pickupDrop = drop;
        pickupAllowed = isAllowed;
        pickupData = data;

        if (pickupAllowed)
        {
            //Check if you have this weapon & if space for ammo. Destroy pickup if took any ammo
            foreach (var weapon in carriedWeapons)
            {
                if (weapon.name == pickupDrop.pickupWeapon.name)
                {
                    //Already have this weapon, trying to take ammo
                    int oldGlobalAmmo = GlobalAmmo;
                    GlobalAmmo += pickupData.currentClipAmmo;
                    if (oldGlobalAmmo != GlobalAmmo)
                    {
                        Destroy(pickupDrop.gameObject);
                    }
                    return;
                }
            }

            //If no space in inventory, update instruction text for swapping.
            if (carriedWeapons.Count >= maxWeapons)
                hud.UpdateMiddleInstructions(pickupAllowed,pickupDrop.pickupWeapon.gameObject.name, GamepadButton.Y);

            else if (carriedWeapons.Count < maxWeapons)
            {
                if (autoPickup)
                    PickupWeapon();
                else
                    hud.UpdateMiddleInstructions(pickupAllowed, pickupDrop.pickupWeapon.gameObject.name, GamepadButton.Y);
            }

        }
        else
        {
            //If pickup not allowed, disable instruction text
            hud.UpdateMiddleInstructions(pickupAllowed,"", GamepadButton.Y);
        }
    }

    //Picks up new weapon to carry
    public void PickupWeapon()
    {
        if (isAlive && pickupDrop != null && pickupAllowed)
        {

            foreach (var weapon in carriedWeapons)
            {
                if (weapon.name == pickupDrop.pickupWeapon.name)
                    return;
            }

            if (carriedWeapons.Count < maxWeapons)
            {
                Debug.Log("New weapon picked up");
                carriedWeapons.Add(Instantiate(pickupDrop.pickupWeapon, transform.position, transform.rotation));
                PersonalExtensions.CopyComponentValues<WeaponData>(pickupData, carriedWeapons[carriedWeapons.Count-1].gameObject);
                carriedWeapons[carriedWeapons.Count-1].gameObject.name = pickupDrop.pickupWeapon.gameObject.name;
                carriedWeapons[carriedWeapons.Count-1].playerController = this;
                carriedWeapons[carriedWeapons.Count-1].Initialize();
                carriedWeapons[carriedWeapons.Count-1].Deactivate();

                Destroy(pickupDrop.gameObject);

                if (switchOnPickup)
                    SwitchWeapon(carriedWeapons.Count-1, false);
            }
        }

    }

    //Switches between carried weapons
    public void SwitchWeapon(int index, bool manualSwitch)
    {
        if (index > carriedWeapons.Count-1)
            weaponIndex = 0;
        else
            weaponIndex = index;


        if (currentWeapon != carriedWeapons[weaponIndex])
        {
            if (currentWeapon)
                currentWeapon.Deactivate();

            currentWeapon = carriedWeapons[weaponIndex];
            ActivateWeapon();
        
            if (!hasSwitchedWeapon && manualSwitch)
                hasSwitchedWeapon = true;

        }

    }
    //Swaps current weapon to new and throws current away
    public void SwapWeapon()
    {
        if (!pickupAllowed)
            return;
     
        foreach (var weapon in carriedWeapons)
        {
            if (weapon.name == pickupDrop.pickupWeapon.name)
            {
                return;
            }
        }


        DropWeapon(true);
        carriedWeapons.Add(Instantiate(pickupDrop.pickupWeapon, transform.position, transform.rotation));
        weaponIndex = carriedWeapons.Count-1; //Index updates to be list's last item
        currentWeapon = carriedWeapons[weaponIndex];
        currentWeapon.name = pickupDrop.pickupWeapon.name; //Prevents name to be a "(clone)"
        PersonalExtensions.CopyComponentValues<WeaponData>(pickupData, currentWeapon.gameObject);
        currentWeapon.playerController = this;
        currentWeapon.Initialize(); // Picked up weapons need to set some initial values
        ActivateWeapon(); //SwapWeapon makes swapped weapon current & active

        Destroy(pickupDrop.gameObject);
    }

    //Autopickup must be disabled to use this function
    public void DropWeapon(bool isReplaced)
    {
        if ((carriedWeapons.Count > 1 && !autoPickup) || isReplaced) // Player must have at least one weapon
        {
            GameObject swappedGunPickup = Instantiate(currentWeapon.weaponPickup, transform.position + (transform.forward + Vector3.up)*0.7f, Quaternion.LookRotation(transform.right));
            PersonalExtensions.CopyComponentValues<WeaponData>(currentWeapon.weaponData, swappedGunPickup);
            var tempRB = swappedGunPickup.GetComponent<Rigidbody>();
            if (tempRB)
                tempRB.AddForce(transform.forward*6f, ForceMode.Impulse);
            Destroy(currentWeapon.gameObject);
            carriedWeapons.RemoveAt(weaponIndex);

            if (!isReplaced)
                SwitchWeapon(weaponIndex+1, false);
        }

    }

    private void ActivateWeapon()
    {
        currentWeapon.Activate();
        hud.hudWeaponsHandler.SetWeaponUI(maxWeapons, carriedWeapons, weaponIndex);
    }

    #endregion


    public void TakeDamage(int damage, PlayerController attacker)
    {
        hud.TakeDamage(attacker.transform.position);
        ColBehaviour.soundBehaviour.PlayTakeDamage();

        CurrentHealth -= damage;
        if(currentHealth<1 && isAlive)
        {
            attacker.stats.kills++;
            Die(attacker);
        }
    }

    //Gets called on particle collision
    //Can be used for score system later on.
    public void DealDamage()
    {
        hud.DealDamage();
    }

    public void Die(PlayerController attacker)
    {
        //Note: attacker will be null on suicide.
        if (isAlive)
        {
            isAlive = false;
            ColBehaviour.soundBehaviour.PlayDestroy(rigController.transform.position);
            cameraHandler.Die(attacker);
            rigController.Die();
            // armRigController.Die();

            foreach(Collider col in damageColliders)
            {
                col.enabled = false;
            }

            Debug.Log("Dropping weapon and creating pickup drop");
            GameObject swappedGunPickup = Instantiate(currentWeapon.weaponPickup, currentWeapon.transform.position, transform.rotation);
            PersonalExtensions.CopyComponentValues<WeaponData>(currentWeapon.weaponData, swappedGunPickup);
            foreach(Weapon weapon in carriedWeapons)
            {
                Destroy(weapon.gameObject);
            }
            StartCoroutine(RespawnTimer());
        }


    }
    private IEnumerator RespawnTimer()
    {
        int time = 0;
        while (time <= controller.RespawnTime)
        {
            hud.UpdateTimerText(controller.RespawnTime - time);
            time++;
            yield return new WaitForSeconds(1);
        }
        controller.Spawn(playerNumber);
        yield break;
    }
    public void Reset()
    {
        GlobalAmmo = maxGlobalAmmo;
        CurrentHealth = maxHealth;
        rotationH = transform.localEulerAngles.y;

        if (!isAlive)
        {
            weaponIndex = 0;
            carriedWeapons.Clear();
            carriedWeapons.Add(defaultWeapon);
            currentWeapon = Instantiate(carriedWeapons[weaponIndex], transform.position, transform.rotation);
            currentWeapon.name = carriedWeapons[weaponIndex].name; //Prevents name to be a "(clone)"
            carriedWeapons[weaponIndex] = currentWeapon;
            currentWeapon.playerController = this;

            foreach(Collider col in damageColliders)
            {
                col.enabled = true;
            }
            //Initialize basically a start function but had to be called after parameters are set
            currentWeapon.Initialize();
            //Make weapon as current active weapon
            ActivateWeapon();
            isAlive = true;
        }


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



    /******************************
     *            To Do
     *  Make associated camera follow the
     *  killer until death timer is up and
     *  call respawn
     *
     ******************************/

}