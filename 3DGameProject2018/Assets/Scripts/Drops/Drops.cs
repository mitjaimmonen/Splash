using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PickupEnum {
    healthPickup,
    ammoPickup
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


    private void Awake()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        playerController = other.GetComponent<PlayerController>();
        Debug.Log("Collided with pickup. Name: " + pickupType);

        if (other.gameObject.CompareTag("Player"))
        {
            int tempHealth = playerController.CurrentHealth;
            int tempAmmo = playerController.GlobalAmmo;

            if (pickupType == PickupEnum.healthPickup)
            {
                playerController.CurrentHealth += pickupValue;

                if (tempAmmo < playerController.GlobalAmmo)
                {
                    //Play sound
                    Destroy(this.gameObject);
                }
                    
            } 
            else if (pickupType == PickupEnum.ammoPickup)
            {
                playerController.GlobalAmmo += pickupValue;

                if (tempAmmo < playerController.GlobalAmmo)
                {
                    //Play sound
                    Destroy(this.gameObject);
                }
            }
            
        }
    }
}
