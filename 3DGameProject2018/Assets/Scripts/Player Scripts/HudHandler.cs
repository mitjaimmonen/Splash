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
        public Image hitDirectionIndicator;
    #endregion

    public PlayerController playerController;

    private int maxHealth, currentHealth;
    private int globalAmmo, clipSize, currentAmmo;
    private float healthUpdateTimer = 0, ammoUpdateTimer = 0, damageIndicatorTimer = 0;
    private bool isHealthUpdating, isAmmoUpdating;
    private int oldMaxHealth, oldCurrentHealth, oldHealthPercentage;
    private int oldCurrentAmmo, oldClipSize;
    private Vector3 lastDamageOrigin;


    private void Start() {
        oldCurrentHealth = playerController.CurrentHealth;
        oldMaxHealth = playerController.MaxHealth;
        oldHealthPercentage = 100;

        oldCurrentAmmo = playerController.CurrentAmmo;
        oldClipSize = playerController.ClipSize;
    }

    private void Update() 
    {
        if (damageIndicatorTimer <= 0.5f)
        {
            UpdateDamageIndicator();
            damageIndicatorTimer += Time.deltaTime;
        }


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

    public void TakeDamage(Vector3 origin) 
    {
        lastDamageOrigin = origin;
        damageIndicatorTimer = 0;
        UpdateDamageIndicator();
    }

    private void UpdateDamageIndicator() 
    {
        Vector3 dir = transform.InverseTransformDirection(lastDamageOrigin - transform.position);
        float angle = Mathf.Atan2(-dir.x, dir.z) * Mathf.Rad2Deg;
        hitDirectionIndicator.transform.localEulerAngles = new Vector3(0, 0, angle);

        byte colorLerp = (byte)Mathf.Lerp(255,0, damageIndicatorTimer/0.5f);
        hitDirectionIndicator.color = new Color32(255,0,0,colorLerp);

    }
    public void UpdateHealth(){

        isHealthUpdating = true;
        // if (currentHealth != playerController.CurrentHealth)
        //     oldCurrentHealth = currentHealth;
        currentHealth = playerController.CurrentHealth;
        maxHealth = playerController.MaxHealth;

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