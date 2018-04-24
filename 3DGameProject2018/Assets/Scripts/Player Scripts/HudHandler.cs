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

public enum GamepadButton
{
    None,
    A, Y, R1, R2, L1, L2, L3

}

public class HudHandler : MonoBehaviour {     

    #region canvasReferences
        public Image healthIcon, damageIndicator, hitmarker, crosshair, buttonIndicatorMiddle, buttonIndicatorLeft;
        public List<Sprite> buttonSprites;
        public Slider healthSlider;
        public Slider clipSlider;
        public Slider circleTimerSlider;
        public Image circleSliderImage;
        
        public Text deathCountText, killCountText;
        public Text instructionTextMiddle, instructionTextLeft;
        public Text clipAmmoText;
        public Text globalAmmoText;
        public Text timerText;
        public Sprite[] healthIcons, thorwableIcons;
        
    #endregion


    public float hitMarkerTime = 0.15f, damageIndicatorTime = 1f;
    public Color damageIndicatorColor;
    public PlayerController playerController;
    public HudWeaponsHandler hudWeaponsHandler;

    private int maxHealth, currentHealth;
    private int globalAmmo, clipSize, currentAmmo;
    private float healthUpdateTimer = 0, ammoUpdateTimer = 0, damageIndicatorTimer = 0, hitmarkerTimer = 1;
    private bool isHealthUpdating = false, isAmmoUpdating = false , instructionsFadeMiddle = true, instructionsFadeLeft = true;
    private int oldCurrentHealth, oldCurrentAmmo;
    private Vector3 lastDamageOrigin, hitmarkerScale;
    private float delta=0, sceneTime = 0;
    private float tipTimerLeftSide =0;
    private Color noAlpha;


    private bool hasJumped = false;
    private bool hasShot = false;
    private bool hasReloaded = false;
    private bool hasSprinted = false;
    private bool hasSwitchedWeapon = false;

    private void Start() {
        oldCurrentHealth = playerController.CurrentHealth;
        hitmarkerScale = crosshair.transform.localScale;
        oldCurrentAmmo = playerController.CurrentAmmo;
        killCountText.text = playerController.stats.kills.ToString();
        deathCountText.text = playerController.stats.deaths.ToString();

        buttonIndicatorMiddle.color = Color.clear;
        buttonIndicatorLeft.color = Color.clear;
        instructionTextMiddle.text = "";
        instructionTextLeft.text = "";
        circleTimerSlider.value = 0;
    }

    #region Updates
    private void Update() 
    {
        delta = Time.deltaTime;
        sceneTime = Time.timeSinceLevelLoad;
        UpdateColorAlphas();
        UpdateStats();

        if (playerController.helpfulTips)
            UpdateTips();

        if (hitmarkerTimer < hitMarkerTime)
        {
            hitmarkerTimer += delta;
            UpdateHitmarker();
        }

        if (isHealthUpdating)
        {
            healthUpdateTimer += delta * 3.5f;
            UpdateHealth();

        }
        if (isAmmoUpdating)
        {
            ammoUpdateTimer += delta * 3.5f;
            UpdateAmmo();
        }
        
    }

    void UpdateStats()
    {
        killCountText.text = playerController.stats.kills.ToString();
        deathCountText.text = playerController.stats.deaths.ToString();
    }
    void UpdateColorAlphas()
    {

        if (timerText.color.a != 0)
        {
            noAlpha = timerText.color;
            noAlpha.a = 0;
            timerText.color = Color.Lerp(timerText.color, noAlpha, delta * 5f);
        }

        if (damageIndicator.color.a != 0)
        {
            noAlpha = damageIndicator.color;
            noAlpha.a = 0;
            UpdateDamageIndicator();
            damageIndicator.color = Color.Lerp(damageIndicator.color, noAlpha, delta * 5f);
        }

        if (circleSliderImage.color.a != 0)
        {
            noAlpha = circleSliderImage.color;
            noAlpha.a = 0;
            circleSliderImage.color = Color.Lerp(circleSliderImage.color, noAlpha, delta * 5f);
        }
        if (instructionTextMiddle.color.a != 0 && instructionsFadeMiddle)
        {
            noAlpha = instructionTextMiddle.color;
            noAlpha.a = 0;
            instructionTextMiddle.color = Color.Lerp(instructionTextMiddle.color, noAlpha, delta * 5f);
            buttonIndicatorMiddle.color = Color.Lerp(buttonIndicatorMiddle.color, noAlpha, delta * 5f);
            
        }
        if (instructionTextLeft.color.a != 0 && instructionsFadeMiddle)
        {
            noAlpha = instructionTextLeft.color;
            noAlpha.a = 0;
            instructionTextLeft.color = Color.Lerp(instructionTextLeft.color, noAlpha, delta * 5f);
            buttonIndicatorLeft.color = Color.Lerp(buttonIndicatorLeft.color, noAlpha, delta * 5f);
            
        }

    }

    void UpdateTips()
    {
        if (instructionsFadeMiddle) //Middle has ONLY aggressive instructions (kinda blocks the view)
        {

            if (playerController.GlobalAmmo + playerController.CurrentAmmo < playerController.CurrentWeapon.weaponData.shotUsage)
            {
                instructionTextMiddle.color = Color.white;
                buttonIndicatorMiddle.color = Color.clear;
                instructionTextMiddle.text = "Not enough ammo";
                buttonIndicatorMiddle.sprite = buttonSprites[(int)GamepadButton.None];
                return;
            }

            if (playerController.CurrentAmmo < playerController.CurrentWeapon.weaponData.shotUsage && !playerController.autoReload)
            {
                instructionTextMiddle.color = Color.white;
                buttonIndicatorMiddle.color = Color.white;
                instructionTextMiddle.text = "Reload";
                buttonIndicatorMiddle.sprite = buttonSprites[(int)GamepadButton.L1];
                return;

            }
        }
        if (instructionsFadeLeft) //Left has less important tips and texts
        {
            if (!playerController.hasShot)
            {
                if (tipTimerLeftSide < sceneTime - 5f && sceneTime > 5f)
                {
                    instructionTextLeft.color = Color.white;
                    buttonIndicatorLeft.color = Color.white;
                    instructionTextLeft.text = "Shoot";
                    buttonIndicatorLeft.sprite = buttonSprites[(int)GamepadButton.R2];
                }
                return;
            }
            else if (playerController.hasShot && !hasShot)
            {
                hasShot = true;
                tipTimerLeftSide = sceneTime;
                return;
            }

            if (!playerController.hasSwitchedWeapon)
            {
                if (playerController.carriedWeapons.Count > 1)
                {
                    instructionTextLeft.color = Color.white;
                    buttonIndicatorLeft.color = Color.white;
                    instructionTextLeft.text = "Switch weapon";
                    buttonIndicatorLeft.sprite = buttonSprites[(int)GamepadButton.R1];
                }
                return;
            }
            else if (playerController.hasSwitchedWeapon && !hasSwitchedWeapon)
            {
                hasSwitchedWeapon = true;
                tipTimerLeftSide = sceneTime;
                return;
            }

            if (!playerController.hasJumped)
            {
                if (tipTimerLeftSide < sceneTime - 5f)
                {
                    instructionTextLeft.color = Color.white;
                    buttonIndicatorLeft.color = Color.white;
                    instructionTextLeft.text = "Jump";
                    buttonIndicatorLeft.sprite = buttonSprites[(int)GamepadButton.L2];
                }
                return;
            }
            else if (playerController.hasJumped && !hasJumped)
            {
                hasJumped = true;
                tipTimerLeftSide = sceneTime;
                return;
            }
            if (!playerController.hasSprinted)
            {
                if (tipTimerLeftSide < sceneTime - 5f)
                {
                    instructionTextLeft.color = Color.white;
                    buttonIndicatorLeft.color = Color.white;
                    instructionTextLeft.text = "Sprint";
                    buttonIndicatorLeft.sprite = buttonSprites[(int)GamepadButton.L3];
                }
                return;
            }
            else if (playerController.hasSprinted && !hasSprinted)
            {
                hasSprinted = true;
                tipTimerLeftSide = sceneTime;
                return;
            }
        }
    }

    #endregion

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


    public void UpdateMiddleInstructions(bool active,string textContent, GamepadButton btn)
    {   
        if (active)
        {
            instructionsFadeMiddle = false;
            instructionTextMiddle.color = Color.white;
            instructionTextMiddle.text = textContent;
            buttonIndicatorMiddle.sprite = buttonSprites[(int)btn];
            buttonIndicatorMiddle.color = Color.white;
        }
        else
        {
            instructionsFadeMiddle = true;
        }
    }
    
    public void UpdateCircleTimer(float time)
    {
        circleSliderImage.color = Color.white;
        circleTimerSlider.value = time;
    }

    private void UpdateHitmarker()
    {
        Vector3 newScale = hitmarkerScale + (hitmarkerScale/2) * Mathf.Sin(Mathf.PI*hitmarkerTimer/hitMarkerTime);
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

    public void UpdateTimerText(int time)
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


}