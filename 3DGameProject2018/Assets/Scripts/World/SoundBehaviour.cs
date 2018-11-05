using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBehaviour : MonoBehaviour {



	[FMODUnity.EventRef] public string collisionSE, waterSplashSE, destroySE, hitmarkerSE, hitmarkerHeadshotSE;
    private FMOD.Studio.EventInstance soundEI; 
    private FMOD.Studio.ParameterInstance FMOD_FlowToBurst, FMOD_Volume;
	private float volume, soundTimer, collisionCountTimer;


	#region Sound Functionality
		public void PlayDestroy(Vector3 position)
		{
			// Debug.Log("PlaySound: " + destroySE);
			FMODUnity.RuntimeManager.PlayOneShot(destroySE, position);
		}

		public void PlayHitmarker(PlayerController attacker, bool isHead)
		{
			if (isHead)
				FMODUnity.RuntimeManager.PlayOneShotAttached(hitmarkerHeadshotSE, attacker.gameObject);
			else
				FMODUnity.RuntimeManager.PlayOneShotAttached(hitmarkerSE, attacker.gameObject);
		}

		public void PlayCollisionSound(float countByMaxCount, Vector3 intersection)
		{
			//Collision sound must have volume and flowToBurst parameters defined, otherwise it wont play at all.
			// Debug.Log("PlaySound: " + collisionSE);

			if (soundEI.isValid())
			{
				FMOD_Volume.setValue(1f);
				FMOD_FlowToBurst.setValue(countByMaxCount);
			}
			else
			{
				soundEI = FMODUnity.RuntimeManager.CreateInstance(collisionSE);
				soundEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(intersection));
				soundEI.getParameter("MasterVolume", out FMOD_Volume);
				soundEI.getParameter("FlowToBurst", out FMOD_FlowToBurst);
				FMOD_FlowToBurst.setValue(countByMaxCount);
				FMOD_Volume.setValue(1f);
				soundEI.start();
				soundEI.release();
				StartCoroutine(FadeOut());
			}
		}

		public void PlayTakeDamage()
		{
			// Debug.Log("PlaySound: " + collisionSE);
			//Oneshot. For example dealDamage sound effect.
			FMODUnity.RuntimeManager.PlayOneShot(collisionSE, transform.position);
		}

		public void WaterSplash(){

			if (soundTimer < Time.time - 0.25f)
			{
				Debug.Log("PlaySound: " + waterSplashSE);
				FMODUnity.RuntimeManager.PlayOneShot(waterSplashSE, transform.position);
				soundTimer = Time.time;
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

	#endregion

}
