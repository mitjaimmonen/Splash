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

    public CharacterBody charBody;

    public LayerMask raycastLayerMask;
    [FMODUnity.EventRef] public string jumpSE;
    //Movement Variables
    public float lookSensV = 0.8f, lookSensH = 1f;
    public bool invertSensV = false;
 
    public bool helpfulTips = true;

    private List<Collider> damageColliders; //Colliders that take damage
    private CapsuleCollider capsule;
    //possibly a list of who damaged you as well so we could give people assists and stuff
    //wed have to run off a points system that way so id rather keep it to k/d right now or time because they are both easy
    private List<Effects> currentEffects;
    private float invertTime;
    private bool InvertBtnReleased = true;


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

        public float Acceleration
        {
            get { return charBody.Acceleration; }
            set { charBody.YInput(value); }
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
        charBody.IsRunning = false;

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
            transform.eulerAngles = new Vector3(transform.localEulerAngles.x, rotationH, 0);//right horizontal
            playerHead.transform.localEulerAngles = new Vector3(rotationV, 0, 0);
            playerAnim.SetBool("isGrounded", charBody.IsGrounded);
            if((charBody.IsRunning && !toggleRun) && runningTimer > 0.1f)
            {
                charBody.IsRunning = false;
                cameraHandler.NewFov(1); // 1 = original fov
            }
            if(movingTimer > 0.1f)
            {
                playerAnim.SetBool("isMoving", false);
            }
        }
        gunsParent.transform.rotation = playerHead.transform.rotation;
    }

    private void Update()
    {
        //Timers
        runningTimer += Time.deltaTime;
        movingTimer += Time.deltaTime;
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
                //velocity.y = 0;
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
                if (input[1] == "RightStick" && InvertBtnReleased)
                {
                    invertTime = Time.time;
                    InvertBtnReleased= false;
                    Debug.Log("INVERT BTN");
                    invertSensV = !invertSensV;
                }
                else if (input[1] != "RightStick" && invertTime + 0.1f < Time.time)
                {
                    InvertBtnReleased = true;
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
                
                break;
            case "RightVertical":
                //Face gameObject only rotates vertically.
                if (invertSensV)
                {
                    magnitude *= -1;
                    Debug.Log("INVERTED");
                }
                rotationV += magnitude * lookSensV * Time.deltaTime;
                rotationV = Mathf.Clamp(rotationV, minRotV, maxRotV);
                
                //gunsParent.transform.localEulerAngles = new Vector3(rotationV, 0, 0);

                break;
        }
    }
    //apply nongravity movements

    private void Jump()
    {
        if(charBody.IsGrounded)
        {
            playerAnim.SetTrigger("isJumping");
            charBody.Jump();
            FMODUnity.RuntimeManager.PlayOneShotAttached(jumpSE, gameObject);
            hasJumped = true;
        }
    }
    private void Move(string axis, float magnitude)
    {
        switch(axis)
        {
            case "L3":

                if(!charBody.IsRunning && runningTimer > 0.1f)
                {
                    charBody.IsRunning = true;
                    cameraHandler.NewFov(1.2f);
                } else if(toggleRun && runningTimer > 0.1f && charBody.IsRunning)
                {
                    charBody.IsRunning = false;
                    cameraHandler.NewFov(1f);
                }

                hasSprinted = true;
                runningTimer = 0;
                break;

            case "LeftHorizontal":
                //movementMagnitude.x = magnitude;
                charBody.XZInput(new Vector2(0,- magnitude * Time.deltaTime));
                playerAnim.SetBool("isMoving", true);
                playerAnim.SetFloat("sideways", magnitude);
                movingTimer = 0;
                break;

            case "LeftVertical":
                //movementMagnitude.y = magnitude;
                charBody.XZInput(new Vector2(-magnitude * Time.deltaTime,0));
                playerAnim.SetBool("isMoving", true);
                playerAnim.SetFloat("forward", Mathf.Clamp(-magnitude, -0.9f, 0.9f));
                if(charBody.IsRunning)
                    playerAnim.SetFloat("forward", -magnitude);
                movingTimer = 0;
                break;


            default:
                break;
        }
    }


    public void PlatformJump(float multiplier) {

        charBody.YInput( charBody.JumpVelocity * multiplier);
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
            stats.deaths++;
            isAlive = false;
            ColBehaviour.soundBehaviour.PlayDestroy(rigController.transform.position);
            cameraHandler.Die(attacker);
            rigController.Die(charBody.Acceleration);
            
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
            StartCoroutine(RespawnTimer(attacker));
        }


    }
    private IEnumerator RespawnTimer(PlayerController attacker)
    {
        int time = 0;
        hud.UpdateKillcamText(true, attacker);
        while (time <= controller.RespawnTime)
        {
            hud.UpdateTimerText(controller.RespawnTime - time);
            time++;
            yield return new WaitForSeconds(1);
        }
        hud.UpdateKillcamText(false, attacker);
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
            charBody.Acceleration = 0;
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
            charBody.Resetinter();
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