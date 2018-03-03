using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using Holojam.Vive;
public class GameMaster: MonoBehaviour {
    public enum State { Dead, Guard, Priest, Baron, Handmaid, Prince, King, Countess, Princess }

    public PlayerController player1;
    public PlayerController player2;
    public ViveControllerReceiver receiver1;
    public ViveControllerReceiver receiver2;

    public GameObject stateMenuObject;
    public GameObject twoStateMenuObject;
    StateController stateCard1;
    StateController stateCard2;

    const string _GameStatusWin = "Game over. You win!";
    const string _GameStatusLose = "Game over. You lost.";
    const string _GameStatusTie = "Game over. It was a tie!";

    PlayerController[] players;

    int currentPlayerCount;

    // synced objects : deck, nextStateIdx, playerStates, currentPlayerIdx
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
        
    public int nextStateIdx = 1; // Skips card at index 0 because one card is taken out.
    public State[] playerStates; // playerStates will always be max # of players
    public int currentPlayerIdx = 0;

    void Start () {
        // Populate State Menu State Controllers
        StateController[] menuStateControllers = stateMenuObject.GetComponentsInChildren<StateController>();
        for (int i = 0; i < menuStateControllers.Length; i++) {
            menuStateControllers[i].SetState((State)(i + 1));
        }
        StateController[] cardStateControllers = twoStateMenuObject.GetComponentsInChildren<StateController>();
        stateCard1 = cardStateControllers[0];
        stateCard2 = cardStateControllers[1];

        deck = ShuffleDeck(deck);

        players = new PlayerController[] {
            player1, player2
        };

        currentPlayerCount = players.Length;
        playerStates = new State[4];
        
        if (Holojam.Tools.BuildManager.IsMasterClient()) {
            // Picking initial cards
            for (int i = 0; i < currentPlayerCount; i++) {
                State s = deck[nextStateIdx++];
                players[i].SetState(s);
                playerStates[i] = s;
            }
            // Start first player's turn
            StartPlayersTurn(deck[nextStateIdx++], deck[nextStateIdx]);
        }
        else {
            for (int i = 0; i < currentPlayerCount; i++) {
                if (Holojam.Tools.BuildManager.BUILD_INDEX - 1 == i) {
                    //players[i].pointerObject.SetActive(true);
                    //players[i].stateTextObject.gameObject.SetActive(true);
                    players[i].gameStatusTextObject.gameObject.SetActive(true);
                } else {
                    //players[i].pointerObject.SetActive(false);
                    //players[i].stateTextObject.gameObject.SetActive(false);
                    players[i].gameStatusTextObject.gameObject.SetActive(false);
                }
            }
        }
    }

    void Update () {
        PlayerController currentPlayer = players[currentPlayerIdx];
        
        if (Holojam.Tools.BuildManager.IsMasterClient()) {
            //I am the master client. Manage game state.

            ViveControllerReceiver receiver = receiver1;
            switch((currentPlayerIdx+1)) {
                case 1:
                    receiver = receiver1;
                    break;
                case 2:
                    receiver = receiver2;
                    break;
            }

            if (Input.GetKeyDown("space") && nextStateIdx < 16) {
                currentPlayerIdx = (currentPlayerIdx + 1) % players.Length;
                currentPlayer = players[currentPlayerIdx];
                StartPlayersTurn(deck[nextStateIdx++], deck[nextStateIdx]);
            }

            if (nextStateIdx == 16) {
                // Game over. Calculate winner(s)
                int winnerIdx = 0;
                int tie1 = -1;
                int tie2 = -1; // 3 way tie is possible
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

                // Set Game Status Text
                for (int i = 0; i < players.Length; i++) {
                    if (i == winnerIdx || i == tie1 || i == tie2) {
                        if (tie1 > 0)
                            players[i].gameStatusTextObject.text = _GameStatusTie;
                        else
                            players[i].gameStatusTextObject.text = _GameStatusWin;
                    } else
                        players[i].gameStatusTextObject.text = _GameStatusLose;
                }
                nextStateIdx++;

            }
            else if (receiver.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip) && nextStateIdx < 16) {
                // Sync up and count player states after the end of each turn
                currentPlayerCount = 0;
                for (int i = 0; i < players.Length; i++) {
                    State s = players[i].CurrentState;
                    if (s != State.Dead)
                        currentPlayerCount++;
                    if (playerStates[i] != s)
                        playerStates[i] = s;
                }

                // When currentPlayer used Prince card, advance the deck another card
                if (currentPlayer.UsedNextState) { 
                    nextStateIdx++;
                    currentPlayer.UsedNextState = false;
                }

                if (currentPlayerCount == 1) {
                    nextStateIdx = 16; //this ends the game in the next Update call
                    return;
                }

                if (currentPlayerCount > 1) {
                    do {
                        currentPlayerIdx = (currentPlayerIdx + 1) % players.Length;
                        currentPlayer = players[currentPlayerIdx];
                    } while (currentPlayer.CurrentState == State.Dead);
                }

                if (currentPlayerCount > 1 && currentPlayer.CurrentState != State.Dead) {
                    StartPlayersTurn(deck[nextStateIdx++], deck[nextStateIdx]);
                }
            }
        }
        else {
            // Sync playerStates from Master Client
            for (int i = 0; i < players.Length; i++) {
                State s = playerStates[i];
                if (players[i].CurrentState != s) {
                    players[i].SetState(s);
                    Debug.Log("SYNCING STATE");
                    Debug.Log("player " + (i + 1) + " state should be " + s);
                }
            }

            if (!currentPlayer.IsDoingTurn) {

                // Make sure all other players and not doing their turn
                for (int i = 0; i < players.Length; i++) {
                    if (players[i] != currentPlayer) {
                        players[i].IsDoingTurn = false;
                    }
                }

                if (currentPlayer.CurrentState != State.Dead) {
                    Debug.Log("Start players turn");
                    StartPlayersTurn(deck[nextStateIdx - 1], deck[nextStateIdx]);
                    return;
                }
                else {
                    Debug.Log("Illegal State");
                    // currentPlayer state == dead is not possible,
                    // wait for the next Update() for the states to be synced
                    return;
                }
            }

            PlayerController clientPlayer = players[Holojam.Tools.BuildManager.BUILD_INDEX - 1];

            if (currentPlayer == clientPlayer) {
                // currentPlayer is me

                // Show menus based on what the player is currently choosing
                if (currentPlayer.IsChoosingOwnState != twoStateMenuObject.activeSelf) {
                    twoStateMenuObject.SetActive(currentPlayer.IsChoosingOwnState);
                }
                else if (currentPlayer.IsChoosingMenuState != stateMenuObject.activeSelf) {
                    stateMenuObject.SetActive(currentPlayer.IsChoosingMenuState);
                }
            }
            else {
                // clientPlayer is me
                if (twoStateMenuObject.activeSelf != false) twoStateMenuObject.SetActive(false);
                if (stateMenuObject.activeSelf != false) stateMenuObject.SetActive(false);
                clientPlayer.SetGameStatus("Player " + (currentPlayerIdx + 1) + " is doing their turn");
            }
        }

    }

    void StartPlayersTurn(State next, State nextnext) {
        PlayerController currentPlayer = players[currentPlayerIdx];
        State previous = currentPlayer.CurrentState;
        currentPlayer.IsDoingTurn = true;
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
