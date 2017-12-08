using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using State = GameMaster.State;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

	public GameObject pointerObject;
	public TextMesh stateTextObject;
	public TextMesh gameStatusTextObject;

    PlayerController chosenOtherPlayer;
	StateController chosenStateController;

	public State CurrentState { get; private set; }
	private State DismissState { get; set; }
	private State NextStateInDeck { get; set; }

	public bool IsDoingTurn { get; set; }
	public bool IsChoosingOwnState { get; private set; }
	public bool IsChoosingMenuState { get; private set; }
	public bool UsedNextState { get; set; }

	private bool IsChoosingOtherPlayer { get; set; }
	private bool AllowChooseSelf { get; set; }
	private bool Immune { get; set; }

	const string _GameStatusWin = "Game over. You win!";
	const string _GameStatusLose = "Game over. You lost.";
	const string _GameStatusTie = "Game over. It was a tie!";

	void Update () {
		bool pointerEnabled = IsChoosingOtherPlayer || IsChoosingOwnState || IsChoosingMenuState;
        if (pointerEnabled != pointerObject.activeSelf)
        {
            //Debug.Log(this + " toggle pointer to " + pointerEnabled);
            pointerObject.SetActive(pointerEnabled);
        }
	}

    #region General Player Methods
	public void SetState(State s) {
		CurrentState = s;
		stateTextObject.text = "Currently Holding: " + s.ToString();
		Debug.Log (this + " state changed to " + s);
	}

	public void StartTurn(State next, State nextnext) //nextnext is needed for PrinceForceDiscard()
    {
		Debug.Log (this + " starts their turn");
		if (Immune == true) Immune = false;
		DismissState = next;
		NextStateInDeck = nextnext;
		StartCoroutine(ChooseDismiss());
    }

    IEnumerator ChooseDismiss()
    {
		if (DismissState == State.Countess && (CurrentState == State.Prince || CurrentState == State.King) )
		{
			// Skip choosing state to dismiss
			gameStatusTextObject.text = "The Countess is dismissed because you have a Prince or King";
		}
		else
		{
			gameStatusTextObject.text = "Choose a card you would like to discard and enact its power";
			yield return StartCoroutine(ChooseStateToDismiss());
		}
			
        switch (DismissState)
        {
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
		Debug.Log ("end of turn reached");
		IsDoingTurn = false;
    }

    IEnumerator ChooseStateToDismiss()
    {
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
			SetState(DismissState);
			DismissState = temp;
        }
    }

    IEnumerator ChooseOtherPlayerState()
    {
		gameStatusTextObject.text = "Choose a state you believe another player has";
		chosenStateController = null;
		IsChoosingMenuState = true;
		while (chosenStateController == null) {
			// wait for player to choose state from menu
			yield return null;
		}
		IsChoosingMenuState = false;
    }

    void Die()
    {
		gameStatusTextObject.text = "You died! " + _GameStatusLose;
		SetState (State.Dead);
    }
    #endregion


    #region Dismissal Actions

	IEnumerator GuardAttack() {
		State guess = chosenStateController.GetState();

		gameStatusTextObject.text = "Choose another player you want to attack";
		chosenOtherPlayer = null;
		IsChoosingOtherPlayer = true;
		while (chosenOtherPlayer == null)
		{
			//wait for player to choose other player
			yield return null;
		}
		IsChoosingOtherPlayer = false;
		if (chosenOtherPlayer.CurrentState == guess)
		{
			//success
			gameStatusTextObject.text = "You guessed correct! " + chosenOtherPlayer.name + " is dead.";
			chosenOtherPlayer.Die();
		}
		else
		{
			//fail
			gameStatusTextObject.text = "You guessed incorrectly. " + chosenOtherPlayer.name + " is still in the game.";
		} 
	}

    IEnumerator PriestReveal()
    {
		gameStatusTextObject.text = "Choose another player to reveal their character";
        chosenOtherPlayer = null;
        IsChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        IsChoosingOtherPlayer = false;
		//Reveal other player
		gameStatusTextObject.text = chosenOtherPlayer.name + " has a " + chosenOtherPlayer.CurrentState;
    }

	IEnumerator BaronBattle()
    {
		gameStatusTextObject.text = "Choose another player you want to battle against";
        chosenOtherPlayer = null;
        IsChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        IsChoosingOtherPlayer = false;
		if (chosenOtherPlayer.CurrentState < CurrentState)
        {
            //success
			gameStatusTextObject.text = "You beat " + chosenOtherPlayer.name + "'s " + chosenOtherPlayer.CurrentState + " in battle";
            chosenOtherPlayer.Die();
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
        chosenOtherPlayer = null;
        IsChoosingOtherPlayer = true;
		AllowChooseSelf = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
		IsChoosingOtherPlayer = false;
		AllowChooseSelf = false;
		chosenOtherPlayer.SetState (NextStateInDeck); // new state for other player
		UsedNextState = true; // used to let Game Master know you used the next card
    }

	IEnumerator KingTradeHands()
    {
		gameStatusTextObject.text = "Choose another player you want to switch characters with";
        chosenOtherPlayer = null;
        IsChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        IsChoosingOtherPlayer = false;
        State temp = CurrentState;
		this.SetState(chosenOtherPlayer.CurrentState);
		chosenOtherPlayer.SetState(temp);
    }
    #endregion

    #region Vive Controller Methods
    public void OnGlobalTriggerPressDown(VREventData eventData)
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