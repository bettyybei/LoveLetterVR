﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerController : MonoBehaviour {

    public PlayerController player1;
    public PlayerController player2;

    private PlayerController player;

	void Start () {
		switch(Holojam.Tools.BuildManager.BUILD_INDEX) {
            case 1:
                player = player1;
                break;
            case 2:
                player = player2;
                break;
            default:
                Debug.Log("ERROR IN POINTER");
                break;
        }
	}
	
	void Update () {
        bool pointerEnabled = player.IsChoosingOtherPlayer || player.IsChoosingOwnState || player.IsChoosingMenuState;
        if (pointerEnabled != gameObject.activeSelf) {
            gameObject.SetActive(pointerEnabled);
        }
    }
}