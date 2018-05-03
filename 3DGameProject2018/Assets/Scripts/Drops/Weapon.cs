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
    public Transform MuzzleTransform;
    public GameObject leftArmPoint, rightArmPoint;
    public GameObject arms;


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
    private float muzzleToWeaponMagnitude;
    private Quaternion lookRotation;
    private Vector3 lookDirection, particlesLocalOffsetY;
    

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

        public Recoil RecoilScript
        {
            get { return recoil; }
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

        if (!weaponData.isLauncher)
        {
            waterParticles = gameObject.GetComponentInChildren<ParticleSystem>();
            if (!waterParticles)
                Debug.LogWarning("No ParticleSystem found!");
            
            particleLauncher = waterParticles.GetComponent<ParticleLauncher>();
            if (!particleLauncher)
                Debug.LogWarning("No ParticleLauncher found!");
        }

            
        gunAnim = gameObject.GetComponentInChildren<Animator>();
        if (!gunAnim)
            Debug.LogWarning("No Animator found!");

        if (!leftArmPoint || !rightArmPoint)
                Debug.LogWarning("Arm points not assigned. Arms wont be moved.");
        if (MuzzleTransform == null)
        {
            foreach (Transform trans in GetComponentInChildren<Transform>(true))
            {
                if (trans.gameObject.name == "Muzzle")
                    MuzzleTransform = trans;
            }
        }

        if (arms)
            arms.layer = LayerMask.NameToLayer("Culling" + playerController.playerNumber);
        
        if (!weaponData.isLauncher)
        {
            waterParticles.transform.localPosition = -MuzzleTransform.localPosition; //Inverse the muzzle position to get origin position
            var shape = waterParticles.shape; //Access particles component
            shape.position = -waterParticles.transform.localPosition; //change position where particles get emitted to muzzle position.
            weaponData.shootSpeed = waterParticles.main.startSpeedMultiplier;
            particleLauncher.MaxLoopCount = weaponData.maxCollisionCount;
        }

        particlesLocalOffsetY = new Vector3(0,MuzzleTransform.localPosition.y,0);
        muzzleToWeaponMagnitude = (RecoilScript.WeaponBody.transform.position - MuzzleTransform.position).magnitude;

    }

    //When weapon is picked up or when switched as currentWeapon
    public void Activate()
    {
        if (playerController)
        {
            playerController.ClipSize = weaponData.clipSize;
            playerController.CurrentAmmo = weaponData.currentClipAmmo;
            playerController.CurrentDamage = weaponData.damage;
            playerController.rigController.SwitchArmPoints(leftArmPoint, rightArmPoint);
            // playerController.armRigController.SwitchArmPoints(leftArmPoint, rightArmPoint);
        }
        else
        {
            Debug.Log("No PlayerController on Activate. This should not happen.");
            
        }
        if (particleLauncher)
        {
            particleLauncher.HeadshotMultiplier = weaponData.headshotMultiplier;       
            int ammo = Mathf.CeilToInt(CurrentClipAmmo/weaponData.shotUsage);
            gunAnim.SetFloat("ammo", ammo+1);
        }
        else
        {
            Debug.Log("No particle launcher on Activate. This should not happen.");
        }
        if (!weaponData)
            Debug.Log("No weaponData on Activate. This should not happen.");
            
            
        gunAnim.Rebind();
        isActive = true;
        transform.localRotation = Quaternion.Euler(0,rotationOffsetY,0);
        gameObject.SetActive(true);



    }


    //When no more currentWeapon or when destroyed
    public void Deactivate()
    {
        gunAnim.Rebind();
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
        if(gameObject.activeSelf && isActive && !playerController.controller.IsPaused)
        {
            //Set weapon aim to center worldpoint of the viewport.

            if (playerController.IsAimRaycastHit && weaponData.centerAim)
            {
                lookDirection = (playerController.AimWorldPoint - MuzzleTransform.position).normalized;
                lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

                if (weaponData.lerpAim)
                    MuzzleTransform.rotation = Quaternion.Slerp(MuzzleTransform.rotation, lookRotation, Time.deltaTime * weaponData.rotationSpeed);
                else
                    MuzzleTransform.rotation = lookRotation;
            }
            else 
                MuzzleTransform.localEulerAngles = RecoilScript.WeaponBody.transform.localEulerAngles - transform.localEulerAngles;


            //keep the water particles Muzzle position at its place during recoils.
            Vector3 newPosition = transform.position + (muzzleToWeaponMagnitude * RecoilScript.WeaponBody.transform.forward);
            MuzzleTransform.position = newPosition;
            MuzzleTransform.localPosition += particlesLocalOffsetY;
            




            
            //Updates sound position.
            shootEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            

            //Checks if shoot trigger is no longer called.
            if (inputTimer < Time.time - 0.05f || !isShooting)
            {
                if (!weaponData.isLauncher)
                    waterParticles.Stop();

                isShooting = false;
                FMOD_Shooting.setValue(0);
            }

            gunAnim.SetBool("isShooting", isShooting);

            if (playerController.autoReload && weaponData.currentClipAmmo < weaponData.shotUsage)
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
                if (!weaponData.isLauncher)
                    waterParticles.Stop();
                isShooting = false;
                return;
            }

            //Tests passed and shooting is allowed, set bool true.
            isShooting = true;
            //Reset rest of the timers
            fireRateTimer = 0;
            //Reset animation bools making sure shooting animation gets played.
            gunAnim.SetBool("reload", false);

            if (!weaponData.isLauncher)
            {
                //Set WaterParticle parameters.
                var main = waterParticles.main;
                float currentShootSpeed = weaponData.shootSpeed;

                if(isContinuous)
                {
                    currentShootSpeed = weaponData.shootSpeed * input;
                    //Stops water particles from just falling down due to too little force. (Usual max speed is around 30-60)
                    if(currentShootSpeed < 5)
                        currentShootSpeed = 5;
                }
                else
                    currentShootSpeed = weaponData.shootSpeed;

                main.startSpeedMultiplier = currentShootSpeed;
                if (!waterParticles.isPlaying || weaponData.alwaysPlayWaterOnShoot)
                    waterParticles.Play();
            }
            else
            {
                Quaternion randRot = new Quaternion(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
                Balloon balloon = Instantiate(weaponData.launcherProjectile, MuzzleTransform.position, randRot).GetComponent<Balloon>();
                balloon.playerController = playerController;
                balloon.particleLauncherParent.MaxLoopCount = weaponData.maxCollisionCount;
                balloon.particleLauncherParent.HeadshotMultiplier = weaponData.headshotMultiplier;
                balloon.particleLauncherChild.MaxLoopCount = weaponData.maxCollisionCount;
                balloon.particleLauncherChild.HeadshotMultiplier = weaponData.headshotMultiplier;
                balloon.Instantiate();

                balloon.rb.AddTorque(new Vector3(Random.Range(-100,100), Random.Range(-100,100), Random.Range(-100,100)), ForceMode.Impulse);
                balloon.rb.AddForce(MuzzleTransform.forward * weaponData.shootSpeed, ForceMode.Impulse);
                
                int ammo = Mathf.CeilToInt(CurrentClipAmmo/weaponData.shotUsage);
                gunAnim.SetFloat("ammo", ammo);

                //Grenade launcher stuff
                //Instantiate balloon
                //Set weapondata such as damage & collision amounts
                //Give force to aimed direction
                
            }


            
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
        gunAnim.SetFloat("ammo", 4);
    }

    #endregion

}
