using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster: MonoBehaviour {
	public enum State { Dead, Guard, Priest, Baron, Handmaid, Prince, King, Countess, Princess }

	public PlayerController player1;
	public PlayerController player2;

	public GameObject stateMenuObject;
	public GameObject twoStateMenuObject;
	StateController stateCard1;
	StateController stateCard2;

	int currentPlayerCount;

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
		// Populate State Menu State Controllers
		StateController[] menuStateControllers = stateMenuObject.GetComponentsInChildren<StateController>();
		for (int i=0; i<menuStateControllers.Length; i++) {
			menuStateControllers[i].SetState ((State)(i + 1));
		}
		StateController[] cardStateControllers = twoStateMenuObject.GetComponentsInChildren<StateController>();
		stateCard1 = cardStateControllers [0];
		stateCard2 = cardStateControllers [1];

		players = new Queue<PlayerController>();
		players.Enqueue (player1);
		players.Enqueue (player2);
		//deck = ShuffleDeck(deck);
		currentPlayerCount = 2;
		while (nextStateIdx <= currentPlayerCount) {
			PlayerController player = players.Dequeue();
			player.SetState(deck[nextStateIdx++]);
			players.Enqueue(player);
		}
		currentPlayer = players.Dequeue();
		StartPlayersTurn (deck [nextStateIdx++]);
	}
	
	// Update is called once per frame
	void Update () {
		if (currentPlayer.GetIsChoosingOwnState() != twoStateMenuObject.activeSelf) {
			Debug.Log ("setting state cards to " + currentPlayer.GetIsChoosingOwnState());
			twoStateMenuObject.SetActive(currentPlayer.GetIsChoosingOwnState());
		} else if (currentPlayer.GetIsChoosingMenuState() != stateMenuObject.activeSelf) {
			Debug.Log ("setting state menu to " + currentPlayer.GetIsChoosingMenuState());
			stateMenuObject.SetActive(currentPlayer.GetIsChoosingMenuState());
		}

		if (currentPlayerCount == 1 || nextStateIdx == 16) {
			// game over
		} else if (!currentPlayer.GetIsDoingTurn()) {
			if (currentPlayer.GetCurrentState() == State.Dead) {
				currentPlayerCount--;
			} else {
				// Only put player back in queue if they didn't die this round
				players.Enqueue(currentPlayer);
				currentPlayerCount--;
			}
			currentPlayer = players.Dequeue();
			StartPlayersTurn(deck[nextStateIdx++]);
		}
	}

	void StartPlayersTurn(State next) {
		State previous = currentPlayer.current;
		currentPlayer.SetIsDoingTurn(true);
		stateCard1.SetState(previous);
		stateCard2.SetState(next);
		Debug.Log ("Choice 1: " + previous + " Choice 2: " + next);
		currentPlayer.StartTurn(next);
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
