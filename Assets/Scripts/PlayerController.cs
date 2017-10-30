using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

    enum State { Dead, Guard, Priest, Baron, Handmaid, Prince, King, Countess, Princess }
    bool immune = false;

    State current;
    State dismiss;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

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

    void Dismiss()
    {
        switch (dismiss)
        {
            case State.Guard:
                State state = ChooseOtherPlayerState();
                ChooseOtherPlayer();
                break;
            case State.Priest:
                ChooseOtherPlayer();
                //Reveal Other Player();
                break;
            case State.Baron:
                break;
            case State.Handmaid:
                immune = true;
                break;
            case State.Prince:
                ChooseOtherPlayer();
                break;
            case State.King:
                ChooseOtherPlayer();
                break;
            case State.Countess:
                break;
            case State.Princess:
                break;
        }
    }

    void ChooseOtherPlayer()
    {

    }

    State ChooseOtherPlayerState()
    {
        return State.Dead;
    }

    void IGlobalTriggerPressDownHandler.OnGlobalTriggerPressDown(VREventData eventData)
    {
        PlayerController opc = eventData.currentRaycast.GetComponent<PlayerController>();
        if (opc != null)
        {
            //We're pointing at a PlayerController! Do something.
        } else
        {
            //We're not pointing at a PlayerController.
        }
    }
}
