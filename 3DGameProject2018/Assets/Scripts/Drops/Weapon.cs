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
    public enum WeaponTypeEnum
    {
        [Tooltip("Shoots continuously as long as bullets left")]AutoRifle,
        [Tooltip("Shoots continuously for specified maximum time")]Pistol, 
        [Tooltip("Shoots once")]Shotgun,
        [Tooltip("Shoots projectile once")]Launcher 
    };
    public Weapon.WeaponTypeEnum weaponType = WeaponTypeEnum.AutoRifle;

    public PlayerController playerController;

    [Tooltip("How much water in a single magazine")]
    public int maxMagazineSize = 100;

    [Tooltip("How much water in a single magazine")]
    public int maxBullets = 1000;

    [Tooltip("Amount of water containers at start")]
    public int startMagazines = 1;

    [Tooltip("How much one shot consumes water")]
    public int shotUsage = 1;

    [Tooltip("How long to wait until shoot input can get called again")]
    public float fireRate = 0.25f;

    [Tooltip("This force stops shooting after given time. Set 0 if infinity/ignore.")]
    public float maxShootTime = 0;  

    [Tooltip("Does weapon reload automatically when empty?")]
    public bool autoReload = true, isContinuous = true;

    private Animator gunAnim;
    private ParticleSystem waterParticles;
    private bool isShooting, isScope;
    private int currentMagazineSize, currentMagazines;
    private float fireRateTimer, shootTimeTimer;
    private float shootForceMultiplier; //Controls water force according to input axis amounts
    private float accuracyRandomizer; //How much particle direction is randomized
    private float shootSpeed; //Particle start speed (=force)

    


    #region FMOD

    //Only shoot sound event is stored in event instance because its parameters need to be accessed during lifetime.
    //Other sound events are "one shots" which will be released after playing.
    [FMODUnity.EventRef] public string shootSE, reloadSE, emptyMagSE, takeScopeSE;        
    private FMOD.Studio.EventInstance shootEI; 

    //Store all fmod variables in code for easier tweaking during playback
    private FMOD.Studio.ParameterInstance FMOD_Magazine; // amount of water left in magazine, 0-100
    private FMOD.Studio.ParameterInstance FMOD_Shooting; // Shooting force from Input, 0-100            

    #endregion


    public void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        waterParticles = gameObject.GetComponentInChildren<ParticleSystem>();
        // gunAnim = gameObject.GetComponent<Animator>();
        
        shootSpeed = waterParticles.main.startSpeedMultiplier;
        accuracyRandomizer = waterParticles.shape.randomDirectionAmount;

        currentMagazineSize = maxMagazineSize;
        currentMagazines = startMagazines;

    }

    private void Update()
    {
        Debug.Log(weaponType);
        fireRateTimer -= Time.deltaTime;
        shootTimeTimer += Time.deltaTime;

        //Test inputs temporarily here
        //If input amount more than 20, call shoot() every frame.
        float axis = Input.GetAxis("Fire1");

        if (axis > 0.2f) 
        {
            Shoot(axis);

        }
        else 
        {
            FMOD_Shooting.setValue(axis * 100f);
            shootTimeTimer = 0;
        }

        


        
    }

    public void Shoot(float force)
    {

        if (fireRateTimer > 0) 
        {
            if (shootTimeTimer > maxShootTime)
                FMOD_Shooting.setValue(0);            
            return;

        }

        if (currentMagazineSize < shotUsage)
        {

            FMODUnity.RuntimeManager.PlayOneShotAttached(emptyMagSE, gameObject);

            if(autoReload && currentMagazines>0)
                Reload();

            //If no ammo or magazines, do not shoot.
            FMOD_Shooting.setValue(0);
            return;
        }



        //Set WaterParticle parameters.
        var main = waterParticles.main;
        var shape = waterParticles.shape;
        float currentShootSpeed = shootSpeed;
        float currentAccuracyRandomizer = accuracyRandomizer;

        if(isContinuous)
            currentShootSpeed = shootSpeed * force;

        if (!isScope)
            currentAccuracyRandomizer = 2 * accuracyRandomizer;

        main.startSpeed = currentShootSpeed;
        shape.randomDirectionAmount = currentAccuracyRandomizer;
        waterParticles.Play();
        
        
        //SPAGHETTI HELL INC
        //These checks need to be made for "one-shot" sounds to work
        //One shots can start on top of each other while continuous continues the old sound without creating new.

        if (!shootEI.isValid() || !isContinuous)
        {
            shootEI = FMODUnity.RuntimeManager.CreateInstance(shootSE);
            shootEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        }

        //Set FMOD sound parameters.
        shootEI.getParameter("Magazine", out FMOD_Magazine);
        shootEI.getParameter("Shooting", out FMOD_Shooting);
        FMOD_Shooting.setValue(force * 100f);
        float magPercentage = (float)currentMagazineSize / (float)maxMagazineSize * 100f;        
        FMOD_Magazine.setValue(magPercentage);

        FMOD.Studio.PLAYBACK_STATE playbackState;
        shootEI.getPlaybackState(out playbackState);

        if (playbackState != FMOD.Studio.PLAYBACK_STATE.PLAYING || !isContinuous)
        {
            shootEI.start();
            shootEI.release();
        }

        


        currentMagazineSize -= shotUsage;
        
        //Timer starts countdown for next possibility to shoot        
        fireRateTimer = fireRate;
        

    }

    private void Reload() {
        // gunAnim.SetTrigger("reload");

        FMODUnity.RuntimeManager.PlayOneShotAttached(reloadSE, gameObject);
        currentMagazines--;
        currentMagazineSize = maxMagazineSize;
        fireRateTimer = 5f;
    }

    void OnDestroy() {
        waterParticles.Stop();
        shootEI.release();
        shootEI.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

}
