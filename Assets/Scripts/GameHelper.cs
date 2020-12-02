using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum TypeAction
{
    INCREASE = 0,
    DECREASE = 1,
    EXPLOSION = 2
}
public class GameHelper : MonoBehaviour
{
    public float BoxSize;               // вместимость
    public float percentWin;            //процент вместимости для победы
    public float timerCoroutine;        //время ожирания корутины
    public float timerSpawnTarget;      // пауза при спавне Target
    public int maxSpawnTarget;         // максимальное кол-во Target
    public int spawnTargetOneMoument;


    public GameObject cattleCorral;     // загон
    public GameObject pasture;          // пастбище
    public Animator gateAnimator;       // аниматор забора
    public GameObject panelRestart;     // панель проигрыша
    public GameObject panelVin;         //панель победы
    public List<Target> targets;        //Trgets

    public bool folow;                  // двигаемся?
    public bool endGame = false;        // конец игры

    private float squareTargets;        //сумарная площадь Targets
    private MeshFilter meshCattleCorral;// загон
    private MeshFilter meshpasture;     // пастбище
    private GameObjectPool objectPool;  //pool
    private int spawnTarget;
    private float secondgametime;


    private void Awake()
    {
        squareTargets = 0;
        folow = false;
        objectPool = GetComponent<GameObjectPool>();
        meshCattleCorral = cattleCorral.GetComponent<MeshFilter>();
        meshpasture = pasture.GetComponent<MeshFilter>();
    }

    private void Start()
    {
        gateAnimator.SetBool("isOpened", false);

        for (int i = 0; i < objectPool.caches.Length; i++)
        {
            objectPool.OnInitialized();
        }

        StartGame();

    }

    private void Update()
    {
        if (!endGame)
        {
            int i = 0;
            while (i < Input.touchCount)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Stationary || Input.GetTouch(i).phase == TouchPhase.Moved ||
                    Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    folow = true;
                    if (!gateAnimator.GetBool("isOpened"))
                    {
                        gateAnimator.SetBool("isOpened", true);
                    }
                }
                else if (Input.GetTouch(i).phase == TouchPhase.Ended)
                {
                    folow = false;
                    if (gateAnimator.GetBool("isOpened"))
                    {
                        gateAnimator.SetBool("isOpened", false);
                    }
                    StartCoroutine(WaitClosedGate());
                }
                ++i;
            }
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                folow = true;
                if (!gateAnimator.GetBool("isOpened"))
                {
                    gateAnimator.SetBool("isOpened", true);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                folow = false;
                if (gateAnimator.GetBool("isOpened"))
                {
                    gateAnimator.SetBool("isOpened", false);
                }
                StartCoroutine(WaitClosedGate());
            }
#endif

            if (folow && maxSpawnTarget > spawnTarget)
            {
                secondgametime += Time.deltaTime;
                if (secondgametime >= timerSpawnTarget)
                {
                    for(int t = 0; t < spawnTargetOneMoument; t++)
                    {
                        GameObject ob = GameObjectPool.Spawn(objectPool.caches[Random.Range(0, objectPool.caches.Length)].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
                        NavMeshAgent navMeshAgent = ob.GetComponent<NavMeshAgent>();
                        navMeshAgent.SetDestination(GetRandomPos(cattleCorral, cattleCorral.GetComponent<MeshFilter>()));
                        Animator animator = ob.GetComponent<Animator>();
                        animator.SetInteger("animation", 1);
                        animator.speed = navMeshAgent.speed + ob.GetComponent<Target>().SpedAnimationTarget;
                        secondgametime = 0;
                        spawnTarget++;
                    }
                    
                }
            }

        }

    }

    private void StartGame()
    {
        for (int i = 0; i < objectPool.caches.Length; i++)
        {
            GameObjectPool.Spawn(objectPool.caches[i].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
            spawnTarget++;
        }
    }

    private void StopGame()
    {

        foreach (Target target in targets)
        {
            if(target.typeAction != TypeAction.EXPLOSION)
            {
                squareTargets += target.square;
            }
            target.Action();
        }

        float temp = BoxSize * percentWin;
        float percent = temp;

        if (squareTargets >= percent && squareTargets <= BoxSize)
        {
            StartCoroutine(WaitDrawUI(panelVin));
        }
        else
        {
            StartCoroutine(WaitDrawUI(panelRestart));
        }

        while (squareTargets > BoxSize)
        {
            int t = Random.Range(0, targets.Count);
            squareTargets -= targets[t].square;
            targets[t].KickOut();
            targets.RemoveAt(t);
        }
        endGame = true;
    }

    IEnumerator WaitClosedGate()
    {
        yield return new WaitForSeconds(timerCoroutine);
        StopGame();
    }
    IEnumerator WaitDrawUI(GameObject gameObject)
    {
        yield return new WaitForSeconds(timerCoroutine);
        gameObject.SetActive(true);
    }

    public Vector3 GetRandomPos(GameObject @object, MeshFilter mesh)
    {
        List<Vector3> VerticeList = new List<Vector3>(mesh.sharedMesh.vertices);
        Vector3 leftTop = @object.transform.TransformPoint(VerticeList[0]);
        Vector3 rightTop = @object.transform.TransformPoint(VerticeList[10]);
        Vector3 leftBottom = @object.transform.TransformPoint(VerticeList[110]);
        Vector3 rightBottom = @object.transform.TransformPoint(VerticeList[120]);
        Vector3 XAxis = rightTop - leftTop;
        Vector3 ZAxis = leftBottom - leftTop;
        Vector3 RndPointonPlane = leftTop + XAxis * Random.value + ZAxis * Random.value;

        return RndPointonPlane;
    }

    public void Restart()
    {
        panelRestart.SetActive(false);
        panelVin.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
