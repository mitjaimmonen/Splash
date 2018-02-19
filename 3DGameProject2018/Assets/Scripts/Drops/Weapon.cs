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

    private Animator gunAnim;
    private ParticleSystem waterParticles;
    private bool isShooting, isContinuous, isScope;
    private int currentMagazineSize, currentMagazines;
    private float timer;
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


    public void Start()
    {
        waterParticles = gameObject.GetComponentInChildren<ParticleSystem>();
        shootSpeed = waterParticles.main.startSpeedMultiplier;
        accuracyRandomizer = waterParticles.shape.randomDirectionAmount;

        Debug.Log(shootSpeed);
        currentMagazineSize = maxMagazineSize;
        currentMagazines = startMagazines;

        gunAnim = gameObject.GetComponent<Animator>();

        shootEI = FMODUnity.RuntimeManager.CreateInstance(shootSE);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(shootEI, GetComponent<Transform>(), GetComponent<Rigidbody>());
    }

    private void Update()
    {
        if(timer > 0) 
        {
            timer -= Time.deltaTime;
        }



        //Test inputs temporarily here
        //If input amount more than 20, call shoot() every frame.
        float axis = Input.GetAxis("Fire1");
        if (axis > 0.2f)
            Shoot(axis);
        
        
    }

    public void Shoot(float force)
    {

        if (timer > 0)
            return;

        if (currentMagazineSize < shotUsage)
        {

            FMODUnity.RuntimeManager.PlayOneShotAttached(emptyMagSE, gameObject);

            if(autoReload && currentMagazines>0)
                Reload();

            //If no ammo or magazines, do not shoot.
            return;
        }



        //Give water particles all variables they might need (speed,accuracy etc)
        var main = waterParticles.main;
        var shape = waterParticles.shape;
        float currentShootSpeed;

        if(isContinuous)
            currentShootSpeed = shootSpeed * force;
        else
            currentShootSpeed = shootSpeed;

        float currentAccuracyRandomizer = accuracyRandomizer;

        if (!isScope)
            currentAccuracyRandomizer = 2 * accuracyRandomizer;
        else
            currentAccuracyRandomizer = accuracyRandomizer;


        main.startSpeed = currentShootSpeed;
        shape.randomDirectionAmount = currentAccuracyRandomizer;



        
        waterParticles.Play();
        //--start sound & give sound parameters

        currentMagazineSize -= shotUsage;

        //Timer starts countdown for next possibility to shoot        
        timer = fireRate;
        

    }

    private void Reload() {
        //This asks for animator to change animation as soon as last animation lets to.
        //Reload animation will then call sounds and magazine changes from itself.
        //This way we avoid subtracting magazines too early or playing sounds in wrong places.
        //See: void AnimReloading()
        gunAnim.SetTrigger("reload");
    }

    void OnDestroy() {
        waterParticles.Stop();
        shootEI.release();
    }


    #region IN-ANIMATION CALLS

    public void AnimReloading() {
        FMODUnity.RuntimeManager.PlayOneShotAttached(reloadSE, gameObject);

        currentMagazines--;
        currentMagazineSize = maxMagazineSize;
    }

    public void AnimShooting() {

    }

    #endregion


}
