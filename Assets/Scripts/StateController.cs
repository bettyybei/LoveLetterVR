using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = GameMaster.State;

public class StateController : MonoBehaviour {

	private Renderer rend;
	private State state;

	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer> ();
		Debug.Log ("texture: " + rend.material.mainTexture);
		rend.material.mainTextureOffset = new Vector2 (0.5f, 0.5f);
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
