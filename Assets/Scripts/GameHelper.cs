using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameHelper : MonoBehaviour
{
    public float BoxSize;               // вместимость
    public float percentWin;            //процент вместимости для победы
    public float timerCoroutine;        //время ожирания корутины
    public float timerSpawnTarget;      // пауза при спавне Target
    public int maxTrgetInScene;         // максимальное кол-во Target

    public GameObject cattleCorral;     // загон
    public GameObject pasture;          // пастбище
    public Animator gateAnimator;       // аниматор забора
    public GameObject panelRestart;     // панель проигрыша
    public GameObject panelVin;         //панель победы
    public List<Target> targets;        //Trgets

    private float secondgametime; 

    public bool folow;                  // двигаемся?
    public bool endGame = false;        // конец игры

    private float squareTargets;        //сумарная площадь Targets
    private MeshFilter meshCattleCorral;// загон
    private MeshFilter meshpasture;     // пастбище
    private GameObjectPool objectPool;  //pool


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
        StartGame();
        objectPool.caches[0].cacheSize = maxTrgetInScene;
        objectPool.OnInitialized();
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
                    StopGame();
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
                StopGame();
            }
#endif

            if (folow)
            {
                secondgametime += Time.deltaTime;
                if (secondgametime >= timerSpawnTarget)
                {
                    GameObject ob =  GameObjectPool.Spawn(objectPool.caches[0].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
                    NavMeshAgent navMeshAgent = ob.GetComponent<NavMeshAgent>();
                    navMeshAgent.SetDestination(GetRandomPos(cattleCorral, cattleCorral.GetComponent<MeshFilter>()));
                    Animator animator = ob.GetComponent<Animator>();
                    animator.SetInteger("animation", 1);
                    animator.speed = navMeshAgent.speed + ob.GetComponent<Target>().SpedAnimationTarget;
                    secondgametime = 0;
                }
            }

        }

    }

    private void StartGame()
    {
        for (int i = 0; i < objectPool.caches.Length; i++)
        {
            for (int o = 0; o < objectPool.caches[i].cacheSize; o++)
            {
                GameObjectPool.Spawn(objectPool.caches[i].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));

            }

        }
    }

    private void StopGame()
    {
        
        foreach (Target target in targets)
        {
            squareTargets += target.square;
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
