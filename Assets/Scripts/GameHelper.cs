using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public enum TypeAction
{
    INCREASE = 0,
    DECREASE = 1,
    EXPLOSION = 2
}
public class GameHelper : MonoBehaviour
{
    public int levelID;
    public int stageID;
    public int stars;

    public float percentWin;            //процент вместимости для победы
    public float timerCoroutine;        //время ожирания корутины
    public float timerSpawnTarget;      // пауза при спавне Target
    public int maxSpawnTarget;         // максимальное кол-во Target
    public int spawnTargetOneMoument;   //кол-во объектов за один спавн
    public float percentSpawnExplosion;  //процент спавна
    public float _timeToNormal;          // время появления окна
    public float _tempScale;             // сила увеличения окна
    public float waitStartGame;          //ожидание старта после нажатия кнопки
    public float waitEndGame;             //ожидание окна после анимации животных
    public int maxKick;
    public int persecntToDestroy = 50;


    public GameObject pasture;          // пастбище спавна Target
    public Animator gateAnimator;       // аниматор забора
    public GameObject panelRestart;     // панель проигрыша
    public GameObject panelWin;         //панель победы
    public GameObject panelStage;       //панель следующего стейджа
    public GameObject panelEndGameWin;     //панель конца уровня
    public GameObject panelEndGameFail;  //панель конца уровня
    public LevelHelper levelHelper;     //конструктор уровня
    public Slider slider;

    public Text levelText;
    public Text stageText;
    public Text coralSizeText;
    //public Text allStarsText;
    //public Text starsText;

    public bool spawnInStartGame;        //создавать ли Target при старте
    private bool folow;                  // двигаемся?
    private bool isPlayGame = false;        // конец игры

    public List<Сorral> corrals;
    public List<Target> targets;
    public List<GameObject> targetsKick;


    private GameObject cattleCorral;
    private float squareTargets;        //сумарная площадь Targets
    private MeshFilter meshCattleCorral;
    private MeshFilter meshpasture;
    private GameObjectPool objectPool;
    private int spawnTarget;
    private float secondgametime;
    private List<Vector3> VerticeList;
    private int intexTemp;
    private bool inverse;
    public List<GameObject> targetSpawned;
    public float starsFromStage;

    public List<GameObject> _allStars = new List<GameObject>();
    public List<GameObject> _allStarsEnd = new List<GameObject>();
    public float _delayBetweenStars = 1f;
    public float _timeToMaxStar = 0.5f;
    public float _timeToNormalStar = 0.5f;
    public float _scaleMaxStar = 1.1f;


    private void Awake()
    {
        starsFromStage = 0;
        isPlayGame = false;
        squareTargets = 0;
        folow = false;
        objectPool = GetComponent<GameObjectPool>();
        meshpasture = pasture.GetComponent<MeshFilter>();

    }

    private void Start()
    {
        stars = 3;
        gateAnimator.SetBool("isOpened", false);

        int IDLevel = PlayerPrefs.GetInt("Level") + 1;
        levelText.text = IDLevel.ToString();
        stageID = levelHelper.stages[0].stageID;
        stageText.text = levelHelper.stages[0].stageID.ToString();
        coralSizeText.text = levelHelper.stages[0].coralSize.ToString();
        slider.maxValue = levelHelper.stages.Length;
        StartCoroutine(WaitDrawUI(panelStage));

    }

    private void Update()
    {
        if (isPlayGame)
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

                    for (int t = 0; t < spawnTargetOneMoument; t++)
                    {
                        GameObject ob = new GameObject();
                        int rnd = Random.Range(0, 100);
                        if (rnd <= percentSpawnExplosion)
                        {
                            for (int o = 0; o < objectPool.caches.Count; o++)
                            {
                                if (objectPool.caches[o].typeAction == TypeAction.EXPLOSION)
                                {
                                    ob = GameObjectPool.Spawn(objectPool.caches[o].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            int rnd_2 = Random.Range(0, 100 - (int)percentSpawnExplosion);
                            if (rnd_2 < (100 - percentSpawnExplosion) / 2)
                            {
                                for (int o = 0; o < objectPool.caches.Count; o++)
                                {
                                    if (objectPool.caches[o].typeAction == TypeAction.DECREASE)
                                    {
                                        ob = GameObjectPool.Spawn(objectPool.caches[o].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int o = 0; o < objectPool.caches.Count; o++)
                                {
                                    if (objectPool.caches[o].typeAction == TypeAction.INCREASE)
                                    {
                                        ob = GameObjectPool.Spawn(objectPool.caches[o].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
                                        break;
                                    }
                                }
                            }
                        }
                        NavMeshAgent navMeshAgent = ob.GetComponent<NavMeshAgent>();
                        navMeshAgent.SetDestination(GetRandomPos(cattleCorral, meshCattleCorral));
                        //Debug.Log(navMeshAgent.gameObject.name);
                        Animator animator = ob.GetComponent<Animator>();
                        animator.SetInteger("animation", 1);
                        animator.speed = navMeshAgent.speed + ob.GetComponent<Target>().SpedAnimationTarget;
                        targetSpawned.Add(ob);
                        secondgametime = 0;
                        spawnTarget++;
                    }

                }
            }

        }

    }

    public void ButtonStartGame()
    {
        objectPool.caches.Clear();
        isPlayGame = false;
        squareTargets = 0;
        folow = false;
        if (targetSpawned.Count != 0)
        {
            for (int i = 0; i < targetSpawned.Count; i++)
            {
                if (targetSpawned[i].gameObject != null)
                    GameObjectPool.Unspawn(targetSpawned[i].gameObject);
            }
            targetSpawned.Clear();
            targets.Clear();
        }

        levelID = levelHelper.levelID;

        LoadStage(stageID);
        StartCoroutine(WaitClosedUI(panelStage));
        if (spawnInStartGame)
        {
            for (int i = 0; i < objectPool.caches.Count; i++)
            {
                GameObject ob = GameObjectPool.Spawn(objectPool.caches[i].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
                targetSpawned.Add(ob);
                spawnTarget++;
            }
        }
        StartCoroutine(WaitStartGame());
    }

    public void NextStge()
    {
        if (stageID != levelHelper.stages.Length)
        {
            stageID += 1;
            for (int i = 0; i < levelHelper.stages.Length; i++)
            {
                if (levelHelper.stages[i].stageID == stageID)
                {
                    float temp = levelHelper.stages[i].coralSize;
                    stageText.text = levelHelper.stages[i].stageID.ToString();
                    coralSizeText.text = levelHelper.stages[i].coralSize.ToString();
                }
            }
            StartCoroutine(WaitDrawUI(panelStage));
        }
        else
        {
            //allStarsText.text = stars.ToString();
            StartCoroutine(WaitDrawUI(panelEndGameWin));
        }
        StartCoroutine(WaitClosedUI(panelWin));
    }

    public void LoadStage(int stage)
    {

        for (int i = 0; i < levelHelper.stages.Length; i++)
        {

            if (levelHelper.stages[i].stageID == stage)
            {
                int task = 0;
                for (int c = 0; c < corrals.Count; c++)
                {
                    if (corrals[c].coralID == levelHelper.stages[i].coralID)
                    {
                        cattleCorral = corrals[c].cattleCorral;
                        meshCattleCorral = corrals[c].meshCattleCorral;
                        VerticeList = new List<Vector3>(meshCattleCorral.sharedMesh.vertices);
                    }
                }

                if (levelHelper.stages[i].randomTask) task = Random.Range(0, levelHelper.stages[i].tasks.Length);
                else
                {
                    for (int t = 0; t < levelHelper.stages[i].tasks.Length; t++)
                    {
                        if (levelHelper.stages[i].tasks[t].taskID == t) task = t;
                    }
                }

                for (int o = 0; o < levelHelper.stages[i].tasks[task].targets.Length; o++)
                {
                    GameObjectPool.ObjectCache oc = new GameObjectPool.ObjectCache();
                    oc.prefab = levelHelper.stages[i].tasks[task].targets[o].prefab;
                    oc.cacheSize = levelHelper.stages[i].tasks[task].targets[o].cacheSize;
                    oc.typeAction = levelHelper.stages[i].tasks[task].targets[o].typeAction;
                    oc.square = levelHelper.stages[i].tasks[task].targets[o].square;
                    oc.minSpedTarget = levelHelper.stages[i].tasks[task].targets[o].minSpedTarget;
                    oc.maxSpedTarget = levelHelper.stages[i].tasks[task].targets[o].maxSpedTarget;
                    oc.scaledPower = levelHelper.stages[i].tasks[task].targets[o].scaledPower;
                    objectPool.caches.Add(oc);
                }
            }
        }
        for (int i = 0; i < objectPool.caches.Count; i++)
        {
            objectPool.OnInitialized();
        }

    }

    private void StopGame()
    {
        float squareTemp = 0f;

        for(int i = 0; i < targetsKick.Count; i++)
        {
            if(i < maxKick)
            {
                if(Random.Range(0, 100) < persecntToDestroy)
                {
                    //Debug.Log("Unspawn " + targetsKick[i].name);
                    targets.Remove(targetsKick[i].GetComponent<Target>());
                    targetsKick[i].GetComponent<Target>().KickOut();
                }
            }
        }

        foreach (Target target in targets)
        {
            if (target.typeAction != TypeAction.EXPLOSION)
            {
                squareTargets += target.square;
            }
            target.Action();
        }
       
        squareTemp = squareTargets;
        for (int i = 0; i < levelHelper.stages.Length; i++)
        {
            if (levelHelper.stages[i].stageID == stageID)
            {
                while (squareTemp > levelHelper.stages[i].coralSize)
                {
                    int t = Random.Range(0, targets.Count);
                    squareTemp -= targets[t].square;
                    targets[t].KickOut();
                    targets.RemoveAt(t);
                }
            }
        }
        StartCoroutine(WaitEndGame());
    }

    IEnumerator WaitEndGame()
    {

        yield return new WaitForSeconds(waitEndGame);

        for (int i = 0; i < levelHelper.stages.Length; i++)
        {
            if (levelHelper.stages[i].stageID == stageID)
            {
                float temp = levelHelper.stages[i].coralSize * percentWin;
                float percent = temp;

                if (squareTargets >= percent && squareTargets <= levelHelper.stages[i].coralSize)
                {

                    if (i != levelHelper.stages.Length)
                    {
                        //starsText.text = stars.ToString();
                        StartCoroutine(WaitDrawUI(panelWin));
                    }
                    else
                    {
                        //allStarsText.text = stars.ToString();
                        StartCoroutine(WaitDrawUI(panelEndGameWin));
                    }
                    slider.value = stageID;
                }
                else
                {
                    StartCoroutine(WaitDrawUI(panelRestart));
                }
                isPlayGame = false;
                break;
            }
        }
    }

    IEnumerator WaitStartGame()
    {
        yield return new WaitForSeconds(waitStartGame);
        isPlayGame = true;
    }

    IEnumerator WaitClosedGate()
    {
        yield return new WaitForSeconds(timerCoroutine);
        StopGame();
    }
    IEnumerator WaitDrawUI(GameObject gameObject)
    {
        gameObject.SetActive(true);
        gameObject.transform.DOScale(_tempScale, _timeToNormal);
        for (int i = 0; i < _allStars.Count; i++)
        {
            _allStars[i].transform.DOScale(0f, 0f);
            _allStarsEnd[i].transform.DOScale(0f, 0f);
        }
        yield return new WaitForSeconds(_timeToNormal);
        if (gameObject == panelWin)
        {
            for (int i = 0; i < _allStars.Count; i++)
            {
                yield return new WaitForSeconds(_delayBetweenStars);
                _allStars[i].transform.DOScale(_scaleMaxStar, _timeToMaxStar);
                yield return new WaitForSeconds(_timeToMaxStar);
                _allStars[i].transform.DOScale(1f, _timeToNormalStar);
                yield return new WaitForSeconds(_timeToNormalStar);
            }
        }
        if (gameObject == panelEndGameWin)
        {
            PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
            PlayerPrefs.Save();
            for (int i = 0; i < _allStarsEnd.Count; i++)
            {
                yield return new WaitForSeconds(_delayBetweenStars);
                _allStarsEnd[i].transform.DOScale(_scaleMaxStar, _timeToMaxStar);
                yield return new WaitForSeconds(_timeToMaxStar);
                _allStarsEnd[i].transform.DOScale(1f, _timeToNormalStar);
                yield return new WaitForSeconds(_timeToNormalStar);
            }
        }

    }
    IEnumerator WaitClosedUI(GameObject gameObject)
    {

        gameObject.transform.DOScale(0, _timeToNormal);
        yield return new WaitForSeconds(_timeToNormal);
        gameObject.SetActive(false);

    }

    public Vector3 GetRandomPos(GameObject @object, MeshFilter mesh)
    {
        List<Vector3> Vertice = new List<Vector3>(mesh.sharedMesh.vertices);
        Vector3 leftTop = @object.transform.TransformPoint(Vertice[0]);
        Vector3 rightTop = @object.transform.TransformPoint(Vertice[10]);
        Vector3 leftBottom = @object.transform.TransformPoint(Vertice[110]);
        Vector3 rightBottom = @object.transform.TransformPoint(Vertice[120]);
        Vector3 XAxis = rightTop - leftTop;
        Vector3 ZAxis = leftBottom - leftTop;
        Vector3 RndPointonPlane = leftTop + XAxis * Random.value + ZAxis * Random.value;
        //Debug.Log(RndPointonPlane);
        return RndPointonPlane;
    }

    public Vector3 GetRandomPosGate()
    {
        int index = intexTemp;
        if (inverse)
        {
            index = VerticeList.Count - intexTemp;
            index -= 1;
        }
        Vector3 RndPointonPlane = cattleCorral.transform.TransformPoint(VerticeList[index]);
        inverse = !inverse;
        intexTemp++;
        if (intexTemp >= VerticeList.Count)
        {
            intexTemp = 0;
        }
        return RndPointonPlane;
    }

    public void Restart()
    {

        if (panelRestart.activeSelf)
        {
            StartCoroutine(WaitClosedUI(panelRestart));
        }
        else if (panelWin.activeSelf)
        {
            StartCoroutine(WaitClosedUI(panelWin));
        }
        else if (panelEndGameWin.activeSelf)
        {
            StartCoroutine(WaitClosedUI(panelEndGameWin));
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
}
