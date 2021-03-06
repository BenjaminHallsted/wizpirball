﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIRVBallsTrigger : MonoBehaviour {

    public int intialDamage = 8;
    public bool ignoreCaster = true;
    public float delayBeforeCasting = 0.0f;
    public float applyEveryNSeconds = 1.0f;
    public int applyDamageNTimes = 3; // means lasts 3 seconds
    public GameObject impact;
    public GameObject MirvBall;
    public float splitDelay;
    public Vector3 move = new Vector3(5, 0, 0); // offset for the balls, hopefully
    public int splitNum = 0;

    private int player;
    private GameObject enemyShip;
    private int appliedTimes = 0;
    private IEnumerator coroutine;
    private bool test = false;
    private string self;
    private string other;
    private Rigidbody ballRB;

    void Start()
    {
        Debug.Log("MIRV BALL UP IN HERE");
        ballRB = GetComponent<Rigidbody>();
        self = GetComponent<PlayerSelector>().me;
        other = GetComponent<PlayerSelector>().notMe;
        enemyShip = GameObject.FindGameObjectWithTag("Ship_" + other);
        Invoke("Split", splitDelay);
    }

    void Update()
    {

    }

    public void SetPlayer(int playerNum)
    {
        player = playerNum;
    }

    void OnTriggerEnter(Collider col)
    {
        //all projectile colliding game objects should be tagged "Enemy" or whatever in inspector but that tag must be reflected in the below if conditional
        if (col.gameObject.tag == "Ship_" + other)
        {
            //add an explosion or something
            ShipController curhealth = enemyShip.GetComponent<ShipController>();
            //if exists
            if (curhealth != null)
            {
                curhealth.TakeDamage(intialDamage);
                
            }
            var explosion = (GameObject)Instantiate(impact, transform.position, transform.rotation);
            explosion.transform.SetParent(enemyShip.transform);
            explosion.GetComponent<ParticleSystem>().Play();
            explosion.GetComponent<AudioSource>().Play();

            StartCoroutine(DestoryAfterDelay(4.0f, gameObject));
            StartCoroutine(DestoryAfterDelay(3.0f, explosion));
        }
        if (col.gameObject.tag == "WATER")
        {
            StartCoroutine(DestoryAfterDelay(4.0f, gameObject));
        }
        else if (col.gameObject.tag == "Rock")
        {
            StartCoroutine(DestoryAfterDelay(1.0f, gameObject));
        }
        //GetComponent<MIRVBallsTrigger>().enabled = false;
    }
    void Split()
    {
        if(splitNum < 2)
        {
            var ballA = (GameObject)Instantiate(MirvBall, transform.position, transform.rotation);
            ballA.transform.Rotate(0, 10, 0);
            ballA.GetComponent<Rigidbody>().velocity = ballRB.velocity.magnitude * ballA.transform.forward;
            ballA.GetComponent<MIRVBallsTrigger>().splitNum++;

            var ballB = (GameObject)Instantiate(MirvBall, transform.position, transform.rotation);
            ballB.GetComponent<Rigidbody>().velocity = ballRB.velocity;
            ballB.GetComponent<MIRVBallsTrigger>().splitNum++;

            var ballC = (GameObject)Instantiate(MirvBall, transform.position, transform.rotation);
            ballC.transform.Rotate(0, -10, 0);
            ballC.GetComponent<Rigidbody>().velocity = ballRB.velocity.magnitude * ballC.transform.forward;
            ballC.GetComponent<MIRVBallsTrigger>().splitNum++;

            Destroy(this.gameObject);
        }
    }
    private IEnumerator DestoryAfterDelay(float Delay, GameObject destroyable)
    {
        Debug.Log("entered Destory");
        bool alphaBool = true;
        while (alphaBool)
        {
            yield return new WaitForSeconds(Delay);
            Debug.Log("Waited for " + Delay + " seconds");
            Destroy(destroyable);
            alphaBool = false;
        }
    }
}