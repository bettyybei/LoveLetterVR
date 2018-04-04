using UnityEngine;

public class PointerController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;

    private PlayerController player;

	void Start () {
		switch(Holojam.Tools.BuildManager.BUILD_INDEX) {
            case 1:
                player = player1;
                break;
            case 2:
                player = player2;
                break;
            default:
                Debug.Log("ERROR IN POINTER");
                break;
        }
    }
	
	void Update () {
        if (!player) return;
        bool pointerEnabled = player.IsChoosingOtherPlayer || player.IsChoosingOwnState || player.IsChoosingMenuState;
        Debug.Log("POINTER ENABLED " + pointerEnabled);
        if (pointerEnabled != gameObject.activeSelf) {
            gameObject.SetActive(pointerEnabled);
        }
    }
}
