using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actions = Brain.Actions;
using DataForBrainInput = Brain.DataForBrainInput;

public class UnitController : MonoBehaviour
{
    private GameController gameController;
    readonly private string name_gameController = "GameController";
    [SerializeField]
    private Brain brain;
    private Actions action;
    private DataForBrainInput dataForBrain;
    private int updateCount=0;
    private int span_getAction=10;

    private float hp;
    [SerializeField]
    private float hp_MAX;
    [field: SerializeField]
    public float atk { get; private set; }
    [field: SerializeField, Range(0,5)]
    public float atk_range { get; private set; }

    private bool attack;

    private GameObject player;
    readonly private string tag_player= "Fish_Player";

    // -- Flock専用 -- //
    private Flock2D flock;
    // -- -- //

    // -- Chase専用 -- //
    private Chase2D chase;
    // -- -- //

    // -- Evade専用 -- //
    private Evade2D evade;
    // -- -- //


    public void ManagedStart()
    {
        gameController = GameObject.Find(name_gameController).GetComponent<GameController>();
        player = GameObject.FindGameObjectWithTag(tag_player);
        dataForBrain = new DataForBrainInput();

        hp = hp_MAX;

        // -- Flock専用 -- //
        flock = GetComponent<Flock2D>();
        flock.Initialize();
        // -- -- //

        // -- Chase専用 -- //
        chase = GetComponent<Chase2D>();
        chase.Initialize();
        // -- -- //

        // -- Evade専用 -- //
        evade = GetComponent<Evade2D>();
        evade.Initialize();
        // -- -- //
    }

    void Update()
    {
        dataForBrain.SetData(gameController.NOFunit, hp, hp_MAX, (player.transform.position - transform.position).sqrMagnitude);

        updateCount++;
        if (updateCount > span_getAction)
        {
            action = brain.GetAction(dataForBrain);
            updateCount = 0;
        }

        switch (action)
        {
            case Actions.Flock:
                flock.DoFlock();
                break;
            case Actions.Chase:
                chase.DoChase(player);
                break;
            case Actions.Evade:
                evade.DoEvade(player);
                break;
            default:
                Debug.LogError("No correct action is assigned.");
                break;
        }

        if (isPinch())
        {
            brain.ReTrain(dataForBrain, new float[] { 0.4f, 0.1f, 0.5f});
        }
        if (hp <= 0)
        {
            Killed();
        }
        if (isChance())
        {
            brain.ReTrain(dataForBrain, new float[] { 0.1f, 0.8f, 0.1f });
        }

        attack = gameController.UnitAttack(this);
    }

    public void Damaged(float damage)
    {
        hp -= damage;
    }

    private void Killed()
    {
        gameController.DestroyUnit(this);
    }

    private bool isPinch()
    {
        if (hp <= 0)
        {
            return true;
        }
        return false;
    }

    private bool isChance()
    {
        return attack;
    }

    public void SetBrain(Brain brain)
    {
        this.brain = brain;
    }
}
