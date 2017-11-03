using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

    public enum State { Dead, Guard, Priest, Baron, Handmaid, Prince, King, Countess, Princess }

	//TextMesh textObject;
	public bool isDoingTurn = false;

	bool isChoosingOtherPlayer = false;
    bool immune = false;
    PlayerController chosenOtherPlayer;

    State current;
    State dismiss;



	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region General Player Methods
	public void SetState(State s) {
		current = s;
		TextMesh textObject = GameObject.Find("StateText").GetComponent<TextMesh>();
		textObject.text = s.ToString();
	}

    public void StartTurn(State next)
    {
		Debug.Log ("start turn");
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
		isDoingTurn = false;
    }

    void Dismiss()
    {
        switch (dismiss)
        {
            case State.Guard:
                State guess = ChooseOtherPlayerState();
                GuardAttack(guess);
                break;
            case State.Priest:
                PriestReveal();
                break;
            case State.Baron:
                BaronBattle();
                break;
            case State.Handmaid:
                immune = true;
                break;
            case State.Prince:
                PrinceForceDiscard();
                break;
            case State.King:
                KingTradeHands();
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
			return;;
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
    void GuardAttack(State guess)
    {
		Debug.Log ("guard attaack");
		StartCoroutine (ChooseOtherPlayer (guess));
    }

	IEnumerator ChooseOtherPlayer(State guess) {
		chosenOtherPlayer = null;
		isChoosingOtherPlayer = true;
		while (chosenOtherPlayer == null)
		{
			Debug.Log (gameObject.name + "; loop; " + (chosenOtherPlayer == null));
			//wait for player to choose other player
			yield return new WaitForSeconds(1);
		}
		isChoosingOtherPlayer = false;
		if (chosenOtherPlayer.current == guess)
		{
			//success
			Debug.Log ("success");
			chosenOtherPlayer.Die();
		}
		else
		{
			//fail
		}
		Debug.Log ("silent failure");
	}

    void PriestReveal()
    {
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
        }
        isChoosingOtherPlayer = false;
        //Reveal other player
    }

    void BaronBattle()
    {
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
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
    }

    void PrinceForceDiscard()
    {
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
        }
        isChoosingOtherPlayer = false;
        // new state for other player
    }

    void KingTradeHands()
    {
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer == null)
        {
            //wait for player to choose other player
        }
        isChoosingOtherPlayer = false;
        State temp = current;
        current = chosenOtherPlayer.current;
        chosenOtherPlayer.current = temp;
    }
    #endregion

    #region Vive Controller Methods
    public void OnGlobalTriggerPressDown(VREventData eventData)
    {
		Debug.Log ("here");
        PlayerController otherPlayerController = eventData.currentRaycast.GetComponent<PlayerController>();
        if (isChoosingOtherPlayer == true && otherPlayerController != null)
        {
            chosenOtherPlayer = otherPlayerController;
        } else
        {
            //We're not pointing at a PlayerController.
        }
    }
    #endregion
}
