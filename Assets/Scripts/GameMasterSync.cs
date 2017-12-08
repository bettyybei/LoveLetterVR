using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam;
using Holojam.Tools;
using State = GameMaster.State;

public class GameMasterSync : Synchronizable {

	private GameMaster master;

	public override string Label {
		get { return "GameMasterSync"; }
	}

	public override bool Host {
		get { return Holojam.Tools.BuildManager.IsMasterClient(); }
	}

	public override bool AutoHost {
		get { return Host; }
	}

	// Use this for initialization
	void Start () {
		master = GetComponent<GameMaster>();
	}

	public override void ResetData() {
		data = new Holojam.Network.Flake(0, 0, 0, 22);
	}
		
	//0-15 is the deck.
	//16 is the nextStateIdx
	//17-20 is the player current states
	//21 is the currentPlayerIdx

	public void PackInfo() {
		int i = 0;
		for (; i < 16; i++) {
			data.ints[i] = (int) master.deck[i];
		}
		data.ints[i++] = master.nextStateIdx;
		for (int j = 0; j < 4; j++) {
			data.ints[i++] = (int) master.playerStates[j];
		}
		data.ints[i] = master.currentPlayerIdx;
	}

	public void UnpackInfo() {
		int i = 0;
		master.deck = new State[16];
		for (; i < 16; i++) {
			master.deck[i] = (State) data.ints[i];
		}
		master.nextStateIdx = data.ints [i++];
		master.playerStates = new State[4];
		for (int j = 0; j < 4; j++) {
			master.playerStates[j] = (State) data.ints[i++];
		}
		master.currentPlayerIdx = data.ints[i];
	}

	protected override void Sync() {
		if (Sending) {
			//I am the captain now.
			PackInfo();
		} else {
			//I am not the captain now.
			UnpackInfo();
		}
	}
}
