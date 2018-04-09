using UnityEngine;
using UnityEngine.SceneManagement;
using FRL.IO;
[RequireComponent(typeof(Receiver))]

public class IntroPlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

	void Update () {
		if (Holojam.Tools.BuildManager.IsMasterClient()) {
            if (Input.GetKeyDown("space")) {
                SceneManager.LoadScene("LoveLetterGame");
            }
        }
	}

    public void OnGlobalTriggerPressDown(XREventData eventData) {
        SceneManager.LoadScene("LoveLetterGame");
    }
}
