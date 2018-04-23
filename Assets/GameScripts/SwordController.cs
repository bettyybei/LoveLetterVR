using UnityEngine;

public class SwordController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;
    public PlayerController player3;
    public MeshRenderer player1SwordRenderer;
    public MeshRenderer player2SwordRenderer;
    public MeshRenderer player3SwordRenderer;

    private PlayerController player;
    private MeshRenderer swordGlowRenderer;
    private MeshRenderer swordRenderer;

	void Start () {
        swordGlowRenderer = gameObject.GetComponentsInChildren<MeshRenderer>()[0];
        swordGlowRenderer.enabled = false;

        PlayerController[] players = new PlayerController[] {
            null, player1, player2, player3
        };
        MeshRenderer[] swordRenderers = new MeshRenderer[] {
            null, player1SwordRenderer, player2SwordRenderer, player3SwordRenderer
        };

        player = players[Holojam.Tools.BuildManager.BUILD_INDEX];
        swordRenderer = swordRenderers[Holojam.Tools.BuildManager.BUILD_INDEX];
    }
	
	void Update () {
        if (!player) return;
        bool glowEnabled = player.IsChoosingOtherPlayer || player.IsChoosingOwnState || player.IsChoosingMenuState;
        if (glowEnabled != swordGlowRenderer.enabled) {
            Debug.Log("Toggling Sword Glow " + glowEnabled);
            swordGlowRenderer.enabled = glowEnabled;
            swordRenderer.enabled = !glowEnabled;
        }
    }
}
