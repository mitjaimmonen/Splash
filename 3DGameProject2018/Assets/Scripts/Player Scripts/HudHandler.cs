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
        public Image healthIcon, throwableIcon, damageIndicator, hitmarker, crosshair;
        public Slider healthSlider;
        public Slider clipSlider;
        public Text clipAmmoText;
        public Text globalAmmoText;
        public Text timerText;
        public Sprite[] healthIcons, thorwableIcons;
        public Text playerNumberText;
        
    #endregion

    public float hitMarkerTime = 0.15f, damageIndicatorTime = 1f;
    public Color damageIndicatorColor;
    public PlayerController playerController;

    private int maxHealth, currentHealth;
    private int globalAmmo, clipSize, currentAmmo;
    private float healthUpdateTimer = 0, ammoUpdateTimer = 0, damageIndicatorTimer = 0, hitmarkerTimer = 0;
    private bool isHealthUpdating = false, isAmmoUpdating = false;
    private int oldCurrentHealth, oldCurrentAmmo;
    private Vector3 lastDamageOrigin, hitmarkerScale;
    private float fpsDeltaTime=0;
    private Color noAlpha;


    private void Start() {
        oldCurrentHealth = playerController.CurrentHealth;
        hitmarkerScale = crosshair.transform.localScale;
        oldCurrentAmmo = playerController.CurrentAmmo;
    }

    private void Update() 
    {
        fpsDeltaTime += (Time.unscaledDeltaTime - fpsDeltaTime) * 0.1f;
        int fps = (int)(1f/fpsDeltaTime);
        playerNumberText.text = "FPS: " + fps;

        if (timerText.color.a != 0)
        {
            noAlpha = timerText.color;
            noAlpha.a = 0;
            timerText.color = Color.Lerp(timerText.color, noAlpha, Time.deltaTime * 5f);
        }

        if (damageIndicator.color.a != 0)
        {
            noAlpha = damageIndicator.color;
            noAlpha.a = 0;
            UpdateDamageIndicator();
            damageIndicator.color = Color.Lerp(damageIndicator.color, noAlpha, Time.deltaTime * 5f);
        }
        
        if (hitmarkerTimer < hitMarkerTime)
        {
            UpdateHitmarker();
            hitmarkerTimer += Time.deltaTime;
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
        damageIndicator.color = damageIndicatorColor;
        UpdateDamageIndicator();
    }
    public void DealDamage()
    {
        hitmarkerTimer = 0;
        UpdateHitmarker();
    }
    private void UpdateHitmarker()
    {
        Vector3 newScale = hitmarkerScale + hitmarkerScale/2 * Mathf.Sin(Mathf.PI*hitmarkerTimer/hitMarkerTime);
        crosshair.transform.localScale = newScale;

        if (hitmarker.isActiveAndEnabled)
        {
            byte colorLerp = (byte)Mathf.Lerp(255,0, hitmarkerTimer/hitMarkerTime);
            hitmarker.color = new Color32(255,255,255,colorLerp);
        }

    }
    private void UpdateDamageIndicator() 
    {
        Vector3 dir = transform.InverseTransformDirection(lastDamageOrigin - transform.position);
        float angle = Mathf.Atan2(-dir.x, dir.z) * Mathf.Rad2Deg;
        damageIndicator.transform.localEulerAngles = new Vector3(0, 0, angle);
    }

    public void UpdateTimer(int time)
    {
        timerText.color = Color.white;
        timerText.text = time.ToString();
    }
    public void UpdateHealth(){

        isHealthUpdating = true;
        // if (currentHealth != playerController.CurrentHealth)
        //     oldCurrentHealth = currentHealth;
        currentHealth = playerController.CurrentHealth;
        maxHealth = playerController.MaxHealth;

        int lerpHealth = (int)Mathf.Lerp(oldCurrentHealth, currentHealth, healthUpdateTimer);

        int healthPercentage = (int)((float)lerpHealth/(float)maxHealth * 100f);
        int sliderHealth = (int)((float)healthPercentage * 0.6f + 21f);
        healthSlider.value = sliderHealth;

        if (healthUpdateTimer >= 1)
        {
            oldCurrentHealth = currentHealth;
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