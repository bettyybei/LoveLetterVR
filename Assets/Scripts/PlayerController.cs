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
	bool isDoingTurn = false;

	bool isChoosingOtherPlayer = false;
	bool isChoosingOwnState = false;
	bool isChoosingMenuState = false;
	bool allowChooseSelf = false;

	bool immune = false;
    PlayerController chosenOtherPlayer;
	StateController chosenStateController;


	State current;
    State dismiss;

	public State CurrentState { get { return current; } }

	bool usedNextState = false;
	State nextState;

	const string _GameStatusWin = "Game over. You win!";
	const string _GameStatusLose = "Game over. You lost.";
	const string _GameStatusTie = "Game over. It was a tie!";

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		bool pointerEnabled = isChoosingOtherPlayer || isChoosingOwnState || isChoosingMenuState;
        if (pointerEnabled != pointerObject.activeSelf)
        {
            Debug.Log(this + " toggle pointer to " + pointerEnabled);
            pointerObject.SetActive(pointerEnabled);
        }
	}

    #region General Player Methods
	public void SetState(State s) {
		current = s;
		stateTextObject.text = "Currently Holding: " + s.ToString();
		Debug.Log (this + " state changed to " + s);
	}

	public void StartTurn(State next, State nextnext) //nextnext is needed for PrinceForceDiscard()
    {
		Debug.Log (this + " starts their turn");
		if (immune == true) immune = false;
		dismiss = next;
		nextState = nextnext;
		StartCoroutine(Dismiss());
    }

    IEnumerator Dismiss()
    {
		if (dismiss == State.Countess && (current == State.Prince || current == State.King) )
		{
			// Skip choosing state to dismiss
			gameStatusTextObject.text = "The Countess is dismissed because you have a Prince or King";
		}
		else
		{
			gameStatusTextObject.text = "Choose a card you would like to discard and enact its power";
			yield return StartCoroutine(ChooseStateToDismiss());
		}
			
        switch (dismiss)
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
                immune = true;
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
		isDoingTurn = false;
    }

    IEnumerator ChooseStateToDismiss()
    {
		chosenStateController = null;
		isChoosingOwnState = true;
		while (chosenStateController == null) {
			// wait for player to choose state to dismiss
			yield return null;
		}
		isChoosingOwnState = false;
		if (chosenStateController.GetState() == current) // If they want to dismiss their current state
        {
			State temp = current;
			SetState(dismiss);
			dismiss = temp;
        }
    }

    IEnumerator ChooseOtherPlayerState()
    {
		gameStatusTextObject.text = "Choose a state you believe another player has";
		chosenStateController = null;
		isChoosingMenuState = true;
		while (chosenStateController == null) {
			// wait for player to choose state from menu
			yield return null;
		}
		isChoosingMenuState = false;
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
		isChoosingOtherPlayer = true;
		while (chosenOtherPlayer == null)
		{
			//wait for player to choose other player
			yield return null;
		}
		isChoosingOtherPlayer = false;
		if (chosenOtherPlayer.current == guess)
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
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        isChoosingOtherPlayer = false;
		//Reveal other player
		gameStatusTextObject.text = chosenOtherPlayer.name + " has a " + chosenOtherPlayer.CurrentState;
    }

	IEnumerator BaronBattle()
    {
		gameStatusTextObject.text = "Choose another player you want to battle against";
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        isChoosingOtherPlayer = false;
		if (chosenOtherPlayer.CurrentState < current)
        {
            //success
			gameStatusTextObject.text = "You beat " + chosenOtherPlayer.name + "'s " + chosenOtherPlayer.CurrentState + " in battle";
            chosenOtherPlayer.Die();
        }
		else if (chosenOtherPlayer.CurrentState > current)
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
        isChoosingOtherPlayer = true;
		allowChooseSelf = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
		isChoosingOtherPlayer = false;
		allowChooseSelf = false;
		chosenOtherPlayer.SetState (nextState); // new state for other player
		this.usedNextState = true; // let Game Master know you used the next card
    }

	IEnumerator KingTradeHands()
    {
		gameStatusTextObject.text = "Choose another player you want to switch characters with";
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        isChoosingOtherPlayer = false;
        State temp = current;
		this.SetState(chosenOtherPlayer.CurrentState);
		chosenOtherPlayer.SetState(temp);
    }
    #endregion

	#region Public Getters and Setters

	public bool GetIsDoingTurn() {
		return this.isDoingTurn;
	}
	public void SetIsDoingTurn(bool b) {
		this.isDoingTurn = b;
	}
	public bool GetIsChoosingOwnState() {
		return this.isChoosingOwnState;
	}
	public bool GetIsChoosingMenuState() {
		return this.isChoosingMenuState;
	}
	public bool GetUsedNextState() {
		return this.usedNextState;
	}
	public void SetUsedNextState(bool b) {
		this.usedNextState = b;
	}

	#endregion

    #region Vive Controller Methods
    public void OnGlobalTriggerPressDown(VREventData eventData)
    {
		if (eventData.currentRaycast == null)
			return;

		if (isChoosingOtherPlayer) {
			PlayerController otherPlayerController = eventData.currentRaycast.GetComponent<PlayerController> ();
			Debug.Log ("Pointing at " + otherPlayerController);
			if (otherPlayerController != null && !otherPlayerController.immune) {
				if (allowChooseSelf || otherPlayerController != this ) {
					chosenOtherPlayer = otherPlayerController;
				}
			}
		} else if (isChoosingOwnState || isChoosingMenuState) {
			
			StateController otherStateController = eventData.currentRaycast.GetComponent<StateController> ();
			Debug.Log ("Pointing at state: " + otherStateController);
			if (otherStateController != null) {
				chosenStateController = otherStateController;

			}
		}
    }
    #endregion
}