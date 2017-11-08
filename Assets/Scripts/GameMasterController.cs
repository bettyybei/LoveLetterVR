using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = PlayerController.State;

public class GameMasterController : MonoBehaviour {

	public PlayerController player1;
	public PlayerController player2;

	Random _Random = new Random();

	Queue<PlayerController> players;
	State[] deck = new State[] {
		State.Guard, State.Guard, State.Guard, State.Guard, State.Guard, 
		State.Priest, State.Priest,
		State.Baron, State.Baron,
		State.Handmaid, State.Handmaid,
		State.Prince, State.Prince,
		State.King,
		State.Countess,
		State.Princess
	};

	PlayerController currentPlayer;

	int nextStateIdx = 1; // Skips card at index 0 because one card is taken out.

	// Use this for initialization
	void Start () {
		players = new Queue<PlayerController>();
		players.Enqueue (player1);
		players.Enqueue (player2);
		//deck = ShuffleDeck(deck);
		while (nextStateIdx <= players.Count) {
			PlayerController player = players.Dequeue();
			Debug.Log (player);
			player.SetState(deck[nextStateIdx++]);
			players.Enqueue(player);
		}
		currentPlayer = players.Dequeue();
		currentPlayer.isDoingTurn = true;
		currentPlayer.StartTurn(deck[nextStateIdx++]);
	}
	
	// Update is called once per frame
	void Update () {
		if (!currentPlayer.isDoingTurn && nextStateIdx == 16) {
			// game over
		}
		else if (!currentPlayer.isDoingTurn) {
			players.Enqueue(currentPlayer);
			currentPlayer = players.Dequeue();
			currentPlayer.isDoingTurn = true;
			currentPlayer.StartTurn(deck[nextStateIdx++]);
		}
	}

	State[] ShuffleDeck(State[] deck) {
		for (int i = deck.Length; i > 0; i--) {
			int j = Random.Range(0, i);
			State temp = deck[i];
			deck[i] = deck[j];
			deck[j] = temp;
		}
		return deck;
	}
}
