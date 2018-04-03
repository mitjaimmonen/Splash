using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeScript : MonoBehaviour {

	[FMODUnity.EventRef] public string collisionSE;
    private FMOD.Studio.EventInstance soundEI; 
    private FMOD.Studio.ParameterInstance FMOD_Parameter, FMOD_Volume;
	private float vol;
	private Vector3 intersection;
	private float soundTimer;
	private bool isPlaying = false;

	 void Awake()
	 {
		soundEI = FMODUnity.RuntimeManager.CreateInstance(collisionSE);
		soundEI.getParameter("MasterVolume", out FMOD_Volume);
		soundEI.getParameter("FlowToBurst", out FMOD_Parameter);

	 }
	void Update()
	{
		if (isPlaying)
		{
			FMOD_Volume.getValue(out vol);
			FMOD_Volume.setValue(Mathf.Lerp(vol, 0, Time.deltaTime * 2f));
			if (vol < 0.1f)
			{
				// soundEI.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				soundEI.release();
				isPlaying = false;
			}
		}
	}

	void ParticleHitInChild(Vector4 collisionData)
	{
		intersection = new Vector3(collisionData.x, collisionData.y, collisionData.z);

		FMOD_Volume.setValue(1f);

		if (!isPlaying)
		{
			soundEI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(intersection));
			FMOD_Parameter.setValue(collisionData.w / 10);
			isPlaying = true;
			soundEI.start();
		}
	}
}
