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

    /******************/
    /*Member Variables*/

    [HideInInspector] public int currentPlayers, playerNumber;
    [HideInInspector] public float rotationV = 0, rotationH, maxRotV = 80f, minRotV = -60f;


    #region Weapon
        
        [SerializeField, Tooltip ("Switches to picked up weapon instantly.")]
        private bool switchOnPickup;
        [SerializeField, Tooltip ("Picks up new weapon instantly if space available")]
        private bool autoPickup;
        [SerializeField, Tooltip ("Add initial weapon(s) from prefabs. Should always have at least one weapon.")]
        private List<Weapon> carriedWeapons; //All current weapons under gunsParent.
        [SerializeField]
        private int maxWeapons = 2;

        private Weapon currentWeapon; //Active weapon
        private Weapon defaultWeapon; //Reference to a prefab, taken from first carriedWeapons list item.        
        private Drops pickupDrop; //Updates every time weapon drop is nearby
        private WeaponData pickupData; //Updates every time weapon drop is nearby
        private int clipSize, currentAmmo, globalAmmo, maxGlobalAmmo = 150;
        private int weaponIndex = 0; //Current weapon in carriedWeapons
        private bool pickupAllowed = false;

    #endregion


    private int currentHealth;
    private int deaths = 0, kills = 0;
    private float playerSpeed;
    private Vector3 aimWorldPoint; // Where gun rotates towards.

    private bool isAlive = true;
    private bool isAimRaycastHit = false; //Used for aiming the center of screen
    private int currentDamage = 0;
    private float runningTimer = 0, movingTimer = 0, interactTimer = 0, pickupTimer = 0;
    private int maxHealth = 100;
    //Classes
    public CanvasOverlayHandler canvasOverlay;
    public GameObject playerHead; //Has head collider for headshots.
    public GameObject gunsParent; //Position always same as camera. Weapon script uses this to parent guns.
    public HudHandler hud; //Draws player-specific hud inside camera viewport

    private RigController rigController; //Controls player model animator state & physics (ragdoll)
    private CameraHandler cameraHandler;
    [HideInInspector]
    public MatchController controller;
    public PlayerStats stats;
    public Animator playerAnim;

    public LayerMask raycastLayerMask;
    [FMODUnity.EventRef] public string hitmarkerSE, jumpSE, takeDamageSE, dieSE;
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
    public float climbSpeed = 1;
    public float stepHeight = 0;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private Vector3 prevVelocity;
    [SerializeField] private Collider[] damageColliders; //Colliders that take damage
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
                else if (value >= maxGlobalAmmo)
                    globalAmmo = maxGlobalAmmo;
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
                hud.playerNumberText.text = "<HUD debug messages>";
            }
        }
        cameraHandler = playerCamera.GetComponent<CameraHandler>();
        cameraHandler.playerController = this;
        cameraHandler.target = playerHead;
        cameraHandler.SetViewport(currentPlayers, playerNumber);
        
        rigController = GetComponentInChildren<RigController>();

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
            //Make weapon as current active weapon 
            currentWeapon.Activate(); 
        }

    }

    private void Start()
    {
        rotationH = transform.localEulerAngles.y;
        hud.UpdateAmmo();
        capsule = GetComponent<CapsuleCollider>();
        
    }
    //Physics
    private void Update()
    {
        //Timers
        runningTimer += Time.deltaTime;
        movingTimer += Time.deltaTime;
        interactTimer += Time.deltaTime;
        pickupTimer += Time.deltaTime;
        
        velocity += new Vector3(tempVel.x*Time.deltaTime, tempVel.y, tempVel.z * Time.deltaTime);
        tempVel = Vector3.zero;

        if (!controller.IsPaused && isAlive)
        {
            gunsParent.transform.localPosition = transform.InverseTransformPoint(cameraHandler.transform.position);
            gunsParent.transform.rotation = playerHead.transform.rotation;

            /*Problems left:(/ == think its fixxed X = definitly fixed)
             * [/]when hitting a 90degree meeting while jumping with the sphere of the collider it slows down y velocity
             * [/]very occasional clipping when jumping into 90 with head and then pushing into the object
             * [/]windowsils always clip???
             * [/]some occasional thru floor(seems only infront of the first spawn when walking towards the house, possibly also only when looking down)
             * []no real step function
             * []halway of nightmares
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
                        Debug.Log("max loops");
                        velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                        break;
                    }

                    Vector3 XZ = new Vector3(velocity.x, 0, velocity.z);
                    if(hit.normal.y > maxSlope)
                    {//Traversable slope
                     //currently will step over anything less than 1/4th the radius of capsule
                        // Debug.Log("floor");
                        Vector3 perpPlaneDir = Vector3.Cross(hit.normal, XZ);//this is a vector that will be parrallel to the slope but it will be perpindicular to the direction we want to go
                        Vector3 planeDir = Vector3.Cross(perpPlaneDir, hit.normal);//This will be the an axis line of were we are walking, but we dont know if its forwards or backwards right now
                        planeDir = planeDir * Mathf.Sign(Vector3.Dot(planeDir, XZ));//dot returns pos if they are headed in the same direction. so multiplying the planedir by its sign will give us the correct direction on the vector
                        velocity = velocity.normalized * (hit.distance);//this will set velocity to go the un obstructed amount of the cast
                        XZ -= new Vector3(velocity.x, 0, velocity.z);//this makes xv the remainder xv distance
                        velocity += planeDir.normalized * XZ.magnitude;// / Mathf.Cos(Vector3.Angle(XV, planeDir.normalized)));//adds our plane direction of lenght xv if it were strethced to cover the same xv distance on our plane(so it doesnt slow down on slopes)
                        velocity.y += Mathf.Sign(hit.normal.y) * .001f;
                        isGrounded = true;
                    } else
                    {//Wall or too steep slope
                        Debug.Log("wall");
                        if(Physics.CapsuleCast(
                        TopSphere,//Capsule top sphere center
                        BotSphere+new Vector3(0,stepHeight,0),//bottom sphere center
                        capsule.radius,//Capsule Radius
                        velocity.normalized,//Direction vector
                        out hit,
                        Mathf.Abs(velocity.magnitude),
                        raycastLayerMask
                        ))
                        {
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
                        } else
                        {

                            velocity = velocity.normalized * (hit.distance);
                            velocity.y += stepHeight;
                            velocity += hit.normal * .001f;
                            isGrounded = true;
                        }
                        
                    }
                    //if last position velocity was the same as the new calculation set velocity to just before collision and exit
                    if(lastMoveVec == velocity)
                    {
                        velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                        Debug.Log("Same vector");
                        break;
                    }
                }
                playerAnim.SetBool("isGrounded", isGrounded);
            }

            if(Physics.CheckCapsule(TopSphere+velocity, BotSphere+velocity, capsule.radius, raycastLayerMask))
            {
                Debug.Log("Triggered");
                velocity = Vector3.zero;
            }
            /*Apply Velocity*/
            transform.position += velocity;
            prevVelocity = velocity;

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

    private void OnTriggerEnter(Collider col)
    {
        if (isAlive && col.gameObject.layer == LayerMask.NameToLayer("Water"))
            Die(null);
            
    }



    /*****************/
    /*Implementations*/
    //shoots, moves, interacts, shows scores, pauses if pressed button
    public void InputHandle(string[] input)
    {
        if (isAlive)
        {
            if(input[1] == "LeftHorizontal" || input[1] == "LeftVertical" || input[1] == "A" || input[1] == "L3")
            {
                Move(input[1], float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
            }
            if(input[1] == "RightHorizontal" || input[1] == "RightVertical")
            {
                Rotate(input[1],float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
            }
            if (input[1] == "Y")
            {
                if (pickupAllowed && pickupDrop != null && pickupTimer > 0.1f)
                    PickupWeapon();
                pickupTimer = 0;
            }
            if (input[1] == "B")
            {
                if (interactTimer > 0.1f)
                {
                    DropWeapon(false);
                }
                interactTimer = 0;
            }
            if (input[1] == "R1")
            {
                if (interactTimer > 0.1f)
                {
                    SwitchWeapon(1+weaponIndex);                
                }
                interactTimer = 0;
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
                // gunsParent.transform.localEulerAngles = new Vector3(rotationV, 0, 0);

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
                if(!isRunning)
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
        
        velocity.y = JumpVelocity * multiplier;
        isGrounded = false;
    }





    //Gets called when triggered near a gun pickup.
    public void AllowPickup(Drops drop, bool isAllowed, WeaponData data)
    {
        pickupDrop = drop;
        pickupAllowed = isAllowed;
        pickupData = data;

        if (pickupAllowed && isAlive)
        {
            if (autoPickup && pickupDrop != null)
            {
                PickupWeapon();
            }
        }
    }

    //Picks up new weapon to carry
    public void PickupWeapon()
    {
        foreach (var weapon in carriedWeapons)
        {
            if (weapon.name == pickupDrop.pickupWeaponIfAny.name)
            {
                Debug.Log("Carrying this weapon already.");
                //Already have this weapon, trying to take ammo
                int oldGlobalAmmo = GlobalAmmo;
                GlobalAmmo += pickupData.currentClipAmmo;
                if (oldGlobalAmmo != GlobalAmmo)
                {
                    Debug.Log("Took ammo, destroying pickup.");
                    Destroy(pickupDrop.gameObject);
                    return;
                }
                else
                {
                    Debug.Log("Ammo full. Cannot pick up.");
                    return;
                }

            }

        }

        if (carriedWeapons.Count < maxWeapons)
        {
            Debug.Log("New weapon picked up");
            carriedWeapons.Add(Instantiate(pickupDrop.pickupWeaponIfAny, transform.position, transform.rotation));
            PersonalExtensions.CopyComponentValues<WeaponData>(pickupData, carriedWeapons[carriedWeapons.Count-1].gameObject);
            carriedWeapons[carriedWeapons.Count-1].gameObject.name = pickupDrop.pickupWeaponIfAny.gameObject.name;
            carriedWeapons[carriedWeapons.Count-1].playerController = this;
            carriedWeapons[carriedWeapons.Count-1].Initialize();
            carriedWeapons[carriedWeapons.Count-1].Deactivate();

            Destroy(pickupDrop.gameObject);

            if (switchOnPickup)
                SwitchWeapon(carriedWeapons.Count-1);
        }
        else if (!autoPickup)
            SwapWeapon();  
    }

    //Switches between carried weapons
    public void SwitchWeapon(int index)
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
            currentWeapon.Activate();

        }
        
    }
    //Swaps current weapon to new and throws current away
    public void SwapWeapon()
    {
        //Create weapon pickup and remove weapon from player.
        DropWeapon(true);
        carriedWeapons.Add(Instantiate(pickupDrop.pickupWeaponIfAny, transform.position, transform.rotation));
        weaponIndex = carriedWeapons.Count-1; //Index updates to be list's last item
        currentWeapon = carriedWeapons[weaponIndex];
        currentWeapon.name = pickupDrop.pickupWeaponIfAny.name; //Prevents name to be a "(clone)"
        PersonalExtensions.CopyComponentValues<WeaponData>(pickupData, currentWeapon.gameObject);
        currentWeapon.playerController = this;
        currentWeapon.Initialize(); // Picked up weapons need to set some initial values
        currentWeapon.Activate(); //SwapWeapon makes swapped weapon current & active

        Destroy(pickupDrop.gameObject);
    }

    public void DropWeapon(bool isReplaced)
    {
        
        if (carriedWeapons.Count > 1 || isReplaced) // Player must have at least one weapon
        {
            GameObject swappedGunPickup = Instantiate(currentWeapon.weaponPickup, currentWeapon.transform.position, transform.rotation);
            PersonalExtensions.CopyComponentValues<WeaponData>(currentWeapon.weaponData, swappedGunPickup);
            Destroy(currentWeapon.gameObject);
            carriedWeapons.RemoveAt(weaponIndex);

            if (!isReplaced)
                SwitchWeapon(weaponIndex+1);
        }

    }


    public void TakeDamage(int damage, PlayerController attacker)
    {
        hud.TakeDamage(attacker.transform.position);
        FMODUnity.RuntimeManager.PlayOneShotAttached(takeDamageSE, gameObject);

        CurrentHealth -= damage;
        if(currentHealth<1 && isAlive)
        {
            attacker.stats.kills++;
            Die(attacker);
            // controller.Spawn(playerNumber);
        }
    }

    //Gets called on particle collision
    //Can be used for score system later on.
    public void DealDamage()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(hitmarkerSE, gameObject);
        hud.DealDamage();
    }

    public void Die(PlayerController attacker)
    {
        //Note: attacker will be null on suicide.
        if (isAlive)
        {
            isAlive = false;
            FMODUnity.RuntimeManager.PlayOneShotAttached(dieSE, rigController.gameObject); //Arm is part of rig, so sound updates wherever rig has ragdolled.
            cameraHandler.Die(attacker);
            rigController.Die();

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
            hud.UpdateTimer(controller.RespawnTime - time);
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
            currentWeapon.Activate();
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