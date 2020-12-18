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
    EXPLOSION = 2,
    SPECIAL = 3
}
public class GameHelper : MonoBehaviour
{
    private int levelID;
    //private int stageID;
    private int stars;

    public float percentWin;            //процент вместимости для победы
    public float timerCoroutine;        //время ожирания корутины
    public float timerSpawnTarget;      // пауза при спавне Target
    public int maxSpawnTarget;         // максимальное кол-во Target
    public int spawnTargetOneMoument;   //кол-во объектов за один спавн
    public float percentSpawnExplosion;  //процент спавна
    public float percentSpawnSpecial;
    public float _timeToNormal;          // время появления окна
    public float _tempScale;             // сила увеличения окна
    public float waitStartGame;          //ожидание старта после нажатия кнопки
    public float waitEndGame;             //ожидание окна после анимации животных
    public int maxKick;
    public int persecntToDestroy = 50;


    public GameObject background;
    public GameObject panelRestart;     // панель проигрыша
    public GameObject panelWin;         //панель победы
    public bool dravTutorial;
    public GameObject panelTutorial;       //панель следующего стейджа
                                           // public GameObject panelEndGameWin;     //панель конца уровня
    public GameObject panelEndGameFail;  //панель конца уровня
    public GameObject panelPRESS;
    public LevelHelper levelHelper;     //конструктор уровня
                                        // public Slider slider;

    Animator gateAnimator;

    public Text levelText;

    public bool spawnInStartGame;        //создавать ли Target при старте
    private bool folow;                  // двигаемся?
    private bool isPlayGame = false;        // конец игры

    public Сorral corral;
    public List<GameObject> gates;
    public List<MeshFilter> pastures;

    public List<Target> targets;
    public List<GameObject> targetsKick;


    private List<GameObject> cattleCorral = new List<GameObject>();
    private float squareTargets;        //сумарная площадь Targets
    private List<MeshFilter> meshCattleCorral = new List<MeshFilter>();
    private GameObject pasture;
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
    GameObject ob;


    private void Awake()
    {
        starsFromStage = 0;
        isPlayGame = false;
        squareTargets = 0;
        folow = false;
        objectPool = GetComponent<GameObjectPool>();

        stars = 3;

        for (int c = 0; c < gates.Count; c++)
        {
            gateAnimator = gates[c].GetComponent<Animator>();
            gateAnimator.SetBool("isOpened", false);
        }

        

        isPlayGame = false;
        squareTargets = 0;

        levelID = levelHelper.levelID;
        levelText.text = (SceneManager.GetActiveScene().buildIndex + 1).ToString();

        LoadStage();

    }

    private void Start()
    {
        if (dravTutorial)
        {
            background.SetActive(true);
            StartCoroutine(WaitDrawUI(panelTutorial));
        }
        else { ButtonStartGame(); }

    }

    int rndPartPlace;
    List<NavMeshAgent> allSheeps = new List<NavMeshAgent>();
    NavMeshAgent navMeshAgent;
    Target target;
    Animator animator;

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
                    panelPRESS.SetActive(false);
                    folow = true;
                    for (int c = 0; c < gates.Count; c++)
                    {
                        gateAnimator = gates[c].GetComponent<Animator>();
                        gateAnimator.SetBool("isOpened", true);
                    }
                    StartCoroutine(DelayBetweenGateAndSpawn());
                    //StartCoroutine(corral.StartNormalizer(false));

                }
                else if (Input.GetTouch(i).phase == TouchPhase.Ended)
                {
                    folow = false;
                    for (int c = 0; c < gates.Count; c++)
                    {
                        gateAnimator = gates[c].GetComponent<Animator>();
                        gateAnimator.SetBool("isOpened", false);
                    }

                    //StartCoroutine(corral.StartNormalizer(true));
                    StartCoroutine(WaitClosedGate());
                }
                ++i;
            }
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                folow = true;
                panelPRESS.SetActive(false);
                for (int c = 0; c < gates.Count; c++)
                {
                    gateAnimator = gates[c].GetComponent<Animator>();
                    gateAnimator.SetBool("isOpened", true);
                }
                StartCoroutine(DelayBetweenGateAndSpawn());
                //StartCoroutine(corral.StartNormalizer(false));
            }
            else if (Input.GetMouseButtonUp(0))
            {
                folow = false;
                for (int c = 0; c < gates.Count; c++)
                {
                    gateAnimator = gates[c].GetComponent<Animator>();
                    gateAnimator.SetBool("isOpened", false);
                }
                
                //StartCoroutine(corral.StartNormalizer(true));
                StartCoroutine(WaitClosedGate());

                for(int k = 0; k < allSheeps.Count; k++)
                {
                    allSheeps[k].speed = 0f;
                }
            }
#endif

            if (folow && maxSpawnTarget > spawnTarget & delayBetweenGateAndSpawn)
            {
                secondgametime += Time.deltaTime;
                if (secondgametime >= timerSpawnTarget)
                {

                    for (int t = 0; t < spawnTargetOneMoument; t++)
                    {
                        int rndPast = Random.Range(0, pastures.Count);
                        pasture = pastures[rndPast].gameObject;
                        meshpasture = pastures[rndPast];

                        int rndExplosion = Random.Range(0, 100);
                        int rndSpecial = Random.Range(0, 100);
                        if (rndSpecial < percentSpawnSpecial)
                        {
                            for (int o = 0; o < objectPool.caches.Count; o++)
                            {
                                if (objectPool.caches[o].typeAction == TypeAction.SPECIAL)
                                {
                                    ob = GameObjectPool.Spawn(objectPool.caches[o].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
                                    break;
                                }
                            }
                        }
                        else if (rndExplosion < percentSpawnSpecial & rndExplosion < percentSpawnExplosion + percentSpawnSpecial)
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
                            x: int rndTemp = Random.Range(0, objectPool.caches.Count);
                            if (objectPool.caches[rndTemp].typeAction != TypeAction.EXPLOSION & objectPool.caches[rndTemp].typeAction != TypeAction.SPECIAL)
                            {
                                ob = GameObjectPool.Spawn(objectPool.caches[rndTemp].prefab, GetRandomPos(pasture, meshpasture), Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
                            }
                            else goto x;
                        }
                        allSheeps.Add(ob.GetComponent<NavMeshAgent>());
                        navMeshAgent = ob.GetComponent<NavMeshAgent>();
                        target = ob.GetComponent<Target>();
                        navMeshAgent.enabled = true;
                        rndPartPlace = Random.Range(0, cattleCorral.Count);
                        navMeshAgent.SetDestination(GetRandomPos(cattleCorral[rndPartPlace], meshCattleCorral[rndPartPlace]));
                        animator = ob.GetComponent<Animator>();
                        animator.SetInteger("animation", 1);
                        animator.speed = navMeshAgent.speed + target.spedAnimationTarget;
                        target.SetPasture(pasture, meshpasture);
                        targetSpawned.Add(ob);
                        secondgametime = 0;
                        spawnTarget++;
                    }

                }
            }

        }

    }
    bool delayBetweenGateAndSpawn = false;
    IEnumerator DelayBetweenGateAndSpawn()
    {
        yield return new WaitForSeconds(0.5f);
        delayBetweenGateAndSpawn = true;
    }

    public void ButtonStartGame()
    {

        if (dravTutorial)
        {

            StartCoroutine(WaitClosedUI(panelTutorial));
            background.SetActive(false);
            dravTutorial = false;
        }
        panelPRESS.SetActive(true);
        StartCoroutine(WaitStartGame());
    }

    public void NextStge()
    {
        squareTargets = 0;
        spawnTarget = 0;
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
        if (dravTutorial) StartCoroutine(WaitDrawUI(panelTutorial));
        else { ButtonStartGame(); }

        StartCoroutine(WaitClosedUI(panelWin));
    }

    public void LoadStage()
    {
        GameObjectPool.ObjectCache oc;

        int task = 0;

        cattleCorral = corral.cattleCorral;
        meshCattleCorral = corral.meshCattleCorral;
        VerticeList = new List<Vector3>(meshCattleCorral[rndPartPlace].sharedMesh.vertices);


        if (levelHelper.randomTask) task = Random.Range(0, levelHelper.tasks.Length);
        else
        {
            for (int t = 0; t < levelHelper.tasks.Length; t++)
            {
                if (levelHelper.tasks[t].taskID == t) task = t;
            }
        }

        for (int o = 0; o < levelHelper.tasks[task].targets.Length; o++)
        {

            oc = new GameObjectPool.ObjectCache();
            oc.prefab = levelHelper.tasks[task].targets[o].prefab;
            oc.cacheSize = levelHelper.tasks[task].targets[o].cacheSize;
            oc.typeAction = levelHelper.tasks[task].targets[o].typeAction;
            oc.square = levelHelper.tasks[task].targets[o].square;
            oc.minSpedTarget = levelHelper.tasks[task].targets[o].minSpedTarget;
            oc.maxSpedTarget = levelHelper.tasks[task].targets[o].maxSpedTarget;
            oc.scaledPower = levelHelper.tasks[task].targets[o].scaledPower;
            objectPool.caches.Add(oc);
        }


        objectPool.OnInitialized();


    }

    private void StopGame()
    {
        float squareTemp = 0f;

        for (int i = 0; i < targetsKick.Count; i++)
        {
            if (i < maxKick)
            {
                if (Random.Range(0, 100) < persecntToDestroy)
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
        while (squareTemp > levelHelper.coralSize)
        {
            int t = Random.Range(0, targets.Count);
            squareTemp -= targets[t].square;
            targets[t].KickOut();
            targets.RemoveAt(t);
        }
        StartCoroutine(WaitEndGame());
    }

    IEnumerator WaitEndGame()
    {
        if (targets.Count == 0) waitEndGame = 0;

        yield return new WaitForSeconds(waitEndGame);

        float temp = levelHelper.coralSize * percentWin;
        float percent = temp;

        if (squareTargets >= percent && squareTargets <= levelHelper.coralSize)
        {
            if (PlayerPrefs.GetInt("Level") <= levelID)
            {
                PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
                PlayerPrefs.Save();
            }

            StartCoroutine(WaitDrawUI(panelWin));

        }
        else
        {
            StartCoroutine(WaitDrawUI(panelRestart));
        }
        isPlayGame = false;
    }

    IEnumerator WaitStartGame()
    {
        yield return new WaitForSeconds(waitStartGame);
        isPlayGame = true;
    }

    IEnumerator WaitClosedGate()
    {
        isPlayGame = false;
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
        Vector3 RndPointonPlane = cattleCorral[rndPartPlace].transform.TransformPoint(VerticeList[index]);
        inverse = !inverse;
        intexTemp++;
        if (intexTemp >= VerticeList.Count)
        {
            intexTemp = 0;
        }
        return RndPointonPlane;
    }

    public void StartNextLevel()
    {
        if (SceneManager.sceneCountInBuildSettings != SceneManager.GetActiveScene().buildIndex + 1)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else SceneManager.LoadScene(0);
    }

    public void Restart()
    {

        spawnTarget = 0;
        levelID = levelHelper.levelID;


        if (panelRestart.activeSelf)
        {
            StartCoroutine(WaitClosedUI(panelRestart));
        }
        else if (panelWin.activeSelf)
        {
            StartCoroutine(WaitClosedUI(panelWin));
        }

        isPlayGame = false;
        
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

        if (dravTutorial) StartCoroutine(WaitDrawUI(panelTutorial));
        else { ButtonStartGame(); }
        squareTargets = 0;
        delayBetweenGateAndSpawn = false;
    }
}
