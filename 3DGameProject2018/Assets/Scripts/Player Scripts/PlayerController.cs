﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Weapon currentWeapon;
    private int currentHealth, maxHealth = 100;
    private int clipSize, currentAmmo, globalAmmo, maxGlobalAmmo = 150;
    public HudHandler hud;
    private int deaths = 0, kills = 0, damageTake = 0, damageDelt = 0;
    private MatchController controller;
    public int currentDamage = 0;

    public Camera camera;
    //Movement Variables
    public float lookSpeed = 1;
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
        hud = Instantiate(hud, Vector3.zero, Quaternion.Euler(0,0,0));
        // hud.playerController = this;

        //Find controller and bind it
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
        if(input[1] == "RightHorizontal")
        {transform.RotateAround(Vector3.zero, Vector3.up, 20 * Time.deltaTime);
            transform.Rotate(new Vector3(0,1,0) * Time.deltaTime * float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat)*lookSpeed);
            //camera.transform.RotateAround(camera.transform.position, Vector3.up, Time.deltaTime * float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat) * lookSpeed);
        }
        if(input[1] == "RightVertical")
        {
            //look up and down
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