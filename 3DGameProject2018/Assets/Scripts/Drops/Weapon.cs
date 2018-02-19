using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************
 * (class name) Weapon
 *
 * General behaviour for all weapons.
 * Controls weapon particle effects, sounds and animations as well.
 * 
 *
 * Other notes:
 * Timer is set for fire rate now, making isContinuous obsolete.
 * Can be changed if needed.
 */


public class Weapon : MonoBehaviour {
    //hold a sound and particle effect along with damage, clip, ammunition size and isshooting, timer, iscontinuous

    [Tooltip("How much water in a single magazine")]
    public int maxMagazineSize = 100;
    [Tooltip("Amount of water containers at start")]
    public int startMagazines = 1;
    [Tooltip("How much one shot consumes water")]
    public int shotUsage = 1;
    [Tooltip("How long to wait until shoot input can get called again")]
    public float fireRate = 0.25f;
    [Tooltip("Does weapon reload automatically when empty?")]
    public bool autoReload = true;

    private ParticleSystem waterParticles;
    private bool isShooting, isScope;
    private int currentMagazineSize, currentMagazines;
    private float timer;
    private float shootForceMultiplier; //Controls water force according to input axis amounts

    


    #region FMOD

        //Only shoot sound event is stored in event instance because its parameters need to be accessed during lifetime.
        //Other sound events are "one shots" which will be released after playing.
        [FMODUnity.EventRef] public string shootSE, reloadSE, emptyMagazineSE, takeScopeSE;        
        private FMOD.Studio.EventInstance shootEI; 

        //Store all fmod variables in code for easier tweaking during playback
    	private FMOD.Studio.ParameterInstance FMOD_Magazine; // amount of water left in magazine, 0-100
        private FMOD.Studio.ParameterInstance FMOD_Shooting; // Shooting force from Input, 0-100            

    #endregion


    public void Start()
    {        
        currentMagazineSize = maxMagazineSize;
        currentMagazines = startMagazines;

        shootEI = FMODUnity.RuntimeManager.CreateInstance(shootSE);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(shootEI, GetComponent<Transform>(), GetComponent<Rigidbody>());
    }

    private void Update()
    {
        //if shooting add delta to timer
        //if timer is over our max time stop shooting
        // if (!isShooting)
        //     timer = 0;

        if(timer > 0) 
        {
            timer -= Time.deltaTime;
        }

        //Test inputs temporarily here
        //If input amount more than 20, call shoot() every frame.
        
        
    }

    public void Shoot()
    {

        if (timer <= 0)
            return;

        if (currentMagazineSize < shotUsage)
        {
            //--Play empty magazine sound

            if(autoReload && currentMagazines>0)
                Reload();

            //If no ammo or magazines, do not shoot.
            return;
        }


        currentMagazineSize -= shotUsage;

        //--Give water particles all variables it might need (speed,accuracy etc)
        waterParticles.Play();
        //--start sound & give sound parameters


        //Timer starts countdown for next possibility to shoot        
        timer = fireRate;
        

    }

    private void Reload() {
        //--Call reload animation
        //--Call reload sound

        currentMagazines--;
        currentMagazineSize = maxMagazineSize;
    }

    void OnDestroy() {
        waterParticles.Stop();
        shootEI.release();
        
    }


}
