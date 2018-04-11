using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;

    ParticleSystem particleSystemObject;
    private PlayerController player;
    const string _GameStatusWin = "Game over. You win!";

    void Start() {
        particleSystemObject = gameObject.GetComponent<ParticleSystem>();
        particleSystemObject.Stop();

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
        if (_GameStatusWin.Equals(player.GetGameStatus())) {
            Debug.Log("Enabling Particles");
            particleSystemObject.Play();
        }
        else {
            if (particleSystemObject.isPlaying) {
                Debug.Log("Stopping Particles");
                particleSystemObject.Stop();
            }
        }
    }
}
