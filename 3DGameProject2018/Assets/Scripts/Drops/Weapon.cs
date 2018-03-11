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

public class Weapon : MonoBehaviour {
    //hold a sound and particle effect along with damage, clip, ammunition size and isshooting, timer, iscontinuous

    [Tooltip("How much damage per shot.")]
    public int damage = 5;
    public float headshotMultiplier = 1.5f;

    [Tooltip("How much water fits in a clip")]
    public int clipSize = 100;

    [Tooltip("How much one shot consumes water")]
    public int shotUsage = 1;

    [Tooltip("How long to wait until shoot input gets called again. E.g. 0.1f means shooting 10 rounds per second.")]
    public float fireRate = 0.25f;

    [Tooltip("This force-stops shooting after given time. Use isContinuous = true with this.")]
    public float maxShootTime = 0;

    [Tooltip("How long it takes to reload this gun.")]
    public float reloadTime = 2f;

    [Tooltip("Does weapon reload automatically when empty.")]
    public bool autoReload = true;

    [Tooltip("Is weapon burst or continuous (automatic) type.")]
    public bool isContinuous = true;

    [Tooltip("How quickly gun rotates towards worldpoint that is in the middle of the camera viewport.")]
    public float rotationSpeed = 1f;


    private PlayerController playerController;
    private Animator gunAnim;
    private ParticleSystem waterParticles;
    private ParticleLauncher particleLauncher;
    private bool isShooting, isScope, isReloading = false;
    private int currentClipAmmo, currentGlobalAmmo;
    private float fireRateTimer, shootTimer, reloadTimer;
    private float accuracyRandomizer; //How much particle direction is randomized
    private float shootSpeed; //Particle start speed (=force)
    private Quaternion lookRotation;
    private Vector3 lookDirection;
    

    #region FMOD

        //Only shoot soundEvent is stored in event instance because its parameters need to be accessed during lifetime.
        //Other sound events are "one shots" which will be released after playing.
        [FMODUnity.EventRef] public string shootSE, reloadSE, emptyMagSE, takeScopeSE;        
        private FMOD.Studio.EventInstance shootEI; 

        //Store all fmod variables in code for easier tweaking during playback
        private FMOD.Studio.ParameterInstance FMOD_Clip; // amount of water left in magazine, 0-100
        private FMOD.Studio.ParameterInstance FMOD_Shooting; // Shooting force from Input, 0-1           

    #endregion


    public void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        waterParticles = gameObject.GetComponentInChildren<ParticleSystem>();
        particleLauncher = waterParticles.GetComponent<ParticleLauncher>();
        gunAnim = gameObject.GetComponentInChildren<Animator>();
        
        shootSpeed = waterParticles.main.startSpeedMultiplier;
        accuracyRandomizer = waterParticles.shape.randomDirectionAmount;

        currentClipAmmo = clipSize;
        playerController.CurrentAmmo = currentClipAmmo;
        playerController.ClipSize = clipSize;
        playerController.CurrentDamage = damage;
        particleLauncher.headshotMultiplier = headshotMultiplier;

    }

    void OnEnable() {
        playerController.currentWeapon = this;
        playerController.CurrentAmmo = currentClipAmmo;
        playerController.ClipSize = clipSize;
        playerController.CurrentDamage = damage;        
        // playerController.ShotUsage = shotUsage;
    }
    private void Update()
    {

        //Set weapon aim to center worldpoint of the viewport.
        if (playerController.IsAimRaycastHit)
            lookDirection = (playerController.AimWorldPoint - transform.position).normalized;
        else 
            lookDirection = playerController.playerFace.transform.forward;

        lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        
        


        

        //Updates sound position.
        shootEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));

        // Some checks in case shooting has ended
        // if (reloadTimer > reloadTime)
        //     isReloading = false;   
        if (autoReload && currentClipAmmo < shotUsage)
            Reload();

        if (fireRateTimer - 0.1f > fireRate && isShooting)
            isShooting = false;
        else if (fireRateTimer > fireRate || isReloading || (maxShootTime != 0 && shootTimer > maxShootTime) || currentClipAmmo < shotUsage) 
        {
            gunAnim.SetBool("isShooting", false);
            FMOD_Shooting.setValue(0);
            shootTimer = 0;
                
        }
        
        //Look up "fireRate", "maxShootTime" and "reloadTime" to see the meanings.
        fireRateTimer += Time.deltaTime;
        shootTimer += Time.deltaTime;
        // reloadTimer += Time.deltaTime;
        
    }


    public void Shoot(float input)
    {
        isShooting = true;        

        // Do not shoot if one of the conditions is met.
        if (fireRateTimer < fireRate)
        {
            return;
        }
        if (maxShootTime != 0 && shootTimer > maxShootTime)
        {
            isShooting = false;
            return;
        }
        else
        {
            fireRateTimer = 0;
        }
        
        //Do not shoot if not enough ammo.
        if (currentClipAmmo < shotUsage)
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached(emptyMagSE, gameObject);
            isShooting = false;
            return;
        }
        isReloading = false;        
        gunAnim.SetBool("isShooting", true);

        //Set WaterParticle parameters.
        var main = waterParticles.main;
        var shape = waterParticles.shape;
        float currentShootSpeed = shootSpeed;
        float currentAccuracyRandomizer = accuracyRandomizer;

        if(isContinuous)
        {
            currentShootSpeed = shootSpeed * input;
            //Stops water particles from just falling down due to too little force. (Usual max speed is around 30-60)
            if(currentShootSpeed < 7)
                currentShootSpeed = 7;
        }
        if (!isScope)
            currentAccuracyRandomizer = 2 * accuracyRandomizer;

        main.startSpeed = currentShootSpeed;
        shape.randomDirectionAmount = currentAccuracyRandomizer;
        waterParticles.Play();
        
        

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
        float clipPercentage = (float)currentClipAmmo / (float)clipSize * 100f;        
        FMOD_Clip.setValue(clipPercentage);

        FMOD.Studio.PLAYBACK_STATE playbackState;
        shootEI.getPlaybackState(out playbackState);

        if (playbackState != FMOD.Studio.PLAYBACK_STATE.PLAYING || !isContinuous)
        {
            shootEI.start();
            shootEI.release();
        }


        currentClipAmmo -= shotUsage;
        playerController.CurrentAmmo = currentClipAmmo;
        

    }

    public void Reload() {
        
        // When animations are fully implemented, clip & ammo are counted at the end of reload.
        Debug.Log("Trying to reload clip");
        if (!isShooting && playerController.GlobalAmmo > 0 && !gunAnim.GetCurrentAnimatorStateInfo(0).IsName("reload") && currentClipAmmo < clipSize)
        {
            gunAnim.SetBool("reload", true);
            isReloading = true;        
            reloadTimer = 0;
        }
    }


    void OnDestroy() {
        waterParticles.Stop();
        shootEI.release();
        shootEI.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }


    #region In-animation calls

    public void AnimReloadStarted() {
        FMODUnity.RuntimeManager.PlayOneShotAttached(reloadSE, gameObject);
        gunAnim.SetBool("reload", false); //Animation will play till the end anyway if not interrupted.

    }
    public void AnimReloadEnded() {
        int oldClipAmmo = currentClipAmmo;
        currentGlobalAmmo = playerController.GlobalAmmo;
        if (currentGlobalAmmo < (clipSize - oldClipAmmo))
            currentClipAmmo += currentGlobalAmmo;
        else
            currentClipAmmo = clipSize;

        currentGlobalAmmo -= (currentClipAmmo - oldClipAmmo);
        playerController.GlobalAmmo = currentGlobalAmmo;
        
        playerController.CurrentAmmo = currentClipAmmo;
        
    }

    #endregion

}
