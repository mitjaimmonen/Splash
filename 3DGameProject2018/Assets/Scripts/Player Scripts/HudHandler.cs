using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/********************************************
* (class name) HudHandler
* 
* Updates each players hud.
* Updates data from player controller when called to.
*      Health, current ammo, global ammo, throwable item
* 
* NOTES
* Sliders are 180 degrees, while the sprites themselves are only 108 degrees,
*   This is why sliderValue = value * 0.6 + 20
*   slider can not be under 20 or over 80.
*
*/

public class HudHandler : MonoBehaviour {     

    #region canvasReferences
        public Image healthIcon, throwableIcon;
        public Slider healthSlider;
        public Slider clipSlider;
        public Text clipAmmoText;
        public Text globalAmmoText;
        public Sprite[] healthIcons, thorwableIcons;
    #endregion


    public void UpdateHealth(int maxHealth, int currentHealth){

        int healthPercentage = (int)((float)currentHealth/(float)maxHealth * 100f);
        int sliderHealth = (int)((float)healthPercentage * 0.6f + 20f);
        healthSlider.value = sliderHealth;

        if (healthPercentage > 66)
            healthIcon.sprite = healthIcons[0];
        else if  (healthPercentage > 33)
            healthIcon.sprite = healthIcons[1];
        else
            healthIcon.sprite = healthIcons[2];

    }

    public void UpdateAmmo(int globalAmmo, int clipSize, int currentAmmo){

        clipAmmoText.text = "" + currentAmmo;
        globalAmmoText.text = "" + globalAmmo;
        
        int sliderAmmo = (int)(((float)currentAmmo / (float)clipSize *100f) * 0.6f + 20f);
        clipSlider.value = sliderAmmo;

    }

    public void UpdateThrowable(string name) {
        throwableIcon.enabled = true;
        if (name == "balloon")
            throwableIcon.sprite = thorwableIcons[0];
        else
            throwableIcon.enabled = false;

    }

}