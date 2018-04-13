using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Interface*/
public interface IWater
{
	void WaterInteraction();
	ParticleSplash ParticleSplash
	{
		get;
		set;
	}
	CollisionBehaviour ColBehaviour
	{
		get;
		set;
	}
	float SplashSizeMultiplier
	{
		get;
		set;
	}
	//MAke water particles size function here
}

public class Water : MonoBehaviour {

	private ParticleSplash psSplash;
	private float splashTime = 0;
	public CollisionBehaviour collisionBehaviour;

	private void Awake()
	{
		var temp = GameObject.Find("SplashParticles");
		if (temp)
			psSplash = temp.GetComponent<ParticleSplash>();
		
	}
	void OnTriggerEnter(Collider otherCol)
	{
		IWater water = otherCol.GetComponent<IWater>();

		if (water != null && splashTime < Time.time-0.1f)
		{
			splashTime = Time.time;
			water.WaterInteraction(); //General object specific stuff the class wants to do.

			
			if(water.ColBehaviour != null)
				water.ColBehaviour.soundBehaviour.WaterSplash();
			if (water.ParticleSplash == null && psSplash)
				water.ParticleSplash = psSplash;
			if (water.ParticleSplash != null)
				water.ParticleSplash.PlaySplash(otherCol, water.SplashSizeMultiplier);


			//sound

			//If collided object has any class with this function, it will handle the event itself.
			// other.gameObject.BroadcastMessage("OnWaterTrigger", SendMessageOptions.DontRequireReceiver);
			

		}

	}

}



