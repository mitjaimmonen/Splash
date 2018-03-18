using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour {

    public GameObject target; //PlayerController sets this on instantiate.
    private Camera currentCamera; 
    public PlayerController playerController; // Playercontroller refers itself to this on instantiate.
    public float fovLerpTime = 0.2f;
    public float forwardOffset = 0.1f, rotRadius = 0.1f;

    private float fov, newFov, oldFov;
    private float fovTimer = 1f;


    private void Awake() {
        currentCamera = GetComponent<Camera>();
    }

    //Late update looks much more smooth because it lets all other transforms to finish first.    

    private void Update()
    {
        if (fovTimer <= fovLerpTime)
        {
            fovTimer += Time.deltaTime;
            SetFov(oldFov, newFov);
        }
    }
    private void LateUpdate()
    {
        RaycastHit hit;
        Ray forwardRay = new Ray(transform.position + transform.forward, transform.forward);

        if (Physics.Raycast(forwardRay, out hit, Mathf.Infinity))
        {
            playerController.AimWorldPoint = hit.point;
            playerController.IsAimRaycastHit = true;
        } else 
        {
            playerController.IsAimRaycastHit = false;
        }

        var pos = target.transform.position;
        Vector3 forw = playerController.transform.forward.normalized;
        pos += forwardOffset * forw;
        forw = target.transform.forward.normalized;
        pos += rotRadius * forw;

        transform.position = pos;
        transform.rotation = target.transform.rotation;
    }


    //All playerControllers can set their viewports by calling this function
    //Player number starts from 1.
    public void SetViewport(int playerAmount, int player) 
    {
        //Each camera has one cullingMask which no other camera has.
        //Can be used to show player specific in-world elements.
        currentCamera.cullingMask |= 1 << LayerMask.NameToLayer("Culling" + player);

        var rect = currentCamera.rect;
        fov = currentCamera.fieldOfView;
        
        switch(playerAmount)
        {
            case 1: {
                fov = 65f;
                rect.height = 1f;
                rect.width = 1f;
                rect.x = 0;
                rect.y = 0;
                currentCamera.rect = rect;
                currentCamera.fieldOfView = fov;
                break;

            }
            case 2: 
            {      
                fov = 80f; //Half screen looks better with higher fov.
                rect.height = 1f;
                rect.width = 0.5f;       

                if(player == 0) {
                    //viewport half screen left
                    rect.x = 0;
                    rect.y = 0;
                }
                else if (player == 1) {
                    //viewport half screen right
                    rect.x = 0.5f;
                    rect.y = 0;
                } else {
                    Debug.LogError("If two active players, there must be player 0 and player 1 defined.");
                }
                
                currentCamera.rect = rect;
                currentCamera.fieldOfView = fov;

                break;
            }
            case 3: 
            {
                fov = 60f;
                rect.height = 0.5f;
                rect.width = 0.5f;

                if(player == 0) {
                    //Set viewport 1/4 upper left

                    rect.x = 0;
                    rect.y = 0.5f;
                }
                else if (player == 1) {
                    //Set viewport 1/4 upper right
                    rect.x = 0.5f;
                    rect.y = 0.5f;
                }
                else if (player == 2) {
                    //Set viewport 1/4 lower left
                    rect.x = 0;
                    rect.y = 0;
                }
                else {
                    Debug.LogError("If three active players, there must be players 0-3 defined.");                    
                }

                currentCamera.rect = rect;
                currentCamera.fieldOfView = fov;
                
                break;

            }
            case 4: 
            {
                fov = 60f;
                rect.height = 0.5f;
                rect.width = 0.5f;

                if(player == 0) {
                    //Set viewport 1/4 upper left
                    rect.x = 0;
                    rect.y = 0.5f;
                }
                else if (player == 1) {
                    //Set viewport 1/4 upper right
                    rect.x = 0.5f;
                    rect.y = 0.5f;
                }
                else if (player == 2) {
                    //Set viewport 1/4 lower left
                    rect.x = 0;
                    rect.y = 0;
                }
                else if (player == 3) {
                    //Set viewport 1/4 lower right
                    rect.x = 0.5f;
                    rect.y = 0;
                }
                else {
                    Debug.LogError("If four active players, there must be players 0-3 defined");          
                }

                currentCamera.rect = rect;
                currentCamera.fieldOfView = fov;
                
                break;
            }
        }
    }

    public void NewFov(float multiplier)
    {
        newFov = fov*multiplier;
        oldFov = currentCamera.fieldOfView;
        fovTimer = 0;
        Debug.Log("Calling again");        
        SetFov(oldFov, newFov);
    }
    private void SetFov(float oldFov, float newFov)
    {
        float currentFov = Mathf.Lerp(oldFov, newFov, fovTimer/fovLerpTime);
        currentCamera.fieldOfView = currentFov;
    }

}
