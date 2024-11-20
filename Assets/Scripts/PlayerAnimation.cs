using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private enum State
    {
        Idle = 0,
        Jump = 1,
        RightTurn = 2,
        LeftTurn = 3
    }
    private State curState = State.Idle;

    private void Update()
    {
        GetComponent<Animator>().Play(curState.ToString());
    }

    public void SetAnimState(int state)
    {
        curState = (State)state;
    }
}
