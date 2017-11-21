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

	bool immune = false;
    PlayerController chosenOtherPlayer;
	StateController chosenStateController;

    State current;
    State dismiss;

	bool usedNextState = false;
	State nextState;

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
		stateTextObject.text = s.ToString();
		Debug.Log ("State Changed to " + s);
	}

	public void StartTurn(State next, State nextnext) //nextnext is needed for PrinceForceDiscard()
    {
		Debug.Log (this + " turn");
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
				StartCoroutine(GuardAttack());
                break;
			case State.Priest:
				StartCoroutine(PriestReveal());
                break;
            case State.Baron:
				StartCoroutine(BaronBattle());
                break;
            case State.Handmaid:
                immune = true;
                break;
			case State.Prince:
				StartCoroutine(PrinceForceDiscard());
                break;
            case State.King:
				StartCoroutine(KingTradeHands());
                break;
            case State.Countess:
                break;
            case State.Princess:
                Die();
                break;
        }
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
        current = State.Dead;
    }
    #endregion


    #region Dismissal Actions

	IEnumerator GuardAttack() {
		State guess = chosenStateController.GetState();

		gameStatusTextObject.text = "Choose another player you want to attack";
		chosenOtherPlayer = null;
		isChoosingOtherPlayer = true;
		Debug.Log ("guard attack with guess: " + guess);
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
		isDoingTurn = false;    
		Debug.Log ("end of function call");
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
		isDoingTurn = false;
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
		if (chosenOtherPlayer.GetCurrentState() < current)
        {
            //success
			gameStatusTextObject.text = "You beat " + chosenOtherPlayer + "'s " + chosenOtherPlayer.GetCurrentState() + " in battle";
            chosenOtherPlayer.Die();
        }
		else if (chosenOtherPlayer.GetCurrentState() > current)
        {
            //fail
			gameStatusTextObject.text = chosenOtherPlayer + " has a " + chosenOtherPlayer.GetCurrentState() + ". You died in battle";
            Die();
        }
        else
        {
            //tie
			gameStatusTextObject.text = chosenOtherPlayer + " has a " + chosenOtherPlayer.GetCurrentState() + ". You guys are tied";
		}
		isDoingTurn = false;
    }

	IEnumerator PrinceForceDiscard()
    {
		gameStatusTextObject.text = "Choose another player you want to force to change characters";
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
		isChoosingOtherPlayer = false;
		chosenOtherPlayer.SetState (nextState); // new state for other player
		this.usedNextState = true; // let Game Master know you used the next card
		isDoingTurn = false;
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
		current = chosenOtherPlayer.GetCurrentState ();
		chosenOtherPlayer.SetState(temp);
		isDoingTurn = false;
    }
    #endregion

	#region Public Getters and Setters
	public State GetCurrentState() {
		return this.current;
	}
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
			if (otherPlayerController != null && otherPlayerController != this && !otherPlayerController.immune) {
				chosenOtherPlayer = otherPlayerController;
			}
		} else if (isChoosingOwnState || isChoosingMenuState) {
			
			StateController otherStateController = eventData.currentRaycast.GetComponent<StateController> ();
			if (otherStateController != null) {
				chosenStateController = otherStateController;
				Debug.Log ("Pointing at state: " + chosenStateController.GetState ());
			}
		}
    }
    #endregion
}