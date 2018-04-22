using System.Text;
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
                Debug.Log("Master Game Status Text");
                break;
        }
    }
    void Update() {
        if (!player) {
            StringBuilder sb = new StringBuilder();
            sb.Append(player1.GameStatusText).Append("\n");
            sb.Append(player2.GameStatusText).Append("\n");
            textMesh.text = sb.ToString();
        }
        else {
            string currentStatus = player.GameStatusText;
            if (previousFrameStatus.Equals(currentStatus)) return;
            previousFrameStatus = currentStatus;
            textMesh.text = currentStatus;
        }
    }
}
