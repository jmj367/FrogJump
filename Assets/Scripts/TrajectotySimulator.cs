using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrajectotySimulator : MonoBehaviour
{
    [Tooltip("弾道を表示する時に使うオブジェクト")]
    [SerializeField] private GameObject simObj;
    [Tooltip("シミュレートで表示するオブジェクトの上限")]
    [SerializeField] private int simLmt = 20;
    [Tooltip("シミュレート間隔")]
    [SerializeField] private float simInterval = 0.5f;

    private List<GameObject> simList;
    private Vector3 speed = Vector3.zero;
    private float gravity = 0;
    private bool isSim = false;
    private Vector3 targetPos = Vector3.zero;

    private enum State
    {
        None,
        Sim
    }
    private State curState = State.None;
    private State prevState = State.None;

    private void Start()
    {
        simList = new List<GameObject>();
    }

    private void Update()
    {
        switch (curState)
        {
            case State.None: UpdateNone(); break;
            case State.Sim: UpdateSim(); break;
        }
    }

    private void UpdateNone()
    {
        if(curState != prevState)
        {
            prevState = curState;

            foreach (GameObject go in simList)
            {
                Destroy(go);
            }

            simList.Clear();
        }

        if (isSim)
        {
            curState = State.Sim;
        }
    }

    private void UpdateSim()
    {
        if(curState != prevState)
        {
            prevState = curState;

            for(int i = 0; i < simLmt; i++)
            {
                simList.Add(Instantiate(simObj));
            }
        }

        var objPos = targetPos;
        var simSpd = speed;

        for (int i = 0; i < simLmt; i++)
        {
            objPos += simSpd * simInterval;
            simList[i].transform.position = objPos;
            simSpd.y -= gravity * simInterval;
        }

        if (!isSim)
        {
            curState= State.None;
        }
    }

    public void SetIsSim(bool ism)
    {
        isSim = ism;
    }

    public void SetValue(Vector3 pos, Vector3 spd, float g)
    {
        targetPos = pos;
        speed = spd;
        gravity = g;
    }
}
