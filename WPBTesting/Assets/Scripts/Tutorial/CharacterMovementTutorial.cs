﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementTutorial : MonoBehaviour
{

    public float speed = 0.02f; // Player movement speed
    public GameObject spawnPoint; // Where player spawns if they fall off the ship
    public int playerNum; // Player 1 is 1, Player 2 is 2
    public bool stunned = false;
    public GameObject ShieldPrefab;// need editing
    public Transform bulletSpawn;
    public GameObject bulletPrefab; // Gets overwritten once the player changes ball types
    public GameObject DefaultBall; // Ordinary Baseball
    public GameObject[] Bullets; // Array of all possible magic balls
    public Transform batAimer;
    public AudioClip batSound;
    public float batDelay = 2.0f;
    public int tHits = 0;
    public int counterHits = 0;

    public int mana; // starts at 100

    private GameObject mark;

    // This dictionary maps a universal set of numbers to player specific inputs. It gets defined in Start().
    public Dictionary<int, string> playerInput;

    public GameObject[] shields; // 0,1-back, 2-left 3-right 4,5-front

    private GameObject LeftBarr; // left shield
    private GameObject RightBarr;
    private GameObject FrontBarr;
    private GameObject BackBarr;

    private int[] Loadout;

    private int manaCost; // This is cost for spells. Costs are defined in PlayerSelecter component of each ball.
    private int shieldCost = 20;

    private IEnumerator manaRegen;
    private float fireDelay = 0.8f;
    private float verticalInput;
    private float horizontalInput;
    private float timestamp;
    private bool canRespawn = true;
    private IEnumerator mCall;
    private IEnumerator hCall;
    private IEnumerator bCall;

    private bool Xmana = false;
    private bool Xhealth = false;
    private bool Xballs = false;
    private bool[] spellsUsed = new bool[] { false, false, false };//fire, ice, lightning
    private bool trackSpells = false;
    private bool trackCounterHits = false;
    private bool trackshields = false;

    private int currentBall = -1;
    private Animator anim;
    //Animator legsAnim;
    private AudioSource audioSource;
    private DBoxManager dBoxMan;

    private GameObject OOM;
    private IEnumerator OOMCaller;

    [SerializeField]
    private BarScript bar;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Set the position, and make the player start out looking at the enemy ship.
        this.transform.position = spawnPoint.transform.position;
        //this.transform.LookAt(GameObject.FindGameObjectWithTag("Ship_P2").transform);

        anim = GetComponent<Animator>();
        anim.SetBool("Moving", false);

        // Find and set barriers
        LeftBarr = GameObject.Find("ShieldActivatePoint1P" + playerNum);
        dBoxMan = GameObject.Find("DialogueManager").GetComponent<DBoxManager>();

        OOM = GameObject.Find("OOMTextP" + playerNum);
        OOM.SetActive(false);


        // If we didn't come from custom loadout selector, set a default loadout. Fire, Ice, and Lightning.
        Loadout = new int[] { 0, 1, 2 };

        // Startup mana regen
        manaCost = 0;
        manaRegen = Regen();
        StartCoroutine(manaRegen);
        mCall = manaCall();
        hCall = healthCall();
        bCall = ballCall();
        OOMCaller = OOMCall();

        // Set input dictionary appropriate to player
        if (playerNum == 1)
        {
            playerInput = new Dictionary<int, string>()
            {
                {0, "X_1" },
                {1, "Y_1" },
                {2, "A_1" },
                {3, "B_1" },
                {4, "RightStickX" },
                {5, "RightStickY" },
                {6, "Horizontal" },
                {7, "Vertical" },
                {8, "LeftBumper" },
                {9, "RightBumper" },
                {10, "LeftTrigger" },
                {11, "RightTrigger" }
            };
        }
        else if (playerNum == 2)
        {
            playerInput = new Dictionary<int, string>()
            {
                {0, "X_2" },
                {1, "Y_2" },
                {2, "A_2" },
                {3, "B_2" },
                {4, "RightStickX_2" },
                {5, "RightStickY_2" },
                {6, "Horizontal_2" },
                {7, "Vertical_2" },
                {8, "LeftBumper_2" },
                {9, "RightBumper_2" },
                {10, "LeftTrigger_2" },
                {11, "RightTrigger_2" }
            };
        }
        else
        {
            Debug.LogWarning("Player Number not set!");
        }


    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetAxis(playerInput[11]) > 0) && Time.time >= timestamp)
        {
            //Debug.Log("TRIGGERED");
            timestamp = Time.time + batDelay;
            if (!stunned)
            {
                Invoke("Fire", fireDelay);
                anim.Play("Armature|Hitting");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetButtonDown(playerInput[2]))
        {
            bulletPrefab = DefaultBall; // normal
            manaCost = bulletPrefab.GetComponent<PlayerSelector>().manaCost;
            //if a button going to trigger next dialogue trigger here
            if (Xmana)
            {
                StartCoroutine(manaCall());
            }
            if (Xhealth)
            {
                StartCoroutine(healthCall());
            }
            if (Xballs)
            {
                StartCoroutine(ballCall());
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetButtonDown(playerInput[3]))
        {
            bulletPrefab = Bullets[Loadout[0]];
            currentBall = 0;
            manaCost = bulletPrefab.GetComponent<PlayerSelector>().manaCost;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetButtonDown(playerInput[0]))
        {
            currentBall = 1;
            bulletPrefab = Bullets[Loadout[1]];
            manaCost = bulletPrefab.GetComponent<PlayerSelector>().manaCost;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetButtonDown(playerInput[1]))
        {
            currentBall = 2;
            bulletPrefab = Bullets[Loadout[2]];
            manaCost = bulletPrefab.GetComponent<PlayerSelector>().manaCost;
        }

        if (tHits >= 3)
        {
            dBoxMan.ReadyDialogue(5, playerNum);//lead into explaining magic balls
            trackSpells = true;
        }
        if (trackSpells && spellsUsed[0])//explain fire ball
        {
            dBoxMan.ReadyDialogue(6, playerNum);//go to ice
        }
        if (trackSpells && spellsUsed[1])//ice ball
        {
            dBoxMan.ReadyDialogue(7, playerNum);//go to lightning
        }
        if (trackSpells && spellsUsed[2])//lightning
        {
            dBoxMan.ReadyDialogue(8, playerNum);//goes to...
            trackCounterHits = true;
        }
        if (trackCounterHits && counterHits > 2)//go to counter hitting
        {
            dBoxMan.ReadyDialogue(9, playerNum);//3rd to last
            trackshields = true;
        }

        /*if (trackCounterHits && counterHits > 2)
        {
            dBoxMan.ReadyDialogue(10, playerNum);//2nd to last
            trackshields = true;
        }*/
    }
    void FixedUpdate()
    {
        verticalInput = Input.GetAxis(playerInput[7]);
        horizontalInput = Input.GetAxis(playerInput[6]);
        transform.Translate(horizontalInput * speed, 0.0f, verticalInput * speed);
        if (verticalInput != 0 || horizontalInput != 0)
        {
            //legsAnim.SetTrigger("Moving");
            anim.SetBool("Moving", true);
            anim.SetBool("Idle", false);

        }
        else
        {
            //legsAnim.SetTrigger("Stopping");
            anim.SetBool("Idle", true);
            anim.SetBool("Moving", false);
        }

    }
    IEnumerator Regen()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (!(mana >= 100) && Time.time >= batDelay + 0.1)
            {
                mana += 2;
                bar.fillAmount = mana;
            }
        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.transform.tag == "WATER" && canRespawn)
        {
            canRespawn = false;
            Invoke("respawnPlayer", 4);
        }
        if (col.transform.tag == "TutorialBox")
        {
            dBoxMan.ReadyDialogue(1, playerNum);
            Xmana = true;
        }
    }
    void OnTriggerStay(Collider col)
    {
        if(col.transform.tag == "Marker1" || col.transform.tag == "Marker2")
        {
            mark = col.gameObject;
            mark.SetActive(false);
        }
        if (trackshields)
        {
            if (col.transform.tag == "ShieldActivator" && (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown(playerInput[9])))
            {
                if (col.gameObject == LeftBarr) //1-left, 2-right, 3-front, 4-back
                {
                    if (shields[0].activeSelf)
                    {
                        shields[0].SetActive(false);
                    } else if (mana > shieldCost)
                        {
                            shields[0].GetComponent<ShieldController>().Blocks = 3;
                            shields[0].SetActive(true);
                            mana -= shieldCost;
                            bar.fillAmount = mana;
                            dBoxMan.ReadyDialogue(10, playerNum);//end
                        }
                    else
                    {
                        StartCoroutine(OOMCall());
                    }
                    
                }
            }
        }
    }
    void respawnPlayer()
    {
        this.transform.position = spawnPoint.transform.position;
        this.transform.rotation = spawnPoint.transform.rotation;
        canRespawn = true;
    }


    public void Fire()
    {
        if (mana > manaCost)
        {
            if (trackSpells && currentBall >= 0)// 0-fire, 1-ice, 2-lightning
            {
                spellsUsed[currentBall] = true;
            }
            audioSource.PlayOneShot(batSound, 1.0f);
            // Create the Bullet from the Bullet Prefab
            var bullet = Instantiate(bulletPrefab, bulletSpawn.position, batAimer.rotation);
            bullet.GetComponent<PlayerSelector>().SetPlayer(playerNum == 1 ? "P1" : "P2");
            mana -= manaCost;
            bar.fillAmount = mana;

            // Add velocity to the bullet
            bullet.GetComponent<Rigidbody>().velocity = batAimer.forward * Ballistics.bulletSpeed;
            bullet.transform.rotation = Quaternion.LookRotation(bullet.GetComponent<Rigidbody>().velocity, Vector3.up);
            anim.SetBool("Hitting", false);
        }
        else
        {
            StartCoroutine(OOMCall());
        }
    }
    IEnumerator manaCall()
    {
        yield return new WaitForSeconds(0.1f);
        dBoxMan.ReadyDialogue(2, playerNum);
        Xmana = false;
        Xhealth = true;
    }
    IEnumerator healthCall()
    {
        yield return new WaitForSeconds(0.1f);
        dBoxMan.ReadyDialogue(3, playerNum);
        Xhealth = false;
        Xballs = true;
    }
    IEnumerator ballCall()
    {
        yield return new WaitForSeconds(0.1f);
        dBoxMan.ReadyDialogue(4, playerNum);
        Xballs = false;
    }
    IEnumerator OOMCall()
    {
        OOM.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        OOM.SetActive(false);
    }
}
