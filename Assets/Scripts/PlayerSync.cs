using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam.Network;
using Holojam.Tools;
using State = GameMaster.State;

public class PlayerSync : Synchronizable {

    public string label = "PlayerSync";
    public int Number = 1;

    private PlayerController player;
    private int packCount = 0;

    public override bool Host {
        get {
            return Number == BuildManager.BUILD_INDEX;
        }
    }

    public override bool AutoHost { get { return false; } }

    public override string Label { get { return label; } }

    void Start() {
        player = GetComponent<PlayerController>();
    }

    public override void ResetData() {
        data = new Holojam.Network.Flake(0, 0, 0, 9);
    }

    // 0 something changed from last frame
    // 1 is player CurrentState
    // 2 is Immune
    // 3 is UsedNextState
    // 4-8 is BroadcastData

    public void PackInfo() {
        // Player only broadcasts if they are the currentPlayer and their turn has ended
        //if (player.IsBroadcasting) {
            Debug.Log(player.name + " is broadcasting");
            int i = 0;
            data.ints[i++] = ++packCount;
            data.ints[i++] = (int) player.CurrentState;
            data.ints[i++] = player.Immune ? 1 : 0;
            data.ints[i++] = player.UsedNextState ? 1 : 0;
            for (int j=0; j<4; j++) {
                // CurrentPlayer may have change other player's states
                data.ints[i++] =(int) player.BroadcastStates[j];
            }
            player.IsBroadcasting = false;
        //}
    }

    public void UnpackInfo() {
        int i = 0;
        if (data.ints[i++] != packCount) {
            Debug.Log("Player Sync Unpack");
            player.CurrentState = (State) data.ints[i++];
            player.Immune = data.ints[i++] == 1 ? true : false;
            player.UsedNextState = data.ints[i++] == 1 ? true : false;
            for (int j=0; j<4; j++) {
                // Save these so GameMaster has access to them
                player.BroadcastStates[j] = (State) data.ints[i++];
            }
            player.IsEndingTurn = true;
        }
    }

    protected override void Sync() {
        if (Sending && player.IsBroadcasting) {
            //The player index that matches the build index will send here.
            PackInfo();
        }
        if (BuildManager.IsMasterClient()) {
            UnpackInfo();
        }
    }
}
