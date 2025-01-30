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
	private bool autoSolve; //for Twitch
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
		 if (autoSolve) {
			StartCoroutine("TwitchHandleForcedSolve");
		 }
	 }
	 void OnNeedyDeactivation()
	{
		slotState = 1;
		StopAllCoroutines();
		StartCoroutine("quickRevertAnimation");
		StartCoroutine("lightupSign");
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
	void checkSet(List<int> result) {
		int Xcount = 0;
		foreach (int i in result) {
			if (i != 0) {
				break;
			}
			Xcount++;
		}
		if (Xcount == 3) {
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
		needy.SetNeedyTimeRemaining(45f);

		List<List<int>> options; // X = 0, B = 1, L = 2, T = 3
		if (signType == 0) { //Alternating
			switch (bomb.GetStrikes()) {
				case 0: options = new List<List<int>>
				{
					new List<int> {2, 2, 2},
					new List<int> {2, 1, 3},
					new List<int> {1, 1, 3}
				};
				break;
				case 1: options = new List<List<int>>
				{
					new List<int> {2, 2, 3},
					new List<int> {2, 1, 0},
					new List<int> {1, 3, 3}
				};
				break;
				default: options = new List<List<int>>
				{
					new List<int> {2, 2, 2},
					new List<int> {3, 3, 0},
					new List<int> {1, 3, 0}
				};
				break;
			}
		}
		else { //Solid
			switch (bomb.GetStrikes()) {
				case 0: options = new List<List<int>>
				{
					new List<int> {2, 0, 0},
					new List<int> {2, 1, 3},
					new List<int> {3, 3, 3}
				};
				break;
				case 1: options = new List<List<int>>
				{
					new List<int> {2, 2, 2},
					new List<int> {2, 3, 0},
					new List<int> {1, 1, 0}
				};
				break;
				default: options = new List<List<int>>
				{
					new List<int> {1, 3, 0},
					new List<int> {1, 1, 0},
					new List<int> {1, 1, 1}
				};
				break;
			}
		}
		foreach (List<int> i in options) {
			List<int> usedIndex = new List<int>();
			for (int j=0; j<i.Count; j++) {
				for (int k=0; k<result.Count; k++) {
					if (usedIndex.Contains(k)) {
						continue;
					}
					if (i[j] == result[k]) {
						usedIndex.Add(k);
						break;
					}
				}
			}
			if (usedIndex.Count == result.Count) {
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
		List<int> total = new List<int>();
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
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Use !{0} roll to pull the lever.";
	#pragma warning disable 414

	IEnumerator ProcessTwitchCommand (string Command) {
		Command = Command.Trim().ToLower();
		yield return null;
		if (Command != "roll") {
			yield return "sendtochaterror I don't understand!";
			yield break;
		}
		if (slotState == 1) {
			yield return "sendtochaterror Already rolling!";
			yield break;
		}
		knob.OnInteract();
	}
	IEnumerator TwitchHandleForcedSolve () {
		autoSolve = true;
		while (isActive) {
			knob.OnInteract();
			yield return new WaitForSeconds(1f);
		}
	}
}
