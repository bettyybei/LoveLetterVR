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
	const string _GameStatusWin = "Game over. You win!";
	const string _GameStatusLose = "Game over. You lost.";
	const string _GameStatusTie = "Game over. It was a tie!";

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
		currentPlayerCount = 2;

		deck = ShuffleDeck(deck);

		// Picking initial cards
		while (nextStateIdx <= currentPlayerCount) {
			PlayerController player = players.Dequeue();
			player.SetState(deck[nextStateIdx++]);
			players.Enqueue(player);
		}

		// Start first player's turn
		currentPlayer = players.Dequeue();
		StartPlayersTurn (deck [nextStateIdx++], deck [nextStateIdx]);
	}

	void Update () {
		if (currentPlayer.GetIsChoosingOwnState() != twoStateMenuObject.activeSelf) {
			twoStateMenuObject.SetActive(currentPlayer.GetIsChoosingOwnState());
		} else if (currentPlayer.GetIsChoosingMenuState() != stateMenuObject.activeSelf) {
			stateMenuObject.SetActive(currentPlayer.GetIsChoosingMenuState());
		}

		if (nextStateIdx == 16) {
			if (player1.GetCurrentState () < player2.GetCurrentState ()) {
				player1.gameStatusTextObject.text = _GameStatusLose;
				player2.gameStatusTextObject.text = _GameStatusWin;
			} else if (player1.GetCurrentState () > player2.GetCurrentState ()) {
				player1.gameStatusTextObject.text = _GameStatusWin;
				player2.gameStatusTextObject.text = _GameStatusLose;
			} else {
				player1.gameStatusTextObject.text = _GameStatusTie;
				player2.gameStatusTextObject.text = _GameStatusTie;
			}

		}
		else if (!currentPlayer.GetIsDoingTurn()) {
			if (currentPlayer.GetCurrentState() == State.Dead) {
				currentPlayerCount--;
			} else {
				// Check if the player used the next card
				if (currentPlayer.GetUsedNextState ()) {
					nextStateIdx++;
					currentPlayer.SetUsedNextState (false);
				}
				// Only put player back in queue if they didn't die this round
				players.Enqueue(currentPlayer);
			}
			currentPlayer = players.Dequeue();
			if (currentPlayerCount == 1) {
				currentPlayer.gameStatusTextObject.text = _GameStatusWin;
			}
			else if (currentPlayer.GetCurrentState() != State.Dead) {	
				StartPlayersTurn (deck [nextStateIdx++], deck [nextStateIdx]);
			}
		}
	}

	void StartPlayersTurn(State next, State nextnext) {
		State previous = currentPlayer.GetCurrentState ();
		currentPlayer.SetIsDoingTurn(true);
		stateCard1.SetState(previous);
		stateCard2.SetState(next);
		currentPlayer.StartTurn(next, nextnext);
	}

	State[] ShuffleDeck(State[] deck) {
		for (int i = deck.Length-1; i > 0; i--) {
			int j = Random.Range(0, i);
			State temp = deck[i];
			deck[i] = deck[j];
			deck[j] = temp;
		}
		return deck;
	}
}
