using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

    public enum State { Dead, Guard, Priest, Baron, Handmaid, Prince, King, Countess, Princess }

    bool immune = false;
    PlayerController chosenOtherPlayer;

    State current;
    State dismiss;

    bool isChoosingOtherPlayer = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region General Player Methods
    void StartTurn(State next)
    {
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

    State ChooseStateToDismiss()
    {
        if (true) //TODO
        {
            return current;
        }
        State temp = current;
        current = dismiss;
        dismiss = temp;
    }

    State ChooseOtherPlayerState()
    {
        return State.Dead;
    }

    void Die()
    {
        current = State.Dead;
    }
    #endregion


    #region Dismissal Actions
    void GuardAttack(State guess)
    {
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer != null)
        {
            //wait for player to choose other player
        }
        isChoosingOtherPlayer = false;
        if (chosenOtherPlayer.current == guess)
        {
            //success
            chosenOtherPlayer.Die();
        }
        else
        {
            //fail
        }
    }

    void PriestReveal()
    {
        chosenOtherPlayer = null;
        isChoosingOtherPlayer = true;
        while (chosenOtherPlayer != null)
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
        while (chosenOtherPlayer != null)
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
        while (chosenOtherPlayer != null)
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
        while (chosenOtherPlayer != null)
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
    void IGlobalTriggerPressDownHandler.OnGlobalTriggerPressDown(VREventData eventData)
    {
        PlayerController otherPlayerController = eventData.currentRaycast.GetComponent<PlayerController>();
        if (isChoosingOtherPlayer == true && otherPlayerController != null)
        {
            chosenOtherPlayer = otherPlayerController;
            //We're pointing at a PlayerController! Do something.
        } else
        {
            //We're not pointing at a PlayerController.
        }
    }
    #endregion
}
