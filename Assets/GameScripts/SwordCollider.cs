using System.Collections;
using UnityEngine;

public class SwordCollider : MonoBehaviour {

    public SteamVR_TrackedObject controller;

    void Start () {
		
	}

    private void OnTriggerEnter(Collider other) {
        if (other.tag.Equals("Sword")) {
          StartCoroutine(LongVibration(0.3f, 3999));
        }
  }

    IEnumerator LongVibration(float length, float strength) {
        for (float i = 0; i < length; i += Time.deltaTime) {
            SteamVR_Controller.Input((int)controller.index).TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, strength));
            yield return null;
        }
    }
}
