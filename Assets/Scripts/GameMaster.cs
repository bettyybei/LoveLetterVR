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

	Random _Random = new Random();

	const string _GameStatusWin = "Game over. You win!";
	const string _GameStatusLose = "Game over. You lost.";
	const string _GameStatusTie = "Game over. It was a tie!";

	PlayerController[] players;

	int currentPlayerCount;

	public State[] deck = new State[] {
		State.Guard, State.Guard, State.Guard, State.Guard, State.Guard, 
		State.Priest, State.Priest,
		State.Baron, State.Baron,
		State.Handmaid, State.Handmaid,
		State.Prince, State.Prince,
		State.King,
		State.Countess,
		State.Princess
	};

	public State[] playerStates;

	public int currentPlayerIdx = 0;
	public int nextStateIdx = 1; // Skips card at index 0 because one card is taken out.

	void Start () {
		// Populate State Menu State Controllers
		StateController[] menuStateControllers = stateMenuObject.GetComponentsInChildren<StateController>();
		for (int i=0; i<menuStateControllers.Length; i++) {
			menuStateControllers[i].SetState ((State)(i + 1));
		}
		StateController[] cardStateControllers = twoStateMenuObject.GetComponentsInChildren<StateController>();
		stateCard1 = cardStateControllers [0];
		stateCard2 = cardStateControllers [1];

		deck = ShuffleDeck(deck);


		players = new PlayerController[] {
			player1, player2
		};
		currentPlayerCount = players.Length;
		playerStates = new State[currentPlayerCount];

		// Picking initial cards
		for (int i = 0; i < currentPlayerCount; i++) {
			State s = deck[nextStateIdx++];
			playerStates[i] = s;
			players[i].SetState(s);
		}

		// Start first player's turn
		//currentPlayer = players[currentPlayerIdx];
		StartPlayersTurn (deck [nextStateIdx++], deck [nextStateIdx]);
	}

	void Update () {
		PlayerController currentPlayer = players [currentPlayerIdx];
		if (currentPlayer.GetIsChoosingOwnState() != twoStateMenuObject.activeSelf) {
			twoStateMenuObject.SetActive(currentPlayer.GetIsChoosingOwnState());
		} else if (currentPlayer.GetIsChoosingMenuState() != stateMenuObject.activeSelf) {
			stateMenuObject.SetActive(currentPlayer.GetIsChoosingMenuState());
		}

		if (nextStateIdx == 16) {
			int winnerIdx = 0;
			int tie1 = -1; 
			int tie2 = -1; // rare, but a 3 way tie is possible

			for (int i = 1; i < players.Length; i++) {
				State state = players[i].CurrentState;
				State maxState = players[winnerIdx].CurrentState;
				if (state > maxState) {
					winnerIdx = i;
					tie1 = -1;
					tie2 = -1;
				}
				if (state == maxState) {
					// save tied indices
					if (tie1 > 0)
						tie2 = i;
					else
						tie1 = i;
				}
			}

			for (int i = 0; i < players.Length; i++) {
				if (i == winnerIdx || i == tie1 || i == tie2)
				if (tie1 > 0)
					players [i].gameStatusTextObject.text = _GameStatusTie;
				else {
					players [i].gameStatusTextObject.text = _GameStatusWin;
					Debug.Log (players [i] + " set to win here ");
				}
				else 
					players[i].gameStatusTextObject.text = _GameStatusLose;
			}
			nextStateIdx++;
		}
		else if (!currentPlayer.GetIsDoingTurn()) {
			// sync up player states after the end of each turn ASK ABOUT THIS
			for (int i = 0; i < players.Length; i++) {
				State s = players [i].CurrentState;
				if (playerStates [i] != s) {
					playerStates [i] = s;
				}
			}

			if (currentPlayer.CurrentState == State.Dead) {
				currentPlayerCount--;
			} else {
				// Check if the player used the next card
				if (currentPlayer.GetUsedNextState ()) {
					nextStateIdx++;
					currentPlayer.SetUsedNextState(false);
				}
			}

			if (currentPlayerCount == 1) {
				nextStateIdx = 16; //this ends the game
			}
			//Debug.Log("idx " + currentPlayerIdx);
			if (currentPlayerCount > 1) {
				do {
					currentPlayerIdx = (currentPlayerIdx + 1) % players.Length;
					currentPlayer = players [currentPlayerIdx];
					Debug.Log (currentPlayerIdx + " is idx, current player is " + currentPlayer.name);
				} while(currentPlayer.CurrentState == State.Dead);
			}
		//	if (currentPlayerCount == 1 && currentPlayer.CurrentState != State.Dead) {
		//		Debug.Log (currentPlayer + " set to win here ");
		//		currentPlayer.gameStatusTextObject.text = _GameStatusWin;
		//	}
			if (currentPlayerCount > 1 && currentPlayer.CurrentState != State.Dead) {
				StartPlayersTurn(deck[nextStateIdx++], deck[nextStateIdx]);
			}
		}
	}

	void StartPlayersTurn(State next, State nextnext) {
		PlayerController currentPlayer = players[currentPlayerIdx];
		State previous = currentPlayer.CurrentState;
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
