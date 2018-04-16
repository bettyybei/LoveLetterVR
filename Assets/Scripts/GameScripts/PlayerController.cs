using UnityEngine;
using UnityEngine.SceneManagement;
using FRL.IO;
using State = GameMaster.State;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler, IGlobalTouchpadPressDownHandler {

    public TextMesh gameStatusTextObject;
    public int Number;

    public PlayerController chosenOtherPlayer { get; set; }
    public StateController chosenStateController { get; set; }

    public State CurrentState { get; set; }
    public State DismissState { get; set; }

    public bool IsDoingTurn { get; set; }
    public bool IsChoosingOtherPlayer { get; set; }
    public bool IsChoosingOwnState { get; set; }
    public bool IsChoosingMenuState { get; set; }
    public bool Immune { get; set; }
    public bool AllowChooseSelf { get; set; }

    private string[] statusDelim = new string[] { "\n>>" };

    void Start() {
    }

    #region General Player Methods
    public string GetName() {
        return "Player " + Number;
    }
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
        string[] split = gameStatusTextObject.text.Split(statusDelim, System.StringSplitOptions.None);
        if (split.Length == 2) {
            newStatus = split[1] + "\n>>" + newStatus;
        }
        else {
            newStatus = "\n>>" + newStatus;
        }
        gameStatusTextObject.text = newStatus;
    }

    public void Die(string status) {
        SetGameStatus(status);
        CurrentState = State.Dead;
    }
    #endregion

    #region Vive Controller Methods
    public void OnGlobalTriggerPressDown(XREventData eventData)
    {
        SceneManager.LoadScene("Introduction");
    }

    public void OnGlobalTouchpadPressDown(XREventData eventData) {
        if (!Holojam.Tools.BuildManager.IsMasterClient()) return;
        if (eventData.currentRaycast == null) return;
        Debug.Log("" + IsChoosingOtherPlayer + IsChoosingOwnState + IsChoosingMenuState);
        if (IsChoosingOtherPlayer) {
            PlayerController otherPlayerController = eventData.currentRaycast.GetComponent<PlayerController>();
            Debug.Log("Pointing at " + otherPlayerController);
            if (otherPlayerController != null && otherPlayerController.Immune) {
                SetGameStatus("That player is immune");
                return;
            }
            if (otherPlayerController != null) {
                if (AllowChooseSelf || otherPlayerController != this) {
                    chosenOtherPlayer = otherPlayerController;
                }
            }
        } else if (IsChoosingOwnState || IsChoosingMenuState) {

            StateController otherStateController = eventData.currentRaycast.GetComponent<StateController>();
            Debug.Log("Pointing at state: " + otherStateController.GetState());
            if (otherStateController != null) {
                chosenStateController = otherStateController;

            }
        }
    }
    #endregion
}