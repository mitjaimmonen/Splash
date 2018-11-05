using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBehaviour : MonoBehaviour {

	public GameObject[] objectsToEnableOnAwake;
	private StateHandler state;
	void Start () {
		foreach (var obj in objectsToEnableOnAwake)
		{
			obj.SetActive(true);
		}
		state = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
		if (state != null)
		{
			if (state.options.timeOfDay == TimeOfDay.Day)
				SetDay();
			if (state.options.timeOfDay == TimeOfDay.Night)
				SetNight();
		}

	}


	void SetDay()
	{

	}

	void SetNight()
	{

	}
}
