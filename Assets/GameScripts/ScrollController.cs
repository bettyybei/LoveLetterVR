using UnityEngine;

public class ScrollController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;

    private Renderer scrollRenderer;
    private PlayerController player;
    private int previousInstructionNum;

    void Start () {
        scrollRenderer = GetComponent<Renderer>();
        switch (Holojam.Tools.BuildManager.BUILD_INDEX) {
            case 1:
                player = player1;
                break;
            case 2:
                player = player2;
                break;
            default:
                Debug.Log("ERROR IN SCROLL CONTROLLER");
                break;
        }
    }
	
	void Update () {
        if (!player) return;
        if (previousInstructionNum == player.InstructionNum) return;
        previousInstructionNum = player.InstructionNum;
        if (player.InstructionNum == -1) {
            scrollRenderer.enabled = false;
        } else {
            scrollRenderer.enabled = true;
            scrollRenderer.material = Resources.Load<Material>("Materials/Instructions_" + player.InstructionNum);
        }
    }
}
