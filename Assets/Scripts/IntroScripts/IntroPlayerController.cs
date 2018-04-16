using UnityEngine;
using UnityEngine.SceneManagement;
using FRL.IO;
[RequireComponent(typeof(Receiver))]

public class IntroPlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler, IGlobalTouchpadPressDownHandler {

    public Renderer ScrollRenderer;
    private int i = 1;

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

    public void OnGlobalTouchpadPressDown(XREventData eventData) {
        if (Holojam.Tools.BuildManager.IsMasterClient()) return;
        i = (i + 1) % 4;
        ScrollRenderer.material = Resources.Load<Material>("Materials/Instructions_" + i);
    }
}
