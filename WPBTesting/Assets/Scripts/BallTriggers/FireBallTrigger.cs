﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallTrigger : MonoBehaviour
{

    public int initialDamage = 10;
    public int tickDamage = 5;
    public GameObject impact;
    public bool ignoreCaster = true;
    public float delayBeforeCasting = 0.0f;
    public float applyEveryNSeconds = 1.0f;
    public int applyDamageNTimes = 5;

    private int appliedTimes = 0;
    private IEnumerator coroutine;
    private bool test = false;
    private string self;
    private string other;
    private GameObject enemyShip;
    private GameObject myShip;


    // Use this for initialization
    void Start()
    {
        self = GetComponent<PlayerSelector>().me;
        other = GetComponent<PlayerSelector>().notMe;
        enemyShip = GameObject.FindGameObjectWithTag("Ship_" + other);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider col)
    {
        //all projectile colliding game objects should be tagged "Enemy" or whatever in inspector but that tag must be reflected in the below if conditional
        if (col.gameObject.tag == "Ship_" + other)
        {
            //Destroy(col.gameObject);
            //add an explosion or something
            ShipController curhealth = enemyShip.GetComponent<ShipController>();
            //if exists
            if (curhealth != null)
            {
                curhealth.TakeDamage(initialDamage);
                IEnumerator Coroutine = CastDamage(curhealth);
                StartCoroutine(Coroutine);
            }

            var explosion = (GameObject)Instantiate(impact, transform.position, transform.rotation);
            explosion.transform.SetParent(enemyShip.transform);
            explosion.GetComponent<ParticleSystem>().Play();
            explosion.GetComponent<AudioSource>().Play();

            StartCoroutine(DestoryAfterDelay(5.0f, gameObject));
            StartCoroutine(DestoryAfterDelay(3.0f, explosion));
            //destroy the projectile that just caused the trigger collision
            //Destroy(gameObject);
        }

        else if (col.gameObject.tag == "Ship_" + self)
        {
            myShip = GameObject.FindGameObjectWithTag("Ship_" + self);
            //Destroy(col.gameObject);
            //add an explosion or something
            ShipController curhealth = myShip.GetComponent<ShipController>();
            //if exists
            if (curhealth != null)
            {
                curhealth.TakeDamage(initialDamage);
                IEnumerator Coroutine = CastDamage(curhealth);
                StartCoroutine(Coroutine);
            }

            var explosion = (GameObject)Instantiate(impact, transform.position, transform.rotation);
            explosion.transform.SetParent(myShip.transform);
            explosion.GetComponent<ParticleSystem>().Play();
            explosion.GetComponent<AudioSource>().Play();

            //destroy the projectile that just caused the trigger collision
            StartCoroutine(DestoryAfterDelay(5.0f, gameObject));
            StartCoroutine(DestoryAfterDelay(3.0f, explosion));
        }

        if (col.gameObject.tag == "TutorialTarget")
        {
            var explosion = (GameObject)Instantiate(impact, transform.position, transform.rotation);
            explosion.GetComponent<ParticleSystem>().Play();
            explosion.GetComponent<AudioSource>().Play();
            Destroy(gameObject);
        }

        if (col.gameObject.tag == "WATER")
        {
            StartCoroutine(DestoryAfterDelay(5.0f, gameObject));
        }
        else if (col.gameObject.tag == "Rock")
        {
            StartCoroutine(DestoryAfterDelay(1.0f, gameObject));
        }
        GetComponent<FireBallTrigger>().enabled = false;
    }

    IEnumerator CastDamage(ShipController damageable)
    {
        while (true)
        {
            yield return new WaitForSeconds(applyEveryNSeconds);
            if (!test && appliedTimes <= applyDamageNTimes || !test && applyEveryNSeconds == 0)
            {
                test = true;
                damageable.TakeDamage(tickDamage);
                appliedTimes++;
                test = false;
            }
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
