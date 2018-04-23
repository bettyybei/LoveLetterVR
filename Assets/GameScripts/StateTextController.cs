using UnityEngine;
using State = GameMaster.State;

public class StateTextController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;
    public PlayerController player3;
    
    private PlayerController player;
    private State previousFrameState;
    private TextMesh textMesh;

    void Start() {
        textMesh = GetComponent<TextMesh>();
        PlayerController[] players = new PlayerController[] {
            null, player1, player2, player3
        };
        player = players[Holojam.Tools.BuildManager.BUILD_INDEX];
    }

    void Update () {
        if (!player || previousFrameState == player.CurrentState) return;
        previousFrameState = player.CurrentState;
        string newStateText = "";
        if (player.CurrentState == State.Dead) {
            newStateText = "Out of the round";
        } else {
            newStateText = player.CurrentState.ToString();
        }
        textMesh.text = newStateText;
	}
}
