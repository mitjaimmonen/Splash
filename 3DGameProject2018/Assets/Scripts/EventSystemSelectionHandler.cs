using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
public class EventSystemSelectionHandler : MonoBehaviour {

	private EventSystem e;

	void Awake()
	{
		e = GetComponent<EventSystem>();
	}
	void Update () {
		if (e.currentSelectedGameObject == null)
		{
			e.SetSelectedGameObject(e.firstSelectedGameObject);
		}
	}
}
}