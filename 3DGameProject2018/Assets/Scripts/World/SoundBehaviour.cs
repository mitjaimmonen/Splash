using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBehaviour : MonoBehaviour {



	[FMODUnity.EventRef] public string collisionSE, waterSplashSE, destroySE;
    private FMOD.Studio.EventInstance soundEI; 
    private FMOD.Studio.ParameterInstance FMOD_FlowToBurst, FMOD_Volume;
	private float volume, soundTimer, collisionCountTimer;


	#region Sound Functionality
		public void PlayDestroy()
		{
			FMODUnity.RuntimeManager.PlayOneShot(destroySE, transform.position);
		}

		public void PlayCollisionSound(int countByMaxCount, Vector3 intersection)
		{

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

		public void WaterSplash(){

			if (soundTimer < Time.time - 0.25f)
			{
				FMODUnity.RuntimeManager.PlayOneShot(waterSplashSE, transform.position);
				soundTimer = Time.time;
			}
		}
	#endregion

}
