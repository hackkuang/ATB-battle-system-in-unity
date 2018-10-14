﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateMachine : MonoBehaviour {

    public BaseHero hero;

    private BattleStateMachine BSM;
    public enum TurnState
    {
        PROCESSING,
        ADDTOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD
    }

    public TurnState currentState;
    //for progress bar
    private float curCooldown = 0.0f;
    private float maxCooldown = 5.0f;
    public Image progressBar;

    //find selector game object
    public GameObject selector;

    //ienumerator
    public GameObject targetEnemy;
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 10f;

	// Use this for initialization
	void Start ()
    {
        curCooldown = Random.Range(0, 2.5f);
        currentState = TurnState.PROCESSING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        selector.SetActive(false);
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update ()
    {
        Debug.Log(currentState);
        switch (currentState)
        {
            case (TurnState.PROCESSING):
                updateProgressBar();
                break;

            case (TurnState.ADDTOLIST):
                BSM.PlayerManagement.Add(this.gameObject);
                currentState = TurnState.WAITING;
                break;

            case (TurnState.WAITING):
                //idle
                break;

            case (TurnState.ACTION):
                StartCoroutine(actionTimer());
                break;

            case (TurnState.DEAD):

                break;
        }

	}

    void updateProgressBar()
    {

        curCooldown = curCooldown + Time.deltaTime;
        float calcualtion = curCooldown / maxCooldown;
        progressBar.transform.localScale = new Vector3(Mathf.Clamp(calcualtion,0,1),progressBar.transform.localScale.y,progressBar.transform.localScale.z);

        if (curCooldown >= maxCooldown)
        {
            currentState = TurnState.ADDTOLIST;
        }
    }

    private IEnumerator actionTimer()
    {

        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;
        //animate player
        Vector3 targetPos = new Vector3(targetEnemy.transform.position.x + 1.5f, targetEnemy.transform.position.y, targetEnemy.transform.position.z);
        while (MoveToEnemy(targetPos))
        {
            yield return null;
        }
        //wait
        yield return new WaitForSeconds(0.5f);
        //damage
        
        //animate back to start position
        Vector3 originPOS = startPosition;
        while (MoveToOrigin(originPOS))
        {
            yield return null;
        }
        //remove from bsm list
        BSM.TurnList.RemoveAt(0);

        //reset bsm to wait
        BSM.battlestate = BattleStateMachine.PerformAction.WAIT;
        //end coroutine
        actionStarted = false;

        //reset enemy state
        curCooldown = 0f;
        currentState = TurnState.PROCESSING;
    }

    private bool MoveToEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    private bool MoveToOrigin(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    public void takeDamage(float damageAmount)
    {
        //reduce hp by damage amount
        hero.currentHP -= damageAmount;
        //check if dead
        if (hero.currentHP <=0)
        {
            currentState = TurnState.DEAD;
        }
    }

}
