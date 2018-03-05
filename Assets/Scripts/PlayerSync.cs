using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam.Network;
using Holojam.Tools;
using State = GameMaster.State;

public class PlayerSync : Synchronizable {

    public string label = "PlayerSync";
    public int playerIndex = 1;

    private PlayerController player;
    private int packCount = 0;

    public override bool Host {
        get {
            return playerIndex == BuildManager.BUILD_INDEX;
        }
    }

    public override bool AutoHost { get { return Host; } }

    public override string Label { get { return label; } }

    void Start() {
        player = GetComponent<PlayerController>();
    }

    public override void ResetData() {
        data = new Holojam.Network.Flake(0, 0, 0, 22);
    }

    // 0 something changed
    // 1 is player CurrentState
    // 2 is Immune
    // 3 is UsedNextState
    // 4-8 is BroadcastData

    public void PackInfo() {
        if (player.IsBroadcasting) {
            int i = 0;
            data.ints[i++] = ++packCount;
            data.ints[i++] = (int)player.CurrentState;
            data.ints[i++] = player.Immune ? 0 : 1;
            data.ints[i++] = player.UsedNextState ? 0 : 1;
            // broadcastdata
            player.IsBroadcasting = false;
        }
    }

    public void UnpackInfo() {
        int i = 0;
        if (data.ints[i++] != packCount) {
            player.CurrentState = (State)data.ints[i++];
            player.Immune = data.ints[i++] == 1 ? true : false;
            player.UsedNextState = data.ints[i++] == 1 ? true : false;
            // broadcastdata
            player.IsEndingTurn = true;
        }
    }

    protected override void Sync() {
        if (Sending) {
            //The player index that matches the build index will send here.
            PackInfo();
        } else {
            //If my player index doesn't match the build index, I will receive here.
            UnpackInfo();
        }
    }
}
