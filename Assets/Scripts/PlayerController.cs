using UnityEngine;
using FRL.IO;
using State = GameMaster.State;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler {

    public TextMesh gameStatusTextObject;
    public int Number;
    private int Index;

    public PlayerController chosenOtherPlayer { get; set; }
    public StateController chosenStateController { get; set; }

    public State CurrentState { get; set; }
    public State DismissState { get; set; }

    public bool IsBroadcasting { get; set; }
    public bool IsEndingTurn { get; set; }
    public bool IsDoingTurn { get; set; }
    public bool IsChoosingOtherPlayer { get; set; }
    public bool IsChoosingOwnState { get; set; }
    public bool IsChoosingMenuState { get; set; }
    public bool Immune { get; set; }

    public bool AllowChooseSelf { get; set; }

    private string[] statusDelim = new string[] { "\n>>" };

    void Start() {
        Index = Number - 1;
    }

    #region General Player Methods
    public string GetGameStatus() {
        string[] split = gameStatusTextObject.text.Split(statusDelim, System.StringSplitOptions.None);
        if (split.Length == 1) {
            return split[0];
        }
        else {
            return split[1];
        }
    }

    public void SetGameStatus(string newStatus) {
        string currentStatus = gameStatusTextObject.text;
        string[] split = currentStatus.Split(statusDelim, System.StringSplitOptions.None);
        if (split.Length == 2) {
            newStatus = split[1] + "\n>>" + newStatus;
        }
        else {
            newStatus = "\n>>" + newStatus;
        }
        gameStatusTextObject.text = newStatus;
    }

    public void Die(string text) {
        SetGameStatus(text);
        CurrentState = State.Dead;
    }
    #endregion

    #region Vive Controller Methods
    public void OnGlobalTriggerPressDown(XREventData eventData)
    {
        if (!Holojam.Tools.BuildManager.IsMasterClient()) return;
        if (eventData.currentRaycast == null) return;

        if (IsChoosingOtherPlayer) {
            PlayerController otherPlayerController = eventData.currentRaycast.GetComponent<PlayerController> ();
            Debug.Log ("Pointing at " + otherPlayerController);
            if (otherPlayerController != null && otherPlayerController.Immune) {
                SetGameStatus("That player is immune");
                return;
            }
            if (otherPlayerController != null) {
                if (AllowChooseSelf || otherPlayerController != this ) {
                    chosenOtherPlayer = otherPlayerController;
                }
            }
        } else if (IsChoosingOwnState || IsChoosingMenuState) {
            
            StateController otherStateController = eventData.currentRaycast.GetComponent<StateController> ();
            Debug.Log ("Pointing at state: " + otherStateController.GetState());
            if (otherStateController != null) {
                chosenStateController = otherStateController;

            }
        }
    }
    #endregion
}