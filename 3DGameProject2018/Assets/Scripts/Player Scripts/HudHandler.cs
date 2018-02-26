using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudHandler : MonoBehaviour {

    /*
    TO DO:
        - on instantiate, make sure player controller references itself to this class!

    
     */

    #region canvasReferences
        public Image healthIcon;
        public Slider healthSlider;
        public Slider clipSlider;
        public Text clipAmmoText;
        public Text globalAmmoText;
        public Sprite[] healthIcons;
    #endregion

    public PlayerController playerController;

    public float currentHealth; //from playerController
    private float globalAmmo, clipSize, currentAmmo; //from playercontroller

    


    public void Update() {
        // UpdateHealth();
        
    }

    public void UpdateHealth(){
        
        //TODO
        //Currenthealth private, get from playerController.

        //NOTES
        //Health slider is 180 degrees, while the sprite itself is only 108 degrees,
        //This is why sliderHealth = value * 0.6 + 20
        //slider can not be under 20 or over 80.


        // currentHealth = playerController.health;

        if (currentHealth > 66)
            healthIcon.sprite = healthIcons[0];

        else if  (currentHealth > 33)
            healthIcon.sprite = healthIcons[1];

        else
            healthIcon.sprite = healthIcons[2];

        int sliderHealth = (int)(currentHealth * 0.6 + 20);
        healthSlider.value = sliderHealth;

    }

    public void UpdateAmmo(){

        globalAmmo = playerController.globalAmmo;
        currentAmmo = playerController.currentAmmo;

        clipAmmoText.text = ""+currentAmmo;
        globalAmmoText.text = ""+globalAmmo;
        
        int sliderAmmo = (int)(currentAmmo * 0.6 + 20);
        clipSlider.value = sliderAmmo;

    }

    public void UpdateThrowable() {
        //Update if throwable available
    }

}