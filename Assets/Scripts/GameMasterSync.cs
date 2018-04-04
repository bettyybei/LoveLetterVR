using Holojam.Tools;
using State = GameMaster.State;
using System.Text;

public class GameMasterSync : Synchronizable {

	private GameMaster master;

	public override string Label {
		get { return "GameMasterSync"; }
	}

	public override bool Host {
		get { return BuildManager.IsMasterClient(); }
	}

	public override bool AutoHost {
		get { return Host; }
	}

	void Start () {
		master = GetComponent<GameMaster>();
	}

	public override void ResetData() {
		data = new Holojam.Network.Flake(0, 0, 0, 24, 0, true);
	}
		
	//0-15 is the deck
	//16 is the nextStateIdx
	//17-20 is the player current states
	//21 is the currentPlayerIdx
    //22-23 are the TwoStateMenu states to display
    
    //text is game status of players separated by "_"

	public void PackInfo() {
		int i = 0;
		for (; i < 16; i++) {
			data.ints[i] = (int) master.deck[i];
		}
		data.ints[i++] = master.nextStateIdx;
		for (int j = 0; j < 4; j++) {
            if (j < master.players.Length) {
                data.ints[i] = (int) master.players[j].CurrentState;
            }
            i++;
		}
		data.ints[i++] = master.currentPlayerIdx;
        data.ints[i++] = (int) master.stateCard1.GetState();
        data.ints[i++] = (int) master.stateCard2.GetState();

        StringBuilder sb = new StringBuilder();
        for (int j = 0; j < master.players.Length; j++) {
            sb.Append(master.players[j].gameStatusTextObject.text).Append("_");
        }
        data.text = sb.ToString();
	}

	public void UnpackInfo() {
		int i = 0;
		master.deck = new State[16];
		for (; i < 16; i++) {
			master.deck[i] = (State) data.ints[i];
		}
		master.nextStateIdx = data.ints[i++];
		for (int j = 0; j < 4; j++) {
            if (j < master.players.Length) {
                master.players[j].CurrentState = (State) data.ints[i];
            }
            i++;
		}
		master.currentPlayerIdx = data.ints[i++];
        master.stateCard1.SetState((State) data.ints[i++]);
        master.stateCard2.SetState((State) data.ints[i++]);

        string[] gameStatusTexts = data.text.Split('_');
        for (int j = 0; j < (gameStatusTexts.Length - 1); j++) {
            master.players[j].gameStatusTextObject.text = gameStatusTexts[j];
        }
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
