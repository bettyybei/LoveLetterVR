using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTextController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;

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
                Debug.Log("ERROR IN STATE TEXT");
                break;
        }
    }

    // Update is called once per frame
    void Update () {
        string stateText = "";
        if (player.CurrentState == GameMaster.State.Dead) {
            stateText = "Out of the round";
        }
        else {
            stateText = "Currently Holding: " + player.CurrentState;
        }
        
        if (!textMesh.text.Equals(stateText)) {
            textMesh.text = stateText;
        }
	}
}
