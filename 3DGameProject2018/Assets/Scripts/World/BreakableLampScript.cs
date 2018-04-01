using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableLampScript : MonoBehaviour {

	public int breakChancePercentage;
	private List<Light> lampLights;
	private List<float> startValues;
	
	private void Start()
	{
		lampLights = new List<Light>();
		startValues = new List<float>();
		foreach (var lamp in GetComponentsInChildren<Light>())
		{
			Debug.Log("adding light to list");
			lampLights.Add(lamp);
			startValues.Add(lamp.intensity);
		}
	}

	public void TakeDamage()
	{
		float rnd = Random.Range(1,100);
		if (rnd < breakChancePercentage)
		{
			Debug.Log("Luck");
			
			StartCoroutine(BreakLamp());
		}
		else
		{
			Debug.Log("Unluck");
		}
	}

	private IEnumerator BreakLamp()
	{
		float timer = 0;
		while (timer < 1f)
		{
			timer += Time.deltaTime;
			float lerpMultiplier = Mathf.Lerp(1f, 0, timer);
			for(int i = 0; i < lampLights.Count; i++)
			{
				float intensity = Random.Range(0, startValues[i] * lerpMultiplier);
				lampLights[i].intensity = intensity;

			}
			yield return new WaitForEndOfFrame();
		}
		Destroy(gameObject);
	}
}
