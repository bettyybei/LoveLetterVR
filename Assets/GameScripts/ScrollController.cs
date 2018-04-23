using UnityEngine;

public class ScrollController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;
    public PlayerController player3;
    
    private PlayerController player;
    private Renderer scrollRenderer;
    private int previousInstructionNum;

    void Start () {
        scrollRenderer = GetComponent<Renderer>();
        PlayerController[] players = new PlayerController[] {
            null, player1, player2, player3
        };
        player = players[Holojam.Tools.BuildManager.BUILD_INDEX];
        if (!player) scrollRenderer.enabled = false;
    }
	
	void Update () {
        if (!player || previousInstructionNum == player.InstructionNum) return;
        previousInstructionNum = player.InstructionNum;
        if (player.InstructionNum == -1) { // -1 means instructions are closed
            scrollRenderer.enabled = false;
        } else {
            scrollRenderer.enabled = true;
            scrollRenderer.material = Resources.Load<Material>("Materials/Instructions_" + player.InstructionNum);
        }
    }
}
