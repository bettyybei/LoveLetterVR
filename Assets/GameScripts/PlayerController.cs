using UnityEngine;
using FRL.IO;
using State = GameMaster.State;
[RequireComponent(typeof(Receiver))]

public class PlayerController : MonoBehaviour, IGlobalTriggerPressDownHandler, IGlobalTouchpadPressDownHandler {

    public int Number;

    public PlayerController chosenOtherPlayer { get; set; }
    public StateController chosenStateController { get; set; }

    public State CurrentState { get; set; }
    public State DismissState { get; set; }

    public string GameStatusText = "";
    public int InstructionNum { get; set; }
    public bool IsDoingTurn { get; set; }
    public bool IsChoosingOtherPlayer { get; set; }
    public bool IsChoosingOwnState { get; set; }
    public bool IsChoosingMenuState { get; set; }
    public bool Immune { get; set; }
    public bool AllowChooseSelf { get; set; }

    private string[] statusDelim = new string[] { "\n>>" };
    const string _ChooseAnotherPlayerText = "Choose another player ";

  void Start() {
        InstructionNum = 0;
    }

    #region General Player Methods
    public string GetName() {
        return "Player " + Number;
    }
    public string GetGameStatus() {
        string[] split = GameStatusText.Split(statusDelim, System.StringSplitOptions.None);
        if (split.Length == 1) {
            return split[0];
        }
        else {
            return split[1];
        }
    }

    public void SetGameStatus(string newStatus) {
        string[] split = GameStatusText.Split(statusDelim, System.StringSplitOptions.None);
        if (split.Length == 2) {
            newStatus = split[1] + "\n>>" + newStatus;
        }
        else {
            newStatus = "\n>>" + newStatus;
        }
        GameStatusText = newStatus;
    }

    public void Die(string status) {
        SetGameStatus(status);
        CurrentState = State.Dead;
    }
    #endregion

    #region Vive Controller Methods
    public void OnGlobalTouchpadPressDown(XREventData eventData)
    {
        Debug.Log("touchpad press");
        if (Holojam.Tools.BuildManager.IsMasterClient()) {
            if ((InstructionNum == -1)) {
                InstructionNum = 0;
            } else {
                InstructionNum = -1;
            }
        }
    }

    public void OnGlobalTriggerPressDown(XREventData eventData) {
        if (!Holojam.Tools.BuildManager.IsMasterClient()) return;
        Debug.Log("trigger press");
        if (InstructionNum != -1) {
            Debug.Log("Switching instructions");
            InstructionNum = (InstructionNum + 1) % 4;
        } else {
            Debug.Log("Raycasting");
            if (eventData.currentRaycast == null) return;
            Debug.Log("" + IsChoosingOtherPlayer + IsChoosingOwnState + IsChoosingMenuState);
            if (IsChoosingOtherPlayer) {
                PlayerController otherPlayerController = eventData.currentRaycast.GetComponent<PlayerController>();
                Debug.Log("Pointing at " + otherPlayerController);
                if (otherPlayerController == null) return;
                if (otherPlayerController.Immune) {
                    SetGameStatus(_ChooseAnotherPlayerText + "- that player is immune");
                    return;
                }
                if (otherPlayerController.CurrentState == State.Dead) {
                    SetGameStatus(_ChooseAnotherPlayerText + "- that player is dead");
                    return;
                }
                if (AllowChooseSelf || otherPlayerController != this) {
                    chosenOtherPlayer = otherPlayerController;
                }
            } else if (IsChoosingOwnState || IsChoosingMenuState) {

                StateController otherStateController = eventData.currentRaycast.GetComponent<StateController>();
                Debug.Log("Pointing at state: " + otherStateController.GetState());
                if (otherStateController != null) {
                    chosenStateController = otherStateController;

                }
            }
        }
    }
    #endregion
}