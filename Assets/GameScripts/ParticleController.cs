using UnityEngine;

public class ParticleController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;
    public PlayerController player3;
    
    private PlayerController player;
    ParticleSystem particleSystemObject;

    const string _GameStatusWin = "Game over. You win!";
    const string _GameStatusTie = "Game over. It was a tie!";

    void Start() {
        particleSystemObject = gameObject.GetComponent<ParticleSystem>();
        particleSystemObject.Stop();

        PlayerController[] players = new PlayerController[] {
            null, player1, player2, player3
        };
        player = players[Holojam.Tools.BuildManager.BUILD_INDEX];
    }

    void Update() {
        if (!player) return;
        if ((_GameStatusWin.Equals(player.GetGameStatus()) || _GameStatusTie.Equals(player.GetGameStatus())) && !particleSystemObject.isPlaying) {
            Debug.Log("Enabling Particles");
            particleSystemObject.Play();
        } else if (particleSystemObject.isPlaying) {
            Debug.Log("Stopping Particles");
            particleSystemObject.Stop();
        }
    }
}
