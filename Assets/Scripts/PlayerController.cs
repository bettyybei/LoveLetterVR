using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using State = GameMaster.State;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

	public GameObject pointerObject;
	private Renderer pointerRenderer;
	public TextMesh textObject;
	bool isDoingTurn = false;

	bool isChoosingOtherPlayer = false;
	bool isChoosingOwnState = false;
	bool isChoosingMenuState = false;

	bool immune = false;
    PlayerController chosenOtherPlayer;
	StateController chosenStateController;

    public State current;
    State dismiss;

	// Use this for initialization
	void Start () {
		pointerRenderer = pointerObject.GetComponent<Renderer> ();
	}

	// Update is called once per frame
	void Update () {
		//Renderer r = pointerObject.GetComponent<Renderer> ();
		bool pointerEnabled = isChoosingOtherPlayer || isChoosingOwnState || isChoosingMenuState;
		if (pointerRenderer.enabled != pointerEnabled)
			Debug.Log (this + " toggle pointer to " + pointerEnabled);
			pointerRenderer.enabled = pointerEnabled;
	}

    #region General Player Methods
	public void SetState(State s) {
		current = s;
		textObject.text = s.ToString();
	}

    public void StartTurn(State next)
    {
		Debug.Log (this + " turn");
		if (immune == true) immune = false;
		dismiss = next;
		StartCoroutine (Dismiss ());
    }

    IEnumerator Dismiss()
    {
		if (dismiss == State.Countess && (current == State.Prince || current == State.King) )
		{
			// Skip choosing state to dismiss
		}
		else
		{
			yield return StartCoroutine(ChooseStateToDismiss());
		}

        switch (dismiss)
        {
			case State.Guard:
				yield return StartCoroutine (ChooseOtherPlayerState ());
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
		if (chosenStateController.GetState() == current) // If they want to dismiss their current/prev state
        {
			State temp = current;
			SetState(dismiss);
			dismiss = temp;
        }
    }

    IEnumerator ChooseOtherPlayerState()
    {
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
		// Choose state first
		/*chosenStateController = null;
		isChoosingMenuState = true;
		while (chosenStateController == null) {
			// wait for player to choose state from menu
			yield return null;
		}
		isChoosingMenuState = false;
		Debug.Log ("end coroutine choose other player state");
		*/

		State guess = chosenStateController.GetState();

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
			chosenOtherPlayer.Die();
			Debug.Log ("Other player died");
		}
		else
		{
			//fail
		}
		isDoingTurn = false;
		Debug.Log ("end of function call");
	}

    IEnumerator PriestReveal()
    {
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
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        isChoosingOtherPlayer = false;
        if (chosenOtherPlayer.current < current)
        {
            //success
            chosenOtherPlayer.Die();
        }
        else if (chosenOtherPlayer.current > current)
        {
            //fail
            Die();
        }
        else
        {
            //tie
		}
		isDoingTurn = false;
    }

	IEnumerator PrinceForceDiscard()
    {
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        isChoosingOtherPlayer = false;
		// new state for other player
		isDoingTurn = false;
    }

	IEnumerator KingTradeHands()
    {
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
			yield return null;
        }
        isChoosingOtherPlayer = false;
        State temp = current;
        current = chosenOtherPlayer.current;
		chosenOtherPlayer.current = temp;
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
			chosenStateController = eventData.currentRaycast.GetComponent<StateController> ();
		}
    }
    #endregion
}