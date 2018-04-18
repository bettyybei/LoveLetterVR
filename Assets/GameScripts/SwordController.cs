using UnityEngine;

public class SwordController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;
    public MeshRenderer player1Sword;
    public MeshRenderer player2Sword;

    private MeshRenderer swordGlowRenderer;
    private PlayerController player;
    private MeshRenderer swordRenderer;

	void Start () {
        swordGlowRenderer = gameObject.GetComponentsInChildren<MeshRenderer>()[0];
        swordGlowRenderer.enabled = false;

        switch (Holojam.Tools.BuildManager.BUILD_INDEX) {
            case 1:
                player = player1;
                swordRenderer = player1Sword;
                break;
            case 2:
                player = player2;
                swordRenderer = player2Sword;
                break;
            default:
                Debug.Log("ERROR IN SWORD CONTROLLER");
                break;
        }
    }
	
	void Update () {
        if (!player) return;
        bool pointerEnabled = player.IsChoosingOtherPlayer || player.IsChoosingOwnState || player.IsChoosingMenuState;
        if (pointerEnabled != swordGlowRenderer.enabled) {
            Debug.Log("Toggling Sword Glow");
            swordGlowRenderer.enabled = pointerEnabled;
            swordRenderer.enabled = !pointerEnabled;
        }
    }
}
