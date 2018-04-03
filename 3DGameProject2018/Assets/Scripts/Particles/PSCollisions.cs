using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************
 * PSCollisions
 * 
 * Script that can be attached on any gameObject with collider.
 * If collider gets hit by particles, script counts the hits and sends a message upwards.
 * MaxLoopCount defines how many collisions is allowed to be detected at once.
 *
 * Coroutine is used instead of Update to keep script faster even in case of dozens of instances in scene.
 */


public class PSCollisions : MonoBehaviour {

	public int maxCount = 10;
	private int oldCount = 0;
	private int count = 0;
	private float collisionCountTimer;
	private Vector3 intersection;
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();


	[FMODUnity.EventRef] public string collisionSE;
    private FMOD.Studio.EventInstance soundEI; 
    private FMOD.Studio.ParameterInstance FMOD_FadeToBurst, FMOD_Volume;
	private float vol, soundTimer;
	private bool isPlaying = false;


	 void Awake()
	 {
		soundEI = FMODUnity.RuntimeManager.CreateInstance(collisionSE);
		soundEI.getParameter("MasterVolume", out FMOD_Volume);
		soundEI.getParameter("FlowToBurst", out FMOD_FadeToBurst);
	 }

	private void OnParticleCollision(GameObject other)
	{
		if (collisionCountTimer <= Time.time - 0.1f)
		{
			//Resets loopcount 10 times per second or when collision happens
			oldCount = 0;
			collisionCountTimer = Time.time;		
		}
		ParticlePhysicsExtensions.GetCollisionEvents (other.GetComponent<ParticleSystem>(), this.gameObject, collisionEvents);
		count = Mathf.Clamp(collisionEvents.Count, 0, maxCount-oldCount);

		if (count > 0)
		{
			oldCount += count;
			intersection = collisionEvents[0].intersection;
			PlaySound();
		}
	}

	private void PlaySound()
	{
		FMOD_Volume.setValue(1f);


		if (!isPlaying)
		{
			soundEI = FMODUnity.RuntimeManager.CreateInstance(collisionSE);
			soundEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(intersection));
			FMOD_FadeToBurst.setValue(count / 10);
			soundEI.start();
			soundEI.release();
			isPlaying = true;
			StartCoroutine(FadeOut());
		}
	}

	private IEnumerator FadeOut()
	{
		while (isPlaying)
		{
			FMOD_Volume.getValue(out vol);
			FMOD_Volume.setValue(Mathf.Lerp(vol, 0, 0.03f));
			if (vol < 0.1f)
			{
				soundEI.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				Debug.Log("setting false");
				isPlaying = false;
				yield break;
			}
			yield return new WaitForSeconds(0.03f);
		}
	}
}
