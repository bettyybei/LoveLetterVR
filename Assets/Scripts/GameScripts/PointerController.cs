using UnityEngine;

public class PointerController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;

    MeshRenderer pointerRenderer;
    private PlayerController player;

	void Start () {
        pointerRenderer = gameObject.GetComponentsInChildren<MeshRenderer>()[0];
        pointerRenderer.enabled = false;

        switch (Holojam.Tools.BuildManager.BUILD_INDEX) {
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
        if (pointerEnabled != pointerRenderer.enabled) {
            Debug.Log("Toggling Pointer");
            pointerRenderer.enabled = pointerEnabled;
        }
    }
}
