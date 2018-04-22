using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;


public enum PickupEnum {
    healthPickup,
    ammoPickup,
    gunPickup
}


public class Drops : MonoBehaviour {

    /*
        This class is attached to pickup object
        If pickup is on a platform, the platform will take care of spawning a new after a timer.
    
     */

	//have a time variable for when its empty 
    //have a spawn command to spawn random drop from drops prefab


    public int pickupValue;
    public PickupEnum pickupType = PickupEnum.healthPickup;
    private PlayerController playerController;
    public Weapon pickupWeapon;
    public Rigidbody rb;

    private WeaponData weaponData;
    private float triggerTimer = 0;

    [FMODUnity.EventRef] public string pickupSE;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        weaponData = GetComponent<WeaponData>();
        if (!weaponData && pickupWeapon)
        {
            weaponData = PersonalExtensions.CopyComponentValues(pickupWeapon.weaponData, this.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {        
        if (other.gameObject.CompareTag("Player") && triggerTimer < Time.time-0.1f)
        {
            triggerTimer = Time.time;
            playerController = other.GetComponent<PlayerController>();
            if (pickupType == PickupEnum.healthPickup)
            {
                int tempHealth = playerController.CurrentHealth;
                playerController.CurrentHealth += pickupValue;

                if (tempHealth != playerController.CurrentHealth)
                    Destroy(this.gameObject);
                
                    
            } 
            else if (pickupType == PickupEnum.ammoPickup)
            {
                int tempAmmo = playerController.GlobalAmmo;
                playerController.GlobalAmmo += pickupValue;

                if (tempAmmo != playerController.GlobalAmmo)
                    Destroy(this.gameObject);
                
            }
            else if (pickupType == PickupEnum.gunPickup && playerController.IsAlive)
            {
                if (!weaponData)
                {
                    weaponData = GetComponent<WeaponData>();
                    if (!weaponData)
                        return;
                }
                playerController.AllowPickup(this, true, weaponData);
                
            }
            
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (pickupType == PickupEnum.gunPickup)
            {
                playerController.AllowPickup(this, false, weaponData);
                
            }
        }
    }

    void OnDestroy()
    {
        if (pickupType == PickupEnum.gunPickup && playerController)
        {
            Debug.Log("voi vitun vittu");
            playerController.AllowPickup(this, false, null);
            
        }
        // Debug.Log("This might cause fmod error on application exit");
        FMODUnity.RuntimeManager.PlayOneShot(pickupSE, transform.position);
        
    }
}
