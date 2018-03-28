﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using State = GameMaster.State;
[RequireComponent(typeof(Receiver))]
[RequireComponent(typeof(PlayerSync))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

    public TextMesh gameStatusTextObject;
    public int Number;
    private int Index;

    PlayerController chosenOtherPlayer;
    StateController chosenStateController;

    public State CurrentState { get; set; }
    private State DismissState { get; set; }
    private State NextStateInDeck { get; set; }

    public bool IsBroadcasting { get; set; }
    public bool IsEndingTurn { get; set; }
    public bool IsDoingTurn { get; set; }
    public bool IsChoosingOtherPlayer { get; private set; }
    public bool IsChoosingOwnState { get; private set; }
    public bool IsChoosingMenuState { get; private set; }
    public bool Immune { get; set; }
    public bool UsedNextState { get; set; }

    public State[] BroadcastStates { get; set; }


    private bool AllowChooseSelf { get; set; }

    void Start() {
        BroadcastStates = new State[4];
        Index = Number - 1;
    }

    void Update() {

    }

    #region General Player Methods
    public void SetGameStatus(string s) {
        gameStatusTextObject.text = s;
    }

    public void StartTurn(State next, State nextnext) //nextnext is needed for PrinceForceDiscard()
    {
        if (Immune == true) Immune = false;
        DismissState = next;
        NextStateInDeck = nextnext;
        StartCoroutine(ChooseDismiss());
    }

    IEnumerator ChooseDismiss() {
        if (DismissState == State.Countess && (CurrentState == State.Prince || CurrentState == State.King)) {
            // Skip choosing state to dismiss
            gameStatusTextObject.text = "The Countess is dismissed because you have a Prince or King";
        } else {
            gameStatusTextObject.text = "Choose a card you would like to discard and enact its power";
            yield return StartCoroutine(ChooseStateToDismiss());
        }

        switch (DismissState) {
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
                gameStatusTextObject.text = "You are immune until your next turn";
                Immune = true;
                break;
            case State.Prince:
                yield return StartCoroutine(PrinceForceDiscard());
                break;
            case State.King:
                yield return StartCoroutine(KingTradeHands());
                break;
            case State.Countess:
                gameStatusTextObject.text = "You dismissed the Countess. No powers are enacted";
                break;
            case State.Princess:
                Die();
                break;
        }
        IsDoingTurn = false;
        IsBroadcasting = true;
        Debug.Log("end of "+ this.name + "'s turn in PlayerController. Begin broadcasting.");
    }

    IEnumerator ChooseStateToDismiss() {
        chosenStateController = null;
        IsChoosingOwnState = true;
        while (chosenStateController == null) {
            // wait for player to choose state to dismiss
            yield return null;
        }
        IsChoosingOwnState = false;
        if (chosenStateController.GetState() == CurrentState) // If they want to dismiss their current state
        {
            State temp = CurrentState;
            CurrentState = DismissState;
            DismissState = temp;
        }
    }

    IEnumerator ChooseOtherPlayerState() {
        gameStatusTextObject.text = "Choose a state you believe another player has";
        chosenStateController = null;
        IsChoosingMenuState = true;
        while (chosenStateController == null) {
            // wait for player to choose state from menu
            yield return null;
        }
        IsChoosingMenuState = false;
    }

    IEnumerator ChooseOtherPlayer() {
        chosenOtherPlayer = null;
        IsChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null) {
            //wait for player to choose other player
            yield return null;
        }
        IsChoosingOtherPlayer = false;
    }

    void Die() {
        gameStatusTextObject.text = "You died! Game over. You lost.";
        CurrentState = State.Dead;
    }

    void SetBroadcastState(PlayerController player, State s) {
        BroadcastStates[player.Index] = s;
    }
    #endregion

    #region Dismissal Actions

    IEnumerator GuardAttack() {
        State guess = chosenStateController.GetState();

        gameStatusTextObject.text = "Choose another player you want to attack";
        yield return StartCoroutine(ChooseOtherPlayer());
        if (chosenOtherPlayer.CurrentState == guess)
        {
            //success
            gameStatusTextObject.text = "You guessed correct! Player " + chosenOtherPlayer.Number + " is dead.";
            SetBroadcastState(chosenOtherPlayer, State.Dead);
        }
        else
        {
            //fail
            gameStatusTextObject.text = "You guessed incorrectly. Player" + chosenOtherPlayer.Number + " is still in the game.";
        } 
    }

    IEnumerator PriestReveal()
    {
        gameStatusTextObject.text = "Choose another player to reveal their character";
        yield return StartCoroutine(ChooseOtherPlayer());
        //Reveal other player
        gameStatusTextObject.text = chosenOtherPlayer.name + " has a " + chosenOtherPlayer.CurrentState;
    }

    IEnumerator BaronBattle()
    {
        gameStatusTextObject.text = "Choose another player you want to battle against";
        yield return StartCoroutine(ChooseOtherPlayer());
        if (chosenOtherPlayer.CurrentState < CurrentState)
        {
            //success
            gameStatusTextObject.text = "You beat " + chosenOtherPlayer.name + "'s " + chosenOtherPlayer.CurrentState + " in battle";
            SetBroadcastState(chosenOtherPlayer, State.Dead);
        }
        else if (chosenOtherPlayer.CurrentState > CurrentState)
        {
            //fail
            gameStatusTextObject.text = chosenOtherPlayer.name + " has a " + chosenOtherPlayer.CurrentState + ". You died in battle";
            Die();
        }
        else
        {
            //tie
            gameStatusTextObject.text = chosenOtherPlayer.name + " has a " + chosenOtherPlayer.CurrentState + ". You two tied";
        }
    }

    IEnumerator PrinceForceDiscard()
    {
        gameStatusTextObject.text = "Choose another player you want to force to change characters";
        AllowChooseSelf = true;
        yield return StartCoroutine(ChooseOtherPlayer());
        AllowChooseSelf = false;
        SetBroadcastState(chosenOtherPlayer, NextStateInDeck); // new state for other player
        UsedNextState = true; // used to let Game Master know you used the next card
    }

    IEnumerator KingTradeHands()
    {
        gameStatusTextObject.text = "Choose another player you want to switch characters with";
        yield return StartCoroutine(ChooseOtherPlayer());
        SetBroadcastState(chosenOtherPlayer, CurrentState);
        CurrentState = chosenOtherPlayer.CurrentState;
    }
    #endregion

    #region Vive Controller Methods
    public void OnGlobalTriggerPressDown(XREventData eventData)
    {
        if (eventData.currentRaycast == null)
            return;

        if (IsChoosingOtherPlayer) {
            PlayerController otherPlayerController = eventData.currentRaycast.GetComponent<PlayerController> ();
            Debug.Log ("Pointing at " + otherPlayerController);
            if (otherPlayerController != null && !otherPlayerController.Immune) {
                if (AllowChooseSelf || otherPlayerController != this ) {
                    chosenOtherPlayer = otherPlayerController;
                }
            }
        } else if (IsChoosingOwnState || IsChoosingMenuState) {
            
            StateController otherStateController = eventData.currentRaycast.GetComponent<StateController> ();
            Debug.Log ("Pointing at state: " + otherStateController);
            if (otherStateController != null) {
                chosenStateController = otherStateController;

            }
        }
    }
    #endregion
}