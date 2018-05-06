﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
	static public int maxHealth = 200;
    public bool alive;

    private int curHealth = 200;

    [SerializeField]
    private BarScript bar;

    void Start()
    {
        //Cursor.visible = true;
        curHealth = maxHealth;
        alive = true;
    }
    void Update()
    {
        if (curHealth < 1)
        {
            CommitSudoku();
        }
    }

    public void TakeDamage(int amount)
    {
        curHealth -= amount;
        bar.fillAmount = curHealth;
    }

    private void CommitSudoku()
    {
        alive = false;
        GetComponent<ShipFixedPathing>().enabled = false;
        GetComponent<Rigidbody>().useGravity = true;
    }
}