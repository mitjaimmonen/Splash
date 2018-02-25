using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudHandler : MonoBehaviour {

    /*
    Unclear??
    - What class has health?
    - Is hud attached to player?
    - All weapons have own script, should hud get a list and check in update which is active?
    - What script takes care of possible throwable items?
    - How to get current player's camera?
    
     */

    // Variables:
    // Camera viewport size
    // ext player controller & health
    // ext weapon
    // child Health bar
    // child Health face icon
    // child current ammo text
    // child global ammo text
    // (Maybe) child magazine size OR shot usage text
    // (maybe) child "throwable" icon

    public void Awake() {
        //Get components

        //If viewport scaling does not work well automatically:
        //SetHudScaling(viewportSize)
        
    }

    public void Update() {

        //If changes in health:
        Health();
        //If changes in ammunition:
        Ammo();
        //If changes in throwable items:
        Throwable();
        
    }

    private void Health(){
        //Update health bar
        //Update health face icon
    }

    private void Ammo(){
        //Update global ammo
        //Update current weapon ammo
        //Update ammo bar

    }

    private void Throwable() {
        //Update if throwable available
    }

}
