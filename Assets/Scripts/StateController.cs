using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = GameMaster.State;

public class StateController : MonoBehaviour {

	private State state;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetState(State s) {
		this.state = s;
		//Debug.Log (this + " set state to " + s);
	}

	public State GetState() {
		return this.state;
	}
}
