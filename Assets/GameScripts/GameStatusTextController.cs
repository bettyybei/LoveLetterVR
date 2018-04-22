using UnityEngine;

public class GameStatusTextController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;

    private string previousFrameStatus = "";
    private TextMesh textMesh;
    private PlayerController player;

    void Start() {
        textMesh = GetComponent<TextMesh>();
        switch (Holojam.Tools.BuildManager.BUILD_INDEX) {
            case 1:
                player = player1;
                break;
            case 2:
                player = player2;
                break;
            default:
                Debug.Log("ERROR IN GAME STATUS STATE TEXT");
                break;
        }
    }
    void Update() {
        if (!player) return;
        string currentStatus = player.GetGameStatus();
        if (previousFrameStatus.Equals(currentStatus)) return;
        previousFrameStatus = currentStatus;
        textMesh.text = currentStatus;
    }
}
