using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slotScript : MonoBehaviour {
	public Animator animator;
	public GameObject values;
	public int spinSlot() {
		animator.Play("SlotStart");
		int totalValues = values.transform.childCount;
		int shift = UnityEngine.Random.Range(0, totalValues);
		shift = 0;
		StartCoroutine("updateRotation", shift * 360/totalValues);
		int total = 1;
		for (int i = 0; i < shift; i++) {
			total *= 2;
		}
		return total;
	}
	IEnumerator updateRotation(int shift) {
		yield return new WaitForSeconds (0.5f);
		values.transform.localRotation = Quaternion.Euler(0, shift, 0);
	}
}
