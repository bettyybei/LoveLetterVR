using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam;
using Holojam.Tools;

public class GameMasterSync : Synchronizable {

	private GameMaster master;

	public override string Label {
		get { return "GameMasterSync"; }
	}

	public override bool Host {
		get { return Holojam.Tools.BuildManager.IsMasterClient (); }
	}

	public override bool AutoHost {
		get { return Host; }
	}

	// Use this for initialization
	void Start () {
		master = GetComponent<GameMaster> ();
	}

	public override void ResetData() {
		data = new Holojam.Network.Flake (0, 0, 0, 16);
	}

	//int nextStateIdx
	//0-15 is the deck.
	//16 is the nextStateIdx
	//17-20 is the player current states
	//21 is the currentPlayerIdx

	public void PackDeck() {
		for (int i = 0; i < master.deck.Length; i++) {
			data.ints [i] = (int)master.deck [i];
		}
	}

	public void UnpackDeck() {
		master.deck = new GameMaster.State[16];
		for (int i = 0; i < 16; i++) {
			master.deck [i] = (GameMaster.State)data.ints [i];
		}
	}

	protected override void Sync() {
		if (Sending) {
			//I am the captain now.
			PackDeck();
		} else {
			//I am not the captain now.
			UnpackDeck();
		}
	}
}
