using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

    public enum State { Dead, Guard, Priest, Baron, Handmaid, Prince, King, Countess, Princess }

	public GameObject pointerObject;
	private Renderer r;
	public TextMesh textObject;
	public bool isDoingTurn = false;

	bool isChoosingOtherPlayer = false;
	bool immune = false;
    PlayerController chosenOtherPlayer;

    State current;
    State dismiss;

	// Use this for initialization
	void Start () {
		r = pointerObject.GetComponent<Renderer> ();
	}

	// Update is called once per frame
	void Update () {
		//Renderer r = pointerObject.GetComponent<Renderer> ();
		if (r.enabled != isChoosingOtherPlayer)
			Debug.Log (this + " toggle pointer to " + isChoosingOtherPlayer);
			r.enabled = isChoosingOtherPlayer;
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
        if (next == State.Countess && (current == State.Prince || current == State.King) )
        {
            Dismiss();
        }
        else
        {
            ChooseStateToDismiss();
            Dismiss();
        }
    }

    void Dismiss()
    {
        switch (dismiss)
        {
			case State.Guard:
				State guess = ChooseOtherPlayerState ();
				StartCoroutine(GuardAttack(guess));
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

    void ChooseStateToDismiss()
    {
        if (true) //TODO
        {
			return;
        }
        State temp = current;
		SetState(dismiss);
        dismiss = temp;
    }

    State ChooseOtherPlayerState()
    {
        return State.Guard;
    }

    void Die()
    {
        current = State.Dead;
    }
    #endregion


    #region Dismissal Actions

	IEnumerator GuardAttack(State guess) {
		chosenOtherPlayer = null;
		isChoosingOtherPlayer = true;
		Debug.Log ("guard attack");
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
			Debug.Log ("Die reached");
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

    #region Vive Controller Methods
    public void OnGlobalTriggerPressDown(VREventData eventData)
    {
		if (eventData.currentRaycast == null)
			return;
		
        PlayerController otherPlayerController = eventData.currentRaycast.GetComponent<PlayerController>();
		Debug.Log ("Pointing at " + otherPlayerController);
		if (isChoosingOtherPlayer == true && otherPlayerController != null && otherPlayerController != this && !otherPlayerController.immune)
        {
            chosenOtherPlayer = otherPlayerController;
        }
    }
    #endregion
}