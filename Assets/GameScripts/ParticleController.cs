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
                Debug.Log("ERROR IN PARTICLE CONTROLLER");
                break;
        }
    }

    void Update() {
        if (!player) return;
        if (_GameStatusWin.Equals(player.GetGameStatus())) {
            Debug.Log("Enabling Particles");
            if (!particleSystemObject.isPlaying) particleSystemObject.Play();
        }
        else {
            if (particleSystemObject.isPlaying) {
                Debug.Log("Stopping Particles");
                particleSystemObject.Stop();
            }
        }
    }
}
