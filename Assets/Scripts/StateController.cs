using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = GameMaster.State;

public class StateController : MonoBehaviour {

	public bool isCardController = false;
	private State state;
	private Mesh mesh;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void SetState(State s) {
		this.state = s;
		if (isCardController){
			GetComponent<Renderer> ().material = Resources.Load<Material> ("Materials/Card_" + s.ToString());
		}
		Debug.Log (this + " set state to " + s);

	}

	public State GetState() {
		return this.state;
	}
}
