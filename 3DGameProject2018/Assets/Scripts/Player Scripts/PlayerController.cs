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
    - ...
*
*/

public class PlayerController : MonoBehaviour
{
    private int currentHealth, maxHealth = 100;
    private int clipSize, currentAmmo, globalAmmo, maxGlobalAmmo = 150;
    private int deaths = 0, kills = 0, damageTake = 0, damageDelt = 0;
    private MatchController controller;
    private float rotationV = 0, rotationH = 0, maxRotV = 80f, minRotV = -80f;

    public int currentPlayers; // Used for testing

    public int currentDamage = 0;

    //Classes
    public GameObject playerFace; // Takes vertical rotation, also parents all guns.
    public HudHandler hud; //Draws player specific hud inside camera viewport
    public CanvasOverlayHandler canvasOverlay; //Draws some shit on top of all viewports
    public Weapon currentWeapon;
    public CameraHandler cameraHandler;
    //Movement Variables
    public float lookSensV = 0.8f, lookSensH = 1;
    public bool invertSensV = false;
    public float JumpVelocity = 1;
    public float gravity = 1f;
    public float maxVelocity;
    private float currentVerticalVelocity = 0;
    private bool isGrounded = true;
    public float speed = 2;
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

                hud.UpdateAmmo(globalAmmo, clipSize, currentAmmo);
                
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

                hud.UpdateAmmo(globalAmmo, clipSize, currentAmmo);
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

                hud.UpdateAmmo(globalAmmo, clipSize, currentAmmo);
            }
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

                hud.UpdateHealth(maxHealth, currentHealth);
            }
        }

    #endregion


    private void Awake()
    {
        globalAmmo = maxGlobalAmmo;
        
        cameraHandler = Instantiate(cameraHandler, Vector3.zero, Quaternion.Euler(0,0,0));
        cameraHandler.target = playerFace; // Camera gets rotation from this.
        hud = Instantiate(hud, Vector3.zero, Quaternion.Euler(0,0,0));
        canvasOverlay = Instantiate(canvasOverlay, Vector3.zero, Quaternion.Euler(0,0,0));

        //Temporary values until we get the real player amount and can store which player this is.
        cameraHandler.SetViewport(currentPlayers, 1);
        canvasOverlay.SetOverlay(currentPlayers);

        currentWeapon.gameObject.SetActive(true);

        CurrentHealth = 15;

        // hud.playerController = this;
    }

    //apply Gravity
    //do a sphere check for drops
    //visually apply all current effects
    private void Update()
    {
        if(!isGrounded)
        {
            currentVerticalVelocity -= gravity;
            Mathf.Clamp(currentVerticalVelocity, -maxVelocity, maxVelocity);
            RaycastHit hit;
            if(!Physics.Raycast(transform.position, new Vector3(0, currentVerticalVelocity, 0), out hit, 1))
            {
                transform.position += new Vector3(0, currentVerticalVelocity, 0);
            } else
            {
                currentVerticalVelocity = 0;
                isGrounded = true;
            }

        } else
        {
            if(!Physics.Raycast(transform.position, new Vector3(0, 1, 0), 2))
            {
                isGrounded = false;
            }
        }
        
    }

    //shoots, moves, interacts, shows scores, pauses if pressed button
    public void InputHandle(string[] input)
    {
        Debug.Log(input[1]);
        if(input[1] == "LeftHorizontal" || input[1] == "LeftVertical" || input[1] == "A" )
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
                rotationH += magnitude * lookSensH;
                transform.eulerAngles = new Vector3(transform.localEulerAngles.x, rotationH, 0);
                break;
            case "RightVertical":
                //Face gameObject only rotates vertically.            
                if (invertSensV)
                    magnitude *= -1;
                rotationV += magnitude * lookSensV;
                rotationV = Mathf.Clamp(rotationV, minRotV, maxRotV);
                playerFace.transform.localEulerAngles = new Vector3(rotationV, 0, 0);
                break;
        }
    }

    //apply nongravity movements
    private void Move(string axis, float magnitude)
    {
        switch(axis)
        {
            case "LeftHorizontal":
                transform.position += new Vector3(transform.forward.z, 0, -transform.forward.x) * magnitude * speed * Time.deltaTime;
                break;
            case "LeftVertical":
                transform.position += -transform.forward * magnitude * speed * Time.deltaTime;
                break;
            case "A":
                if(isGrounded)
                {
                    currentVerticalVelocity = JumpVelocity;
                    isGrounded = false;
                }
                break;
            default:
                break;
        }
    }


    public void PlatformJump(float velocity) {
            currentVerticalVelocity = JumpVelocity * velocity;
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
    public void TakeDamage(int damage)
    {

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