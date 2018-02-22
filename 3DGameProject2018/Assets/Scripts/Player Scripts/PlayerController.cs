﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Weapon currentWeapon;
    public HudHandler hud;
    private int deaths = 0, kills = 0, damageTake = 0, damageDelt = 0;
    private MatchController controller;
    public int maxHealth = 100, currentDamage = 0;

    public float speed = 2;
    public float runMultiplier = 1.5f;
    //possibly a list of who damaged you as well so we could give people assists and stuff
    //wed have to run off a points system that way so id rather keep it to k/d right now or time because they are both easy

    private List<Effects> currentEffects;

    private void Start()
    {
        //Find controller and bind it
    }

    //apply Gravity
    //do a sphere check for drops
    //visually apply all current effects
    private void Update()
    {

    }

    //shoots, moves, interacts, shows scores, pauses if pressed button
    public void InputHandle(string[] input)
    {
        if(input[1] == "RightStick_X")
        {
            Move("x", float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
        }
        if(input[1] == "RightStick_y")
        {
            Move("y", float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat));
        }
        if(input[1] == "run")
        {
            Move("run", 1);
        }
        //if shoot 
        //shoots currentweapon
        //if show score 
        //hudhandler.showscore
    }



    //apply nongravity movements
    private void Move(string axis, float magnitude)
    {
        switch(axis)
        {
            case "x":
                transform.position += new Vector3(magnitude * speed * Time.deltaTime, 0, 0);
                break;
            case "y":
                transform.position += new Vector3(0, 0, magnitude * speed * Time.deltaTime);
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