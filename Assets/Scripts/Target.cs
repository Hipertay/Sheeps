using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using System.Linq;

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
    public float timeCourotine;
    public SkinnedMeshRenderer skinnedMesh;
    public GameObject prefabParticle;

    public float updatePosition;       //расстояние для обновления позиции

    public bool gravitation;           //гравитация on/off
    public bool flyobject;


    GameHelper gameHelper;
    bool touch;
    Animator animator;
    Rigidbody rigidbody;
    Vector3 randomPosition;
    Vector3 scale;
    bool isPosition;


    public NavMeshAgent Agent { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        isPosition = false;
        touch = false;
        flyobject = false;
        gravitation = true;
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = Random.Range(minSpedTarget, maxSpedTarget);
        animator = GetComponent<Animator>();
        gameHelper = FindObjectOfType<GameHelper>();
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        var look = Quaternion.LookRotation(randomPosition - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, look, spedRotation * Time.deltaTime);
        var heading = randomPosition - transform.position;

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
        if (!isPosition)
        {
            randomPosition = gameHelper.GetRandomPosGate();
            isPosition = true;
        }


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
        animator.SetInteger("animation", 0);
        rigidbody.isKinematic = true;
        switch (typeAction)
        {
            case TypeAction.INCREASE:
                scale = gameObject.transform.localScale * scaledPower;
                StartCoroutine(ScaleTarget(gameObject));
                break;
            case TypeAction.DECREASE:
                scale = gameObject.transform.localScale / scaledPower;
                StartCoroutine(ScaleTarget(gameObject));
                break;
            case TypeAction.EXPLOSION:
                GameObject particle = Instantiate(prefabParticle, transform.position, Quaternion.identity);
                KickOut();
                StartCoroutine(Wait());
                break;
        }
    }

    IEnumerator ScaleTarget(GameObject gameObject)
    {
        gameObject.transform.DOScale(scale, timeCourotine);
        yield return new WaitForSeconds(timeCourotine);

    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(timeCourotine);
    }

    public void KickOut()
    {
            if (!gravitation) rigidbody.useGravity = false;
            Agent.enabled = false;
            flyobject = true;
            rigidbody.AddRelativeForce(new Vector3(Random.Range(minPositionFly, maxPositionFly), Random.Range(minPositionFly, maxPositionFly), Random.Range(minPositionFly, maxPositionFly)), ForceMode.Impulse);
        
    }

    public GameObject TargetSearch()
    {
        var sersh = gameHelper.targets.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).FirstOrDefault();
        return sersh.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            gameHelper.targets.Add(this.gameObject.GetComponent<Target>());
            Folov();
        }

        if (other.CompareTag("Explosion") & !transform.CompareTag("Explosion"))
        {
            if (!gameHelper.targetsKick.Contains(gameObject))
            {
                gameHelper.targetsKick.Add(gameObject);
            }
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Explosion"))
        {
            gameHelper.targetsKick.Remove(gameObject);
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
