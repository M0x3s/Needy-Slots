using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slotScript : MonoBehaviour {
	public Animator animator;
	public GameObject values;
	public int spinSlot() {
		animator.Play("SlotStart");
		int totalValues = values.transform.childCount;
		int shift = UnityEngine.Random.Range(0, totalValues); // X = 0, B = 1, L = 2, T = 3
		// for testing:
		// shift = 0;
		// shift = 2;
		StartCoroutine("updateRotation", shift * 360/totalValues);
		return shift;
	}
	IEnumerator updateRotation(int shift) {
		yield return new WaitForSeconds (0.5f);
		values.transform.localRotation = Quaternion.Euler(0, shift, 0);
	}
}
