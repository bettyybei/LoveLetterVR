using UnityEngine;
using State = GameMaster.State;

public class StateController : MonoBehaviour {

    public bool isCardController = false;
    private State state;

    public void SetState(State s) {
        this.state = s;
        if (isCardController){
            GetComponent<Renderer> ().material = Resources.Load<Material> ("Materials/Card_" + s.ToString());
        }
    }

    public State GetState() {
        return this.state;
    }
}
