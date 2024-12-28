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
	private bool nonPlayer = true;
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

	protected void OnNeedyActivation () {
		 Debug.Log("[Needy Slots #needySlots] Hellow World");
		 isActive = true;
		 slotState = 0;
		 PickSignType();
		 pullHandle();
	 }
	 void OnNeedyDeactivation()
	{
		Debug.Log("deactivate called");
		slotState = 1;
	}
	protected void OnTimerExpired()
    {
		if (isActive & slotState != 1) {
			needy.HandleStrike();
		}
		nonPlayer = true;
		slotState = 1;
    }

	void pullHandle() {
		if (slotState == 1) {
			return;
		}
		if (!nonPlayer) {
			knob.AddInteractionPunch();
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		}
		if (!isActive) {
			needy.OnStrike();
			isActive = true;
			nonPlayer = true;
		}
		slotState = 1;
		StartCoroutine("spinSlots");
		StartCoroutine("pullDownAnimation");
	}
	void checkSet(HashSet<int> result) {
		if (result.SetEquals(new HashSet<int>{0, 0, 0})) {
			Debug.Log("Speedrun!");
			if (nonPlayer == true) {
				Debug.Log("Mercy tho");
				needy.SetNeedyTimeRemaining(3f);
				
			}
			else {
				needy.SetNeedyTimeRemaining(1.5f);
			}
			return;
		}
		needy.SetNeedyTimeRemaining(30f);

		List<HashSet<int>> options; // X = 0, B = 1, L = 2, T = 3
		if (signType == 0) { //Alternating
			switch (bomb.GetStrikes()) {
				case 0: options = new List<HashSet<int>>
				{
					new HashSet<int> {2, 2, 2},
					new HashSet<int> {2, 1, 3},
					new HashSet<int> {1, 1, 3}
				};
				break;
				case 1: options = new List<HashSet<int>>
				{
					new HashSet<int> {2, 2, 3},
					new HashSet<int> {2, 1, 0},
					new HashSet<int> {1, 3, 3}
				};
				break;
				default: options = new List<HashSet<int>>
				{
					new HashSet<int> {2, 2, 2},
					new HashSet<int> {3, 3, 0},
					new HashSet<int> {1, 3, 0}
				};
				break;
			}
		}
		else { //Solid
			switch (bomb.GetStrikes()) {
				case 0: options = new List<HashSet<int>>
				{
					new HashSet<int> {2, 0, 0},
					new HashSet<int> {2, 1, 3},
					new HashSet<int> {3, 3, 3}
				};
				break;
				case 1: options = new List<HashSet<int>>
				{
					new HashSet<int> {2, 2, 2},
					new HashSet<int> {2, 3, 0},
					new HashSet<int> {1, 1, 0}
				};
				break;
				default: options = new List<HashSet<int>>
				{
					new HashSet<int> {1, 3, 0},
					new HashSet<int> {1, 1, 0},
					new HashSet<int> {1, 1, 1}
				};
				break;
			}
		}
		Debug.Log("Light Type");
		Debug.Log(signType);
		Debug.Log("Result");
		foreach (int j in result) {
			Debug.Log(j);
		}
		foreach (HashSet<int> i in options) {
			Debug.Log("Option");
			foreach (int j in i) {
				Debug.Log(j);
			}
			if (result.SetEquals(i)) {
				Debug.Log("Woo!");
				isActive = false;
				return;
			}
		}
	}
	IEnumerator pullDownAnimation() {
		StopCoroutine("returnAnimation");
		yield return quickRevertAnimation();
		 float animationDuration = 0.5f;
		 var initalRotation = lever.transform.localRotation;
		 var targetRotation = initalRotation;
		 targetRotation.eulerAngles = new Vector3(-60, targetRotation.eulerAngles.y, targetRotation.z);
		 for (var t = 0f; t < 1; t += Time.deltaTime / animationDuration)
        {
            lever.transform.localRotation = Quaternion.Lerp(initalRotation, targetRotation, t);
            yield return null;
        }
	}
	IEnumerator quickRevertAnimation() {
		float animationDuration = 0.2f;
		 var initalRotation = lever.transform.localRotation;
		 var targetRotation = initalRotation;
		 targetRotation.eulerAngles = new Vector3(60, targetRotation.eulerAngles.y, targetRotation.z);
		 for (var t = 0f; t < 1; t += Time.deltaTime / animationDuration)
        {
            lever.transform.localRotation = Quaternion.Lerp(initalRotation, targetRotation, t);
            yield return null;
        }
		yield return new WaitForSeconds (0.1f);
	}
	IEnumerator returnAnimation() {
		StopCoroutine("pullDownAnimation");
		 float animationDuration = needy.GetNeedyTimeRemaining();
		 if (animationDuration < 1f) {
			animationDuration = 30f;
		 }
		 var initalRotation = lever.transform.localRotation;
		 var targetRotation = initalRotation;
		 targetRotation.eulerAngles = new Vector3(60, targetRotation.eulerAngles.y, targetRotation.z);
		 for (var t = 0f; t < 1; t += Time.deltaTime / animationDuration)
        {
            lever.transform.localRotation = Quaternion.Lerp(initalRotation, targetRotation, t);
            yield return null;
        }
	}
	IEnumerator spinSlots() {
		HashSet<int> total = new HashSet<int>();
		needy.SetNeedyTimeRemaining(3f); //Prevents timer from ending midspin
		foreach (slotScript slot in slots) {
			total.Add(slot.spinSlot()); //Tells me result + animation
			yield return new WaitForSeconds (0.05f); //Delay to make animation look nicer
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
		}
		yield return new WaitForSeconds (1f); //Slight delay to prevent spam and complete animations
		slotState = 2;
		StartCoroutine("lightupSign");
		checkSet(total);
		StartCoroutine("returnAnimation");
		nonPlayer = false;
	}
	void PickSignType() {
		signType = UnityEngine.Random.Range(0,2);
	}
	IEnumerator lightupSign() {
		if (slotState <= 1) {
			foreach (GameObject setOfLights in lights) {
				setOfLights.SetActive(false);
			}
			yield break;
		}
		bool currState = lights[0].activeSelf;
		currState = !currState;
		foreach (GameObject setOfLights in lights) {
			setOfLights.SetActive(currState);
			if (signType == 0) { //Flicker
				currState = !currState;
			}
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
		}
		yield return new WaitForSeconds (flickerRate);
		StartCoroutine("lightupSign");
	}
}
