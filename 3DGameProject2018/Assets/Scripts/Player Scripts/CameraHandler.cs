using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour {

    [HideInInspector]
    public GameObject target; //PlayerController sets this on instantiate
    [HideInInspector]    
    public PlayerController playerController; // Playercontroller refers itself to this on instantiate.

    private Camera currentCamera; 
    public float fovLerpTime = 0.2f;
    public float rotRadius = 0.1f;
    public Vector3 offsetFromHead = new Vector3(0, -0.2f, -0.1f);

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
        if (playerController.IsAlive)
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

            var pos = target.transform.position; //Player's head position
            Vector3 forw = playerController.transform.forward.normalized; //Player's forward (never looking up or down)
            Vector3 up = playerController.transform.up.normalized; //Player's up vector (should always be straight upwards)
            pos += offsetFromHead.z * forw + offsetFromHead.y * up; //Add offsets to camera position
            forw = target.transform.forward.normalized; //Change forward vector to head's forward, which can also be upwards/downwards
            pos += rotRadius * forw; //Add radius into offset

            transform.position = pos;
            transform.rotation = target.transform.rotation;
        }

    }

    public void Die(PlayerController attacker)
    {
        StartCoroutine(FollowAttacker(attacker));
    }

    private IEnumerator FollowAttacker(PlayerController attacker)
    {
        Vector3 posOffset;
        while (!playerController.IsAlive)
        {
            //First loop looks at dead player
            //Second loop lerps to attacker

            float time = Time.time + 2f;        
            while (time > Time.time || (!playerController.IsAlive && !attacker))
            {
                //If suicide (= !Attacker), look at dying player until respawned.
                posOffset = (playerController.transform.right * 0.5f) + (-playerController.transform.forward * 2f) + (playerController.transform.up *2f);            

                Vector3 lerpPos = Vector3.Lerp(transform.position, playerController.transform.position + posOffset, Time.deltaTime * 5f);
                Quaternion lerpRot = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerController.playerHead.transform.position - lerpPos, playerController.transform.up), Time.deltaTime * 5f);
                transform.position = lerpPos;
                transform.rotation = lerpRot;
                yield return new WaitForEndOfFrame();
            }

            while (!playerController.IsAlive && attacker)
            {
                posOffset =(attacker.transform.forward * 1.5f) + (attacker.transform.up * 0.8f) + (-attacker.transform.right);                
                //Look at attacker until respawn
                Vector3 lerpPos = Vector3.Lerp(transform.position, attacker.transform.position + posOffset, Time.deltaTime *8f);
                Quaternion lerpRot = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(attacker.playerHead.transform.position - lerpPos, attacker.transform.up),Time.deltaTime*8f);
                transform.position = lerpPos;
                transform.rotation = lerpRot;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }
        



        yield break;
    }


    //All playerControllers can set their viewports by calling this function
    //Player number starts from 1.
    public void SetViewport(int playerAmount, int player) 
    {
        //Each camera has one cullingMask which no other camera has.
        //Can be used to show player specific in-world elements.
        // currentCamera.cullingMask |= 1 << LayerMask.NameToLayer("Culling" + player);
        currentCamera.cullingMask &=  ~(1 << LayerMask.NameToLayer("Culling" + player));

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
        SetFov(oldFov, newFov);
    }
    private void SetFov(float oldFov, float newFov)
    {
        float currentFov = Mathf.Lerp(oldFov, newFov, fovTimer/fovLerpTime);
        currentCamera.fieldOfView = currentFov;
    }

}
