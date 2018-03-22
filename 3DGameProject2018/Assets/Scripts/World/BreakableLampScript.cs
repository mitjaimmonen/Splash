using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableLampScript : MonoBehaviour {

	public int breakChancePercentage;
	private Light lampLight;
	private void Start()
	{
		lampLight = GetComponent<Light>();
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
		float startValue = lampLight.intensity;
		while (timer < 1f)
		{
			timer += Time.deltaTime;
			float lerp = Mathf.Lerp(startValue, 0.0f, timer);
			float intensity = Random.Range(lerp, startValue);
			lampLight.intensity = intensity;
			yield return new WaitForEndOfFrame();
		}
		Destroy(gameObject);
	}
}
