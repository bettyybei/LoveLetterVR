using System.Collections;
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

    public PlayerController[] players;
    int currentPlayerCount;

    const string _GameStatusWin = "Game over. You win!";
    const string _GameStatusLose = "Game over. You lost.";
    const string _GameStatusTie = "Game over. It was a tie!";
    const string _ChooseOwnStateText = "Choose a card you would like to discard and enact its power";
    const string _ChooseOtherPlayerStateText = "Choose a card you think another player is holding";
    const string _ChooseAnotherPlayerText = "Choose another player ";

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
        // Populate State Controllers
        StateController[] menuStateControllers = stateMenuObject.GetComponentsInChildren<StateController>();
        for (int i = 0; i < menuStateControllers.Length; i++) {
            menuStateControllers[i].SetState((State)(i + 2));
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

            // Show menus based on what the player is currently choosing
            if (currentPlayer.IsChoosingOwnState != twoStateMenuObject.activeSelf) {
                twoStateMenuObject.SetActive(currentPlayer.IsChoosingOwnState);
            } else if (currentPlayer.IsChoosingMenuState != stateMenuObject.activeSelf) {
                stateMenuObject.SetActive(currentPlayer.IsChoosingMenuState);
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
                // Sync up and count player states after the end of each turn
                currentPlayerCount = 0;
                for (int i = 0; i < players.Length; i++) {
                    if (players[i].CurrentState != State.Dead)
                        currentPlayerCount++;
                }
                
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
                players[i].SetGameStatus(players[currentPlayerIdx].GetName() + " is doing their turn");
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

    void PlayerDie(PlayerController player, string gameStatus) {
        player.Die(gameStatus);
        foreach (PlayerController p in players) {
            if (p != player) {
                p.SetGameStatus(player.GetName() + " is out of this round!");
            }
        }
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
                yield return StartCoroutine(ChooseMenuState());
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
                foreach(PlayerController p in players) {
                    if (p != player && p.CurrentState != State.Dead) {
                        p.SetGameStatus(player.GetName() + " is immune until their next turn");
                    }
                }
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
                foreach (PlayerController p in players) {
                    if (p != player && p.CurrentState != State.Dead) {
                        p.SetGameStatus(player.GetName() + " dismissed the Countess");
                    }
                }
                break;
            case State.Princess:
                PlayerDie(player, "You dismissed the Princess and she's very angry. You are out of the round");
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

    IEnumerator ChooseMenuState() {
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

        player.SetGameStatus(_ChooseAnotherPlayerText + "you believe has a " + guess);
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            if (player.chosenOtherPlayer.CurrentState == guess) {
                //success
                player.SetGameStatus("You guessed correct! " + player.chosenOtherPlayer.GetName() + " is out");
                PlayerDie(player.chosenOtherPlayer, player.GetName() + " used a guard on you and guessed correctly. You are out!");
            } else {
                //fail
                player.SetGameStatus("You guessed incorrectly. " + player.chosenOtherPlayer.GetName() + " is still in the game");
                player.chosenOtherPlayer.SetGameStatus(player.GetName() + " used a guard and knows you do not have a " + guess);
            }
        }
    }

    IEnumerator PriestReveal() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseAnotherPlayerText + "to reveal their messenger");
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            //Reveal other player
            player.SetGameStatus(player.chosenOtherPlayer.GetName() + " has a " + player.chosenOtherPlayer.CurrentState);
            player.chosenOtherPlayer.SetGameStatus(player.GetName() + "'s priest found out you have a " + player.chosenOtherPlayer.CurrentState + ". Watch out!");
        }
    }

    IEnumerator BaronBattle() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseAnotherPlayerText + "you want to battle against");
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            if (player.chosenOtherPlayer.CurrentState < player.CurrentState) {
                //success
                PlayerDie(player.chosenOtherPlayer, player.GetName() + " used a Baron and beat your " + player.chosenOtherPlayer.CurrentState + " in battle. You are out!");
                player.SetGameStatus("You beat " + player.chosenOtherPlayer.GetName() + "'s " + player.chosenOtherPlayer.CurrentState + " in battle");
            } else if (player.chosenOtherPlayer.CurrentState > player.CurrentState) {
                //fail
                PlayerDie(player, player.chosenOtherPlayer.GetName() + " has a " + player.chosenOtherPlayer.CurrentState + ". You are out");
            } else {
                //tie
                player.SetGameStatus(player.chosenOtherPlayer.GetName() + " has a " + player.chosenOtherPlayer.CurrentState + ". You two tied");
                player.chosenOtherPlayer.SetGameStatus(player.GetName() + " used a Baron but he also has a " + player.chosenOtherPlayer.CurrentState + ". You two tied");

            }
        }
    }

    IEnumerator PrinceForceDiscard() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseAnotherPlayerText + "you want to force to change messengers");
        player.AllowChooseSelf = true;
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            player.AllowChooseSelf = false;
            State newState = deck[nextStateIdx++];
            player.chosenOtherPlayer.CurrentState = newState; // new state for other player
            player.chosenOtherPlayer.SetGameStatus(player.GetName() + "'s Prince forced you to get a new messenger. You received a " + newState);
        }
    }

    IEnumerator KingTradeHands() {
        PlayerController player = players[currentPlayerIdx];
        player.SetGameStatus(_ChooseAnotherPlayerText + "you want to switch messengers with");
        yield return StartCoroutine(ChooseOtherPlayer());
        if (player.chosenOtherPlayer != null) {
            State temp = player.CurrentState;
            player.CurrentState = player.chosenOtherPlayer.CurrentState;
            player.chosenOtherPlayer.CurrentState = temp;
            player.SetGameStatus("You received a " + player.CurrentState);
            player.chosenOtherPlayer.SetGameStatus(player.GetName() + " used a King and switched messengers with you. You now have a " + player.chosenOtherPlayer.CurrentState);
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
