using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
    public bool flyobject;
    public float square;              //площадь
    public int scaledPower;           //кратность увеличения
    public float minSpedTarget;       //скорость персонажа min
    public float maxSpedTarget;       //скорость персонажа max
    public float SpedAnimationTarget; //скорость анимации персонажа
    public float spedRotation;        //скорость поворота
    public float minPositionFly;       // min прыжок
    public float maxPositionFly;       // max прыжок
    public bool gravitation;           //гравитация on/off
    public float updatePosition;       //расстояние для обновления позиции

    GameHelper gameHelper;
    bool onGate;
    bool scaled;
    bool touch;
    MeshFilter meshFilter;
    Animator animator;
    Rigidbody rigidbody;
    Vector3 randomPosition;


    public NavMeshAgent Agent { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        touch = false;
        scaled = false;
        flyobject = false;
        gravitation = true;
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = Random.Range(minSpedTarget, maxSpedTarget);
        animator = GetComponent<Animator>();
        gameHelper = FindObjectOfType<GameHelper>();
        meshFilter = gameHelper.cattleCorral.GetComponent<MeshFilter>();
        rigidbody = GetComponent<Rigidbody>();
        onGate = false;
    }

    // Update is called once per frame
    void Update()
    {
        var look = Quaternion.LookRotation(randomPosition - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, look, spedRotation * Time.deltaTime);
        var heading = randomPosition - transform.position;
        if (heading.sqrMagnitude <= updatePosition)
        {
            randomPosition = gameHelper.GetRandomPos(gameHelper.cattleCorral, meshFilter);
            if (Agent.enabled)
            {
                Agent.SetDestination(randomPosition);
                Agent.isStopped = false;
                Agent.speed = minSpedTarget;
            }
        }
        if (!touch)
        {
            int i = 0;
            while (i < Input.touchCount)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Stationary || Input.GetTouch(i).phase == TouchPhase.Moved ||
                    Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    Folov();
                }
                else if (Input.GetTouch(i).phase == TouchPhase.Ended)
                {
                    Stay();
                    if (onGate)
                        Scaled(scaledPower);
                    touch = true;
                }
                ++i;
            }
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                Folov();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Stay();
                if (onGate)
                    Scaled(scaledPower);
                touch = true;
            }
#endif
        }
    }

    public void Folov()
    {
       randomPosition = gameHelper.GetRandomPos(gameHelper.cattleCorral, meshFilter);
        if (Agent.enabled)
        {
            Agent.SetDestination(randomPosition);
            
            Agent.isStopped = false;
        }

        animator.SetInteger("animation", 1);
        animator.speed = Agent.speed + SpedAnimationTarget;

    }

    public void Stay()
    {
        animator.SetInteger("animation", 0);
        if (Agent.enabled)
            Agent.isStopped = true;
    }

    public void Scaled(int i)
    {
        if (!scaled)
        {
            this.gameObject.transform.localScale = gameObject.transform.localScale * i;
            scaled = true;
        }
    }
        

    public void KickOut()
    {
        if(!gravitation) rigidbody.useGravity = false;
        Agent.enabled = false;
        flyobject = true;
        rigidbody.AddRelativeForce(new Vector3(Random.Range(minPositionFly, maxPositionFly), maxPositionFly, Random.Range(minPositionFly, maxPositionFly)), ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            gameHelper.targets.Add(this.gameObject.GetComponent<Target>());
            onGate = true;
            Folov();
        }
    }

   






}
