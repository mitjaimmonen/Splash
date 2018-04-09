using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

/********************************************
 * Weapon
 *
 * General behaviour for all weapons.
 * Controls weapon particle effects, sounds and animations as well.
 * 
 *
 * Other notes:
 *
 * Script assumes all weapons are under a parent which has player controller.
 *
 * isContinuous is used to change between real time shoot force and static.
 * Burst weapons shoot with full force always.
 * 
 */

[RequireComponent(typeof(Recoil))]
public class Weapon : MonoBehaviour {
    //hold a sound and particle effect along with damage, clip, ammunition size and isshooting, timer, iscontinuous
    public WeaponData weaponData;
    public GameObject weaponPickup;

    [Tooltip("Does weapon reload automatically when empty.")]
    public bool autoReload = true;

    [Tooltip("Is weapon burst or continuous (automatic) type.")]
    public bool isContinuous = true;

    [Tooltip("Can be used to center model aim.")]
    public float rotationOffsetY = -3f;

    [HideInInspector]
    public PlayerController playerController;

    private Recoil recoil;
    private Animator gunAnim;
    private Rigidbody rb;
    private Drops dropScript;
    private Collider dropCollider;
    private ParticleSystem waterParticles;
    private ParticleLauncher particleLauncher;
    private bool isShooting, isReloading = false, isActive;

    [Tooltip("What is the gun's local position under player's gunOrigin.")]
    public Vector3 localPositionOffset;

    //InputTimer checks if shoot trigger has been released
    //FirerateTimer checks the interval between shots
    //ShootTimer checks if weapon has been shooting long enough
    private float inputTimer, fireRateTimer, shootTimer;
    private float shootSpeed; //Particle start speed (=force)
    private Quaternion lookRotation;
    private Vector3 lookDirection;
    

    #region FMOD

        //Only shoot soundEvent is stored in event instance because its parameters need to be accessed during lifetime.
        //Other sound events are "one shots" which will be released after playing.
        [FMODUnity.EventRef] public string shootSE, reloadSE, emptyMagSE;        
        private FMOD.Studio.EventInstance shootEI; 

        //Store all fmod variables in code for easier tweaking during playback
        private FMOD.Studio.ParameterInstance FMOD_Clip; // amount of water left in magazine, 0-100
        private FMOD.Studio.ParameterInstance FMOD_Shooting; // Shooting force from Input, 0-1           

    #endregion

    #region getters & setters

        public int CurrentClipAmmo 
        {
            get { return weaponData.currentClipAmmo; }
            set
            {
                if (value < 0)
                    weaponData.currentClipAmmo = 0;
                else if (value <= weaponData.clipSize)
                    weaponData.currentClipAmmo = value;
                else if (value > weaponData.clipSize)
                    weaponData.currentClipAmmo = weaponData.clipSize;
                if (isActive)
                    playerController.CurrentAmmo = weaponData.currentClipAmmo;
            }
        }
        public int ClipSize
        {
            get { return weaponData.clipSize; }
        }

    #endregion

    //Gets called on picked up
    public void Initialize()
    {
        recoil = GetComponent<Recoil>();
        weaponData = GetComponent<WeaponData>();
        transform.parent = playerController.gunsParent.transform;
        transform.localPosition = localPositionOffset;
        
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();
        if (!playerController)
            Debug.LogWarning("No PlayerController found!");
        waterParticles = gameObject.GetComponentInChildren<ParticleSystem>();
        if (!waterParticles)
            Debug.LogWarning("No ParticleSystem found!");
        
        particleLauncher = waterParticles.GetComponent<ParticleLauncher>();
        if (!particleLauncher)
            Debug.LogWarning("No ParticleLauncher found!");
            
        gunAnim = gameObject.GetComponentInChildren<Animator>();
        if (!gunAnim)
            Debug.LogWarning("No Animator found!");
        
        shootSpeed = waterParticles.main.startSpeedMultiplier;
        // weaponData.currentClipAmmo = weaponData.clipSize;
        // playerController.ClipSize = clipSize;
        // playerController.CurrentAmmo = currentClipAmmo;
        // playerController.CurrentDamage = damage;
        // particleLauncher.HeadshotMultiplier = headshotMultiplier;

    }

    //When weapon is picked up or when switched as currentWeapon
    public void Activate()
    {
        if (playerController)
        {
            playerController.ClipSize = weaponData.clipSize;
            playerController.CurrentAmmo = weaponData.currentClipAmmo;
            playerController.CurrentDamage = weaponData.damage;
        }
        else
        {
            Debug.Log("No PlayerController on Activate. This should not happen.");
            
        }
        if (particleLauncher)
        {
            particleLauncher.HeadshotMultiplier = weaponData.headshotMultiplier;            
        }
        else
        {
            Debug.Log("No particle launcher on Activate. This should not happen.");
        }
        if (!weaponData)
            Debug.Log("No weaponData on Activate. This should not happen.");
            
            
        isActive = true;
        transform.localRotation = Quaternion.Euler(0,rotationOffsetY,0);
        gameObject.SetActive(true);

    }


    //When no more currentWeapon or when destroyed
    public void Deactivate()
    {
        shootEI.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        isActive = false;
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        shootEI.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);        
    }


    private void Update()
    {
        if(gameObject.activeSelf && !playerController.controller.IsPaused && isActive)
        {
            //Set weapon aim to center worldpoint of the viewport.

            if (playerController.IsAimRaycastHit && weaponData.centerAim)
            {
                lookDirection = (playerController.AimWorldPoint - waterParticles.transform.parent.position).normalized;
                lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
            else 
               lookRotation = Quaternion.LookRotation(playerController.playerHead.transform.forward);


            if (weaponData.lerpAim)
                waterParticles.transform.parent.rotation = Quaternion.Slerp(waterParticles.transform.parent.rotation, lookRotation, Time.deltaTime * weaponData.rotationSpeed);
            else
                waterParticles.transform.parent.rotation = lookRotation;
            
            //Updates sound position.
            shootEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            

            //Checks if shoot trigger is no longer called.
            if (inputTimer < Time.time - 0.1f || !isShooting)
            {
                isShooting = false;
                FMOD_Shooting.setValue(0);
            }
            gunAnim.SetBool("isShooting", isShooting);

            if (autoReload && weaponData.currentClipAmmo < weaponData.shotUsage)
            {
                Reload();

            }

            //Look up "fireRate", "maxShootTime" and "reloadTime" to see the meanings.
            fireRateTimer += Time.deltaTime;
            shootTimer += Time.deltaTime;
        }
    }


    public void Shoot(float input)
    {
        if (gameObject.activeSelf && !playerController.controller.IsPaused && isActive)
        {
            if (inputTimer < Time.time - 0.1f) //First time calling after input trigger
            {
                //Call everything here that needs to be called only once.

                shootTimer = 0;

                if (weaponData.currentClipAmmo < weaponData.shotUsage)
                {
                    FMODUnity.RuntimeManager.PlayOneShotAttached(emptyMagSE, gameObject);
                    Debug.Log("empty mag sound");
                }
            }
            //Reset input timer right after the if-check.
            inputTimer = Time.time;


            if (fireRateTimer < weaponData.fireRate)
                return;
            if (weaponData.currentClipAmmo < weaponData.shotUsage || (weaponData.maxShootTime != 0 && shootTimer > weaponData.maxShootTime))
            {
                isShooting = false;
                return;
            }

            //Tests passed and shooting is allowed, set bool true.
            isShooting = true;
            //Reset rest of the timers
            fireRateTimer = 0;
            //Reset animation bools making sure shooting animation gets played.
            gunAnim.SetBool("reload", false);

            //Set WaterParticle parameters.
            var main = waterParticles.main;
            float currentShootSpeed = shootSpeed;

            if(isContinuous)
            {
                currentShootSpeed = shootSpeed * input;
                //Stops water particles from just falling down due to too little force. (Usual max speed is around 30-60)
                if(currentShootSpeed < 7)
                    currentShootSpeed = 7;
            }
            else
                currentShootSpeed = shootSpeed;

            main.startSpeed = currentShootSpeed;
            waterParticles.Play();
            
            recoil.StartRecoil();

            //FMOD checks and parameters.
            //If-checks need to be made for "one-shot" sounds to work
            //One shots can start on top of each other while continuous continues the old sound without creating new.

            if (!shootEI.isValid() || !isContinuous)
            {
                shootEI = FMODUnity.RuntimeManager.CreateInstance(shootSE);
                shootEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            }

            //Set FMOD sound parameters.
            shootEI.getParameter("Magazine", out FMOD_Clip);
            shootEI.getParameter("Shooting", out FMOD_Shooting);
            FMOD_Shooting.setValue(input);
            float clipPercentage = (float)weaponData.currentClipAmmo / (float)weaponData.clipSize * 100f;        
            FMOD_Clip.setValue(clipPercentage);

            FMOD.Studio.PLAYBACK_STATE playbackState;
            shootEI.getPlaybackState(out playbackState);

            if (playbackState != FMOD.Studio.PLAYBACK_STATE.PLAYING || !isContinuous)
            {
                shootEI.start();
                shootEI.release();
            }


            weaponData.currentClipAmmo -= weaponData.shotUsage;
            playerController.CurrentAmmo = weaponData.currentClipAmmo;
        }
    }

    public void Reload() {
        
        // When animations are fully implemented, clip & ammo are counted at the end of reload.
        if (!isShooting && playerController.GlobalAmmo > 0 && !gunAnim.GetCurrentAnimatorStateInfo(0).IsName("reload") && weaponData.currentClipAmmo < weaponData.clipSize)
        {
            Debug.Log("Reload clip");
            gunAnim.SetBool("reload", true);
        }
    }




    #region In-animation calls
    public void AnimReloadStarted() {
        FMODUnity.RuntimeManager.PlayOneShotAttached(reloadSE, gameObject);
        gunAnim.SetBool("reload", false); //Animation will play till the end anyway if not interrupted.

    }
    public void AnimReloadEnded() {
        int oldClipAmmo = weaponData.currentClipAmmo;
        int currentGlobalAmmo = playerController.GlobalAmmo;
        if (currentGlobalAmmo < (weaponData.clipSize - oldClipAmmo))
            weaponData.currentClipAmmo += currentGlobalAmmo;
        else
            weaponData.currentClipAmmo = weaponData.clipSize;

        currentGlobalAmmo -= (weaponData.currentClipAmmo - oldClipAmmo);
        playerController.GlobalAmmo = currentGlobalAmmo;
        
        playerController.CurrentAmmo = weaponData.currentClipAmmo;
        
    }
    #endregion

}
