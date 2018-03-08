using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/********************************************
* HudHandler
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
        public Text playerNumberText;
    #endregion

    public PlayerController playerController;

    private int maxHealth, currentHealth;
    private int globalAmmo, clipSize, currentAmmo;
    private float healthUpdateTimer = 0, ammoUpdateTimer = 0;
    private bool isHealthUpdating, isAmmoUpdating;
    private int oldMaxHealth, oldCurrentHealth, oldHealthPercentage;
    private int oldCurrentAmmo, oldClipSize;


    private void Start() {
        oldCurrentHealth = playerController.CurrentHealth;
        oldMaxHealth = playerController.maxHealth;
        oldHealthPercentage = 100;

        oldCurrentAmmo = playerController.CurrentAmmo;
        oldClipSize = playerController.ClipSize;
    }

    private void Update() 
    {
        if (isHealthUpdating || isAmmoUpdating)
        {
            if (isHealthUpdating)
            {
                healthUpdateTimer += Time.deltaTime * 3.5f;
                UpdateHealth();

            }
            if (isAmmoUpdating)
            {
                ammoUpdateTimer += Time.deltaTime * 3.5f;
                UpdateAmmo();
            }
        }
    }

    public void UpdateHealth(){

        isHealthUpdating = true;
        // if (currentHealth != playerController.CurrentHealth)
        //     oldCurrentHealth = currentHealth;
        currentHealth = playerController.CurrentHealth;
        maxHealth = playerController.maxHealth;

        int lerpHealth = (int)Mathf.Lerp(oldCurrentHealth, currentHealth, healthUpdateTimer);

        int healthPercentage = (int)((float)lerpHealth/(float)maxHealth * 100f);
        int sliderHealth = (int)((float)healthPercentage * 0.6f + 20f);
        healthSlider.value = sliderHealth;

        if (healthUpdateTimer >= 1)
        {
            oldCurrentHealth = currentHealth;
            oldMaxHealth = maxHealth;
            healthUpdateTimer = 0;
            isHealthUpdating = false;

        }


        if (healthPercentage > 66)
            healthIcon.sprite = healthIcons[0];
        else if  (healthPercentage > 33)
            healthIcon.sprite = healthIcons[1];
        else
            healthIcon.sprite = healthIcons[2];

    }

    public void UpdateAmmo(){

        isAmmoUpdating = true;
        globalAmmo = playerController.GlobalAmmo;
        clipSize = playerController.ClipSize;
        if (currentAmmo != playerController.CurrentAmmo)
            oldCurrentAmmo = currentAmmo;
        currentAmmo = playerController.CurrentAmmo;


        int lerpAmmo = (int)Mathf.SmoothStep(oldCurrentAmmo, currentAmmo, ammoUpdateTimer);

        clipAmmoText.text = "" + lerpAmmo;
        globalAmmoText.text = globalAmmo.ToString();
        
        int ammoPercentage = (int)((float)lerpAmmo / (float)clipSize *100f);
        int sliderAmmo = (int)(ammoPercentage * 0.6f + 20f);
        clipSlider.value = sliderAmmo;

        if (ammoUpdateTimer > 1)
        {
            oldCurrentAmmo = currentAmmo;
            oldClipSize = clipSize;
            ammoUpdateTimer = 0;
            isAmmoUpdating = false;
        }

    }

    public void UpdateThrowable(string name) {
        throwableIcon.enabled = true;
        if (name == "balloon")
            throwableIcon.sprite = thorwableIcons[0];
        else
            throwableIcon.enabled = false;

    }

}