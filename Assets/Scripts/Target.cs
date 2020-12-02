using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
    public TypeAction typeAction;
    public float square;              //площадь
    public float scaledPower;           //кратность увеличения/уменьшения
    public float minSpedTarget;       //скорость персонажа min
    public float maxSpedTarget;       //скорость персонажа max
    public float SpedAnimationTarget; //скорость анимации персонажа
    public float spedRotation;        //скорость поворота
    public float minPositionFly;       // min прыжок
    public float maxPositionFly;       // max прыжок

    public float updatePosition;       //расстояние для обновления позиции

    public bool gravitation;           //гравитация on/off
    public bool flyobject;

    GameHelper gameHelper;
    bool touch;
    MeshFilter meshFilter;
    Animator animator;
    Rigidbody rigidbody;
    Vector3 randomPosition;
    Vector3 scale;
    bool isScaled;


    public NavMeshAgent Agent { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        isScaled = false;
        touch = false;
        flyobject = false;
        gravitation = true;
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = Random.Range(minSpedTarget, maxSpedTarget);
        animator = GetComponent<Animator>();
        gameHelper = FindObjectOfType<GameHelper>();
        meshFilter = gameHelper.cattleCorral.GetComponent<MeshFilter>();
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isScaled)
        {
            this.gameObject.transform.localScale = new Vector3(Mathf.Lerp(scale.x, gameObject.transform.localScale.x, 1 * Time.deltaTime), Mathf.Lerp(scale.y, gameObject.transform.localScale.y, 1 * Time.deltaTime), Mathf.Lerp(scale.z, gameObject.transform.localScale.z, 1 * Time.deltaTime));
        }

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

    public void Action()
    {
        switch (typeAction)
        {
            case TypeAction.INCREASE:
                scale = gameObject.transform.localScale * scaledPower;
                isScaled = true;
                break;
            case TypeAction.DECREASE:
                scale = gameObject.transform.localScale / scaledPower;
                isScaled = true;
                break;
            case TypeAction.EXPLOSION:
                Destroy(gameObject);
                break;
        }
    }

    public void KickOut()
    {
        if (!gravitation) rigidbody.useGravity = false;
        Agent.enabled = false;
        flyobject = true;
        rigidbody.AddRelativeForce(new Vector3(Random.Range(minPositionFly, maxPositionFly), maxPositionFly, Random.Range(minPositionFly, maxPositionFly)), ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            gameHelper.targets.Add(this.gameObject.GetComponent<Target>());
            Folov();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (touch)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                if (gravitation) rigidbody.useGravity = true;
                Agent.enabled = true;
                flyobject = false;
            }
        }

    }








}
