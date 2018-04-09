using UnityEngine;
using UnityEngine.SceneManagement;
using FRL.IO;
[RequireComponent(typeof(Receiver))]

public class IntroPlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnGlobalTriggerPressDown(XREventData eventData) {
        Debug.Log("Trigger Press");
        if (Holojam.Tools.BuildManager.IsMasterClient()) return;
        SceneManager.LoadScene("LoveLetterGame");
    }
}
