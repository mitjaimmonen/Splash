using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

/********************************************
 * (class name) Weapon
 *
 * General behaviour for all weapons.
 * Controls weapon particle effects, sounds and animations as well.
 * 
 *
 * Other notes:
 *
 * Script assumes all weapons are under a parent which has player controller.
 *
 * Timer is set for fire rate now.
 * Can be changed if needed.
 *
 * isContinuous is used to change between real time shoot force and static.
 * Burst weapons shoot with full force always.
 * 
 *
 * Particles startSpeed working but particlesystem needs tweaking.
 */

public class Weapon : MonoBehaviour {
    //hold a sound and particle effect along with damage, clip, ammunition size and isshooting, timer, iscontinuous

    [Tooltip("How much water fits in a clip")]
    public int clipSize = 100;

    [Tooltip("How much water in a single magazine")]
    public int maxBullets = 1000;

    [Tooltip("How much one shot consumes water")]
    public int shotUsage = 1;

    [Tooltip("How long to wait until shoot input can get called again.")]
    public float fireRate = 0.25f;

    [Tooltip("This force-stops shooting after given time. Applies only to Pistol weaponType.")]
    public float maxShootTime = 0;  

    [Tooltip("How long it takes to reload this gun.")]
    public float reloadTime = 2f;

    [Tooltip("Does weapon reload automatically when empty?")]
    public bool autoReload = true, isContinuous = true;


    private PlayerController playerController;
    private Animator gunAnim;
    private ParticleSystem waterParticles;
    private bool isShooting, isScope, isReloading = false;
    private int currentClipAmmo, currentGlobalAmmo;
    private float fireRateTimer, shootTimer, reloadTimer;
    private float shootForceMultiplier; //Controls water force according to input axis amounts
    private float accuracyRandomizer; //How much particle direction is randomized
    private float shootSpeed; //Particle start speed (=force)
    

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
        // gunAnim = gameObject.GetComponent<Animator>();
        
        shootSpeed = waterParticles.main.startSpeedMultiplier;
        accuracyRandomizer = waterParticles.shape.randomDirectionAmount;

        currentClipAmmo = clipSize;
        playerController.CurrentAmmo = currentClipAmmo;
        playerController.ClipSize = clipSize;

    }

    void OnEnable() {
        playerController.currentWeapon = this;
        playerController.CurrentAmmo = currentClipAmmo;
        playerController.ClipSize = clipSize;
        // playerController.ShotUsage = shotUsage;
    }
    private void Update()
    {


        shootEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));

        if(Input.GetMouseButton(0))
            Shoot(0.9f);
        
        if (reloadTimer > reloadTime)
            isReloading = false;
        
        
        if (isShooting)
        {
            isShooting = false;            
        }
        else if (fireRateTimer > fireRate || isReloading || (maxShootTime != 0 && shootTimer > maxShootTime) || currentClipAmmo < shotUsage) 
        {
            FMOD_Shooting.setValue(0);
            shootTimer = 0;
                
        }
        fireRateTimer += Time.deltaTime;
        shootTimer += Time.deltaTime;
        reloadTimer += Time.deltaTime;
        
    }


    public void Shoot(float input)
    {
        // Shoot is called every frame when input is more than 0.19
        isShooting = true;        

        if (fireRateTimer < fireRate || isReloading || (maxShootTime != 0 && shootTimer > maxShootTime)) 
        {
            isShooting = false;
            return;
        }
        else
        {
            fireRateTimer = 0;
        }
        
        if (currentClipAmmo < shotUsage)
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached(emptyMagSE, gameObject);
            isShooting = false;

            if(autoReload)
                Reload();

            return;
        }


        //Set WaterParticle parameters.
        var main = waterParticles.main;
        var shape = waterParticles.shape;
        float currentShootSpeed = shootSpeed;
        float currentAccuracyRandomizer = accuracyRandomizer;

        if(isContinuous)
            currentShootSpeed = shootSpeed * input;

        if (!isScope)
            currentAccuracyRandomizer = 2 * accuracyRandomizer;

        main.startSpeed = currentShootSpeed;
        shape.randomDirectionAmount = currentAccuracyRandomizer;
        waterParticles.Play();
        
        

        //These checks need to be made for "one-shot" sounds to work
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

    private void Reload() {
        // gunAnim.SetTrigger("reload");
        // When animations are in, clip & ammo are counted at the end of reload.
        isReloading = true;        
        FMODUnity.RuntimeManager.PlayOneShotAttached(reloadSE, gameObject);
        int oldClipAmmo = currentClipAmmo;
        currentGlobalAmmo = playerController.GlobalAmmo;
        if (currentGlobalAmmo < (clipSize - oldClipAmmo))
            currentClipAmmo += currentGlobalAmmo;
        else
            currentClipAmmo = clipSize;

        currentGlobalAmmo -= (currentClipAmmo - oldClipAmmo);
        Debug.Log("Current clip ammo = " + currentClipAmmo + " old clip ammo = " + oldClipAmmo);
        playerController.GlobalAmmo = currentGlobalAmmo;
        
        playerController.CurrentAmmo = currentClipAmmo;

        reloadTimer = 0;
    }


    void OnDestroy() {
        waterParticles.Stop();
        shootEI.release();
        shootEI.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

}
