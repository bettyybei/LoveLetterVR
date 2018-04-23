using System.Text;
using UnityEngine;

public class GameStatusTextController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;
    public PlayerController player3;

    private PlayerController[] players;
    private PlayerController player;
    private string previousFrameStatus = "";
    private TextMesh textMesh;

    void Start() {
        textMesh = GetComponent<TextMesh>();
        players = new PlayerController[] {
            null, player1, player2, player3
        };
        player = players[Holojam.Tools.BuildManager.BUILD_INDEX];
    }
    void Update() {
        if (!player) {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < players.Length; i++) {
                sb.Append(players[i].CurrentState).Append("\n").Append(players[i].GameStatusText).Append("\n");
            }
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
