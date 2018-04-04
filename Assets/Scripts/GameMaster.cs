﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using Holojam.Vive;
using Holojam.Tools;

public class GameMaster: MonoBehaviour {
    public enum State { Dead, Guard, Priest, Baron, Handmaid, Prince, King, Countess, Princess }

    public PlayerController player1;
    public PlayerController player2;

    public GameObject stateMenuObject;
    public GameObject twoStateMenuObject;
    public StateController stateCard1 { get; private set; }
    public StateController stateCard2 { get; private set; }

    const string _GameStatusWin = "Game over. You win!";
    const string _GameStatusLose = "Game over. You lost.";
    const string _GameStatusTie = "Game over. It was a tie!";
    const string _ChooseOwnStateText = "Choose a card you would like to discard and enact its power";
    const string _ChooseOtherPlayerStateText = "Choose a state you believe another player has";
    const string _ChooseAnotherPlayerText = "Choose another player ";

    public PlayerController[] players;

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
        
    public int nextStateIdx = 1; // Skips card at index 0 because one card is taken out.
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

        for (int i=0; i<deck.Length; i++) {
            Debug.Log(deck[i]);
        }

        players = new PlayerController[] {
            player1, player2
        };

        currentPlayerCount = players.Length;
        
        if (Holojam.Tools.BuildManager.IsMasterClient()) {
            // Picking initial cards
            for (int i = 0; i < currentPlayerCount; i++) {
                players[i].CurrentState = deck[nextStateIdx++];
            }
            // Start first player's turn
            StartPlayersTurn(deck[nextStateIdx++]);
        }
        else {
            for (int i = 0; i < currentPlayerCount; i++) {
                if (Holojam.Tools.BuildManager.BUILD_INDEX - 1 == i) {
                    players[i].gameStatusTextObject.gameObject.SetActive(true);
                } else {
                    players[i].gameStatusTextObject.gameObject.SetActive(false);
                }
            }
        }
    }

    void Update () {
        PlayerController currentPlayer = players[currentPlayerIdx];

        if (nextStateIdx > 16) return;


        if (Holojam.Tools.BuildManager.IsMasterClient()) {

            // Space key press for testing purposes
            if (Input.GetKeyDown("space") && nextStateIdx < 16) {
                currentPlayerIdx = (currentPlayerIdx + 1) % players.Length;
                currentPlayer = players[currentPlayerIdx];
                StartPlayersTurn(deck[nextStateIdx++]);
                return;
            }

            // Game over. Calculate winner(s)
            if (nextStateIdx == 16) {
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

                // Set Final Game Status Text
                for (int i = 0; i < players.Length; i++) {
                    if (i == winnerIdx || i == tie1 || i == tie2) {
                        if (tie1 > 0)
                            players[i].SetGameStatus(_GameStatusTie);
                        else
                            players[i].SetGameStatus(_GameStatusWin);
                    } else
                        players[i].SetGameStatus(_GameStatusLose);
                }
                nextStateIdx++;
                return;
            }

            // Current player ended their turn
            if (!currentPlayer.IsDoingTurn && nextStateIdx < 16) {
                Debug.Log(currentPlayer.name + " is ending their turn");
                // Sync up and count player states after the end of each turn
                currentPlayerCount = 0;
                for (int i = 0; i < players.Length; i++) {
                    Debug.Log("*Player " + (i + 1) + " state is " + players[i].CurrentState);
                    if (players[i].CurrentState != State.Dead)
                        currentPlayerCount++;
                }
                
                //for (int i=0; i<players.Length; i++) {
                //    Debug.Log("checking player " + players[i].Number + players[i].CurrentState);
                //}

                Debug.Log("Current Player Count: " + currentPlayerCount);
                Debug.Log("nextStateIdx " + nextStateIdx + " currentPlayerIdx " + currentPlayerIdx);
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
                    StartPlayersTurn(deck[nextStateIdx++]);
                }
            }
        }
        else {
            PlayerController clientPlayer = players[Holojam.Tools.BuildManager.BUILD_INDEX - 1];
            clientPlayer.IsChoosingOwnState = clientPlayer.GetGameStatus().Equals(_ChooseOwnStateText);
            clientPlayer.IsChoosingMenuState = clientPlayer.GetGameStatus().Equals(_ChooseOtherPlayerStateText);
            clientPlayer.IsChoosingOtherPlayer = clientPlayer.GetGameStatus().StartsWith(_ChooseAnotherPlayerText);

            if (currentPlayer == clientPlayer) {
                // currentPlayer is me

                // Show menus based on what the player is currently choosing
                if (currentPlayer.IsChoosingOwnState != twoStateMenuObject.activeSelf) {
                    twoStateMenuObject.SetActive(currentPlayer.IsChoosingOwnState);
                } else if (currentPlayer.IsChoosingMenuState != stateMenuObject.activeSelf) {
                    stateMenuObject.SetActive(currentPlayer.IsChoosingMenuState);
                }
            } else {
                // currentPlayer is not me
                if (twoStateMenuObject.activeSelf != false) twoStateMenuObject.SetActive(false);
                if (stateMenuObject.activeSelf != false) stateMenuObject.SetActive(false);
            }
        }
    }

    void StartPlayersTurn(State next) {
        for (int i = 0; i < players.Length; i++) {
            if (i != currentPlayerIdx) {
                players[i].SetGameStatus("Player " + (currentPlayerIdx + 1) + " is doing their turn");
            }
        }

        PlayerController currentPlayer = players[currentPlayerIdx];
        State previous = currentPlayer.CurrentState;
        currentPlayer.IsDoingTurn = true;
        stateCard1.SetState(previous);
        stateCard2.SetState(next);
        Debug.Log("Starting player " + currentPlayer.Number + "'s turn with " + currentPlayer.CurrentState + next);
        if (currentPlayer.Immune == true) currentPlayer.Immune = false;
        currentPlayer.DismissState = next;
        StartCoroutine(ChooseDismiss());
    }

    #region Player Coroutines
    IEnumerator ChooseDismiss() {
        PlayerController player = players[currentPlayerIdx];
        if (player.DismissState == State.Countess && (player.CurrentState == State.Prince || player.CurrentState == State.King)) {
            // Skip choosing state to dismiss
            player.SetGameStatus("The Countess is dismissed because you have a Prince or King");
        } else {
            yield return StartCoroutine(ChooseStateToDismiss());
        }

        switch (player.DismissState) {
            case State.Guard:
                yield return StartCoroutine(ChooseOtherPlayerState());
                yield return StartCoroutine(GuardAttack());
                break;
            case State.Priest:
                yield return StartCoroutine(PriestReveal());
                break;
            case State.Baron:
                yield return StartCoroutine(BaronBattle());
                break;
            case State.Handmaid:
                player.SetGameStatus("You are immune until your next turn");
                player.Immune = true;
                break;
            case State.Prince:
                yield return StartCoroutine(PrinceForceDiscard());
                break;
            case State.King:
                yield return StartCoroutine(KingTradeHands());
                break;
            case State.Countess:
                player.SetGameStatus("You dismissed the Countess. No powers are enacted");
                break;
            case State.Princess:
                player.Die("You dismissed the Princess and she's very angry. You are out of the round");
                break;
        }
        player.IsDoingTurn = false;
    }

    IEnumerator ChooseStateToDismiss() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseOwnStateText);
        player.chosenStateController = null;
        player.IsChoosingOwnState = true;
        while (player.chosenStateController == null) {
            // wait for player to choose state to dismiss
            yield return null;
        }
        player.IsChoosingOwnState = false;
        if (player.chosenStateController.GetState() == player.CurrentState) // If they want to dismiss their current state
        {
            State temp = player.CurrentState;
            player.CurrentState = player.DismissState;
            player.DismissState = temp;
        }
    }

    IEnumerator ChooseOtherPlayerState() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseOtherPlayerStateText);
        player.chosenStateController = null;
        player.IsChoosingMenuState = true;
        while (player.chosenStateController == null) {
            // wait for player to choose state from menu
            yield return null;
        }
        player.IsChoosingMenuState = false;
    }

    IEnumerator ChooseOtherPlayer() {
        PlayerController player = players[currentPlayerIdx];
        player.chosenOtherPlayer = null;

        // if the only other player is immune, skip
        int validPlayerCount = 0;
        for (int i = 0; i < players.Length; i++) {
            if (i != currentPlayerIdx && !players[i].Immune) {
                validPlayerCount++;
            }
        }
        if (validPlayerCount == 0) {
            player.SetGameStatus("The other player in the game is immune. You cannot enact the power");
        }
        else {
            player.IsChoosingOtherPlayer = true;
            while (player.chosenOtherPlayer == null) {
                //wait for player to choose other player
                yield return null;
            }
            player.IsChoosingOtherPlayer = false;
        }
    }
    #endregion

    #region Character Actions
    IEnumerator GuardAttack() {
        PlayerController player = players[currentPlayerIdx];
        State guess = player.chosenStateController.GetState();

        player.SetGameStatus(_ChooseAnotherPlayerText + "you want to attack");
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            if (player.chosenOtherPlayer.CurrentState == guess) {
                //success
                player.SetGameStatus("You guessed correct! Player " + player.chosenOtherPlayer.Number + " is out");
                player.chosenOtherPlayer.Die("Player " + player.Number + " used a guard and guessed your character correctly. You are out!");
            } else {
                //fail
                player.SetGameStatus("You guessed incorrectly. Player" + player.chosenOtherPlayer.Number + " is still in the game");
            }
        }
    }

    IEnumerator PriestReveal() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseAnotherPlayerText + "to reveal their character");
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            //Reveal other player
            player.SetGameStatus(player.chosenOtherPlayer.name + " has a " + player.chosenOtherPlayer.CurrentState);

        }
    }

    IEnumerator BaronBattle() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseAnotherPlayerText + "you want to battle against");
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            if (player.chosenOtherPlayer.CurrentState < player.CurrentState) {
                //success
                player.SetGameStatus("You beat Player " + player.chosenOtherPlayer.Number + "'s " + player.chosenOtherPlayer.CurrentState + " in battle");
                player.chosenOtherPlayer.Die("Player " + player.Number + " used a Baron and beat your " + player.chosenOtherPlayer.CurrentState + " in battle. You are out!");
            } else if (player.chosenOtherPlayer.CurrentState > player.CurrentState) {
                //fail
                player.Die("Player " + player.chosenOtherPlayer.Number + " has a " + player.chosenOtherPlayer.CurrentState + ". You are out");
            } else {
                //tie
                player.SetGameStatus("Player " + player.chosenOtherPlayer.Number + " has a " + player.chosenOtherPlayer.CurrentState + ". You two tied");
                player.chosenOtherPlayer.SetGameStatus("Player " + player.chosenOtherPlayer.Number + " used a Baron but he also has a " + player.chosenOtherPlayer.CurrentState + ". You two tied");

            }
        }
    }

    IEnumerator PrinceForceDiscard() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseAnotherPlayerText + "you want to force to change characters");
        player.AllowChooseSelf = true;
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            player.AllowChooseSelf = false;
            player.chosenOtherPlayer.CurrentState = deck[nextStateIdx++]; // new state for other player
        }
    }

    IEnumerator KingTradeHands() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseAnotherPlayerText + "you want to switch characters with");
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            State temp = player.CurrentState;
            player.CurrentState = player.chosenOtherPlayer.CurrentState;
            player.chosenOtherPlayer.CurrentState = temp;
            player.SetGameStatus("You received a " + player.CurrentState);
            player.chosenOtherPlayer.SetGameStatus("Player " + player.Number + " switched characters with you.");
        }
    }
    #endregion

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
