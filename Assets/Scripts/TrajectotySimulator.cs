using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrajectotySimulator : MonoBehaviour
{
    [Tooltip("�e����\�����鎞�Ɏg���I�u�W�F�N�g")]
    [SerializeField] private GameObject simObj;
    [Tooltip("�V�~�����[�g�ŕ\������I�u�W�F�N�g�̏��")]
    [SerializeField] private int simLmt = 20;
    [Tooltip("�V�~�����[�g�Ԋu")]
    [SerializeField] private float simInterval = 0.5f;

    //�V�~�����[�g�֘A
    private List<GameObject> simList;
    private Vector3 speed = Vector3.zero;
    private float gravity = 0;
    private bool isSim = false;
    private Vector3 targetPos = Vector3.zero;

    //�X�e�[�g�֘A
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

    //�X�e�[�g����Update
    private void UpdateNone()
    {
        //Start
        if(curState != prevState)
        {
            prevState = curState;

            foreach (GameObject go in simList)
            {
                Destroy(go);
            }

            simList.Clear();
        }

        //Process

        //End
        if (isSim)
        {
            curState = State.Sim;
        }
    }

    private void UpdateSim()
    {
        //Start
        if(curState != prevState)
        {
            prevState = curState;

            for(int i = 0; i < simLmt; i++)
            {
                simList.Add(Instantiate(simObj));
            }
        }

        //Process
        var objPos = targetPos;
        var simSpd = speed;

        for (int i = 0; i < simLmt; i++)
        {
            objPos += simSpd * simInterval;
            simList[i].transform.position = objPos;
            simSpd.y -= gravity * simInterval;
        }

        //End
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
