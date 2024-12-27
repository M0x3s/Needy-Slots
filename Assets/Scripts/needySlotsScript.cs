using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class needySlotsScript : MonoBehaviour {
	public KMAudio audio;
	public KMBombInfo bomb;
	public KMNeedyModule needy;
	public KMSelectable knob;
	public GameObject lever;
	public slotScript[] slots;
	public GameObject[] lights;
	private int slotState = 1; // Inital 0, Spinning 1, Waiting 2, X 3
	private int signType;
	private float flickerRate = 0.5f;
	private bool isActive = true;
	private bool giveMercy = true;
	// Logging
	static int moduleIdCounter = 1;
	int moduleId;
	void Awake () {
		Debug.Log(needy);
		moduleId = moduleIdCounter++;
		knob.OnInteract += delegate() {pullHandle(); return false; };
		needy.OnTimerExpired += OnTimerExpired;
		needy.OnNeedyActivation += OnNeedyActivation;
		needy.OnNeedyDeactivation += OnNeedyDeactivation;
	}
	void Update () {
		if (slotState <= 1) {
			foreach (GameObject setOfLights in lights) {
				setOfLights.SetActive(false);
			}
		}
	}

	protected void OnNeedyActivation () {
		 Debug.Log("[Needy Slots #needySlots] Hellow World");
		 slotState = 0;
		 pullHandle();
	 }
	 void OnNeedyDeactivation()
	{
		Debug.Log("deactivate called");
		slotState = 1;
	}
	void pullHandle() {
		if (slotState == 1) {
			return;
		}
		if (!isActive) {
			needy.OnStrike();
			isActive = true;
			giveMercy = true;
		}
		slotState = 1;
		StopAllCoroutines();
		PickSignType();
		StartCoroutine("spinSlots");
	}

	void PickSignType() {
		signType = UnityEngine.Random.Range(0,2);
	}
	IEnumerator spinSlots() {
		int total = 0;
		needy.SetNeedyTimeRemaining(3f);
		foreach (slotScript slot in slots) {
			total += slot.spinSlot();
			yield return new WaitForSeconds (0.05f);
		}
		if (total == 12) {
			isActive = false;
		}
		yield return new WaitForSeconds (1f);
		slotState = 2;
		StartCoroutine("lightupSign");
		Debug.Log(total);
		if (total == 3) {
			Debug.Log("Speedrun!");
			if (giveMercy == true) {
				Debug.Log("Mercy tho");
				needy.SetNeedyTimeRemaining(5f);
			}
			else {
				needy.SetNeedyTimeRemaining(2f);
			}
		}
		else {
			needy.SetNeedyTimeRemaining(30f);
		}
		Debug.Log(needy.GetNeedyTimeRemaining());
		giveMercy = false;
	}
	IEnumerator lightupSign() {
		if (slotState <= 1) {
			yield break;
		}
		bool currState = lights[0].activeSelf;
		currState = !currState;
		foreach (GameObject setOfLights in lights) {
			setOfLights.SetActive(currState);
			if (signType == 0) { //Flicker
				currState = !currState;
			}
		}
		yield return new WaitForSeconds (flickerRate);
		StartCoroutine("lightupSign");
	}
	protected void OnTimerExpired()
    {
		if (isActive & slotState != 1) {
			needy.HandleStrike();
			giveMercy = true;
			pullHandle();
		}
    }
}
