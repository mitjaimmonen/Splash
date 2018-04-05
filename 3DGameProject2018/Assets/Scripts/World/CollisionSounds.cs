using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************
 * PSCollisions
 * 
 * Script that can be attached on any map element gameObject with collider.
 * If collider gets hit by particles, script counts the hits and plays sound accordingly.
 * MaxCount defines how many collisions is allowed to be detected at once.
 * Handles water collision sounds as well.
 * Coroutine is used instead of Update to keep script faster even in case of dozens of instances in scene.
 */


public class CollisionSounds : MonoBehaviour {

	public int maxParticleCollisionCount = 10;
	private int oldCount = 0;
	private int count = 0;
	private Vector3 intersection;
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();


	[FMODUnity.EventRef] public string collisionSE, waterSplashSE, destroySE;
    private FMOD.Studio.EventInstance soundEI; 
    private FMOD.Studio.ParameterInstance FMOD_FlowToBurst, FMOD_Volume;
	private float volume, soundTimer, collisionCountTimer;


	public void PlayDestroy()
	{
		FMODUnity.RuntimeManager.PlayOneShot(destroySE, transform.position);
	}

	private void OnParticleCollision(GameObject other)
	{
		{
			//Resets count 10 times per second or when collision happens
			oldCount = 0;
			collisionCountTimer = Time.time;		
		}
		ParticlePhysicsExtensions.GetCollisionEvents (other.GetComponent<ParticleSystem>(), this.gameObject, collisionEvents);
		count = Mathf.Clamp(collisionEvents.Count, 0, maxParticleCollisionCount-oldCount);

		if (count > 0)
		{
			oldCount += count;
			intersection = collisionEvents[0].intersection;
			PlaySound();
		}
	}

	private void PlaySound()
	{

		if (soundEI.isValid())
		{
			FMOD_Volume.setValue(1f);
			FMOD_FlowToBurst.setValue(count / 10);
		}
		else
		{
			soundEI = FMODUnity.RuntimeManager.CreateInstance(collisionSE);
			soundEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(intersection));
			soundEI.getParameter("MasterVolume", out FMOD_Volume);
			soundEI.getParameter("FlowToBurst", out FMOD_FlowToBurst);
			FMOD_FlowToBurst.setValue(count / 10);
			FMOD_Volume.setValue(1f);
			soundEI.start();
			soundEI.release();
			StartCoroutine(FadeOut());
		}
	}

	private IEnumerator FadeOut()
	{
		while (soundEI.isValid())
		{
			FMOD_Volume.getValue(out volume);
			FMOD_Volume.setValue(Mathf.Lerp(volume, 0, 0.1f));
			if (volume < 0.1f)
			{
				soundEI.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				yield break;
			}
			yield return new WaitForSeconds(0.05f);
		}

		yield break;
	}

	public void WaterSplashSound(){

		if (soundTimer < Time.time - 1f)
		{
			Debug.Log("asd");
			FMODUnity.RuntimeManager.PlayOneShot(waterSplashSE, transform.position);
			soundTimer = Time.time;
		}
	}
}
