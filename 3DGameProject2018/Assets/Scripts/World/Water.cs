using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Interface*/
public interface IWater
{
	void WaterInteraction();
	ParticleSplash particleSplash
	{
		get;
		set;
	}
	CollisionSounds colsounds
	{
		get;
		set;
	}
	float splashSizeMultiplier
	{
		get;
		set;
	}
	//MAke water particles size function here
}

public class Water : MonoBehaviour {

	private float splashTime = 0;
	public CollisionSounds colsound;
	void OnTriggerEnter(Collider other)
	{
		IWater water = other.GetComponent<IWater>();

		if (water != null && splashTime < Time.time-1f)
		{
			splashTime = Time.time;
			water.WaterInteraction(); //General object specific stuff the class wants to do.
			// water.particleSplash.PlaySplash(other, water.splashSizeMultiplier);
			if(water.colsounds != null)
			{
				water.colsounds.WaterSplashSound();
			}

			//sound

			//If collided object has any class with this function, it will handle the event itself.
			// other.gameObject.BroadcastMessage("OnWaterTrigger", SendMessageOptions.DontRequireReceiver);
			

		}

	}

}



