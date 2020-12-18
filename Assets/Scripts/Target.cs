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
    public float spedTargetOnCloseGate;       //скорость персонажа после закрытия ворот
    public float spedAnimationTarget; //скорость анимации персонажа
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
    bool inGate;
    GameObject pasture;
    MeshFilter meshpasture;

    Vector3 startScale = Vector3.zero;


    public NavMeshAgent Agent { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        startScale = transform.localScale;
        inGate = false;
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

    private void OnEnable()
    {
        if(startScale != Vector3.zero)
        {
            Agent.speed = Random.Range(minSpedTarget, maxSpedTarget);
            transform.localScale = startScale;
            Agent.enabled = true;
            isAction = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

        //var look = Quaternion.LookRotation(randomPosition - transform.position);
        //transform.rotation = Quaternion.Lerp(transform.rotation, look, spedRotation * Time.deltaTime);
        //if(isAction)
        //{
        //    transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
        //}
        //var heading = randomPosition - transform.position;

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
        if (!isAction)
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
            animator.speed = Agent.speed + spedAnimationTarget;
        }
        else
        {
            animator.SetInteger("animation", 0);
            Agent.enabled = false;
        }

    }

    public void Stay()
    {
        if (inGate)
        {
            animator.SetInteger("animation", 0);
            if (Agent.enabled)
                Agent.isStopped = true;
        }
        else
        {
            if (Agent.enabled)
            {
                Agent.SetDestination(gameHelper.GetRandomPos(pasture, meshpasture));
                
            }

            animator.SetInteger("animation", 1);
            Agent.speed = spedTargetOnCloseGate;
            animator.speed = Agent.speed + spedAnimationTarget;
        }
        Agent.speed = 0f;
    }

    bool isAction = false;

    public void Action()
    {
        isAction = true;
        animator.SetInteger("animation", 0);
        Agent.speed = 0f;
        //rigidbody.isKinematic = true;
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
            case TypeAction.SPECIAL:
                scale = gameObject.transform.localScale * scaledPower;
                StartCoroutine(ScaleTarget(gameObject));
                break;
            case TypeAction.EXPLOSION:
                GameObject particle = Instantiate(prefabParticle, transform.position, Quaternion.identity);
                KickOut();
                StartCoroutine(Wait());
                break;
        }
        Agent.enabled = false;
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

    public void SetPasture(GameObject ob, MeshFilter mesh)
    {
        pasture = ob;
        meshpasture = mesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            inGate = true;
            if (!gameHelper.targets.Contains(this.gameObject.GetComponent<Target>()))
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
            if (collision.gameObject.CompareTag("Ground") & isAction)
            {
                if (gravitation) rigidbody.useGravity = true;
                Agent.enabled = true;
                flyobject = false;
            }
        }
    }
}
