using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;

    MeshRenderer particleRenderer;
    private PlayerController player;
    const string _GameStatusWin = "Game over. You win!";

    void Start() {
        particleRenderer = gameObject.GetComponent<MeshRenderer>();
        particleRenderer.enabled = false;

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

    void Update() {
        if (!player) return;
        Debug.Log("game status " + player.GetGameStatus());
        if (_GameStatusWin.Equals(player.GetGameStatus())) {
            particleRenderer.enabled = true;
        }
    }
}
