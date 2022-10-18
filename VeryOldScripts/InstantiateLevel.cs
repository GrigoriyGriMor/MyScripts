//Очень Древняя писанина, но рабочая :D 
//Класс отвечает за генерацию уровня для 2д раннера. Уровень генерируется так, что объекты не мешают друг другу и игрок всегда может его пройти

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InstantiateLevel : MonoBehaviour
{
    private static InstantiateLevel instance;
    public static InstantiateLevel Instance => instance;

    //управление уровнем
    private enum StateGame { StartGame, PlayGame, StopGame }
    private enum DifficultyLevel { Easy, Medium, Hurd, Impossible }
    private StateGame stateGame;
    private DifficultyLevel difficultyLevel = DifficultyLevel.Easy;

    //типы и слои объектов из которых строится уровень
    private enum ObjType { Ground, UpGround, F_Tree, F_Grass, F_Bush, ActiveBush, Pipe, Platform, Stump, UpStump, Point, Hole, UpHole}
    private enum LayerInGame { interactiveObj, level_1, level_2, level_3 }

    [System.Serializable]
    private struct MaxMinValue
    {
        [Range(0, 100)] public float MinValue;
        [Range(0, 100)] public float MaxValue;
    }

    [SerializeField] private MaxMinValue _treeSpawnTime;
    [SerializeField] private MaxMinValue _grassSpawnTime;
    [SerializeField] private MaxMinValue _bushSpawnTime;
    [SerializeField] private MaxMinValue _pipeSpawnChance;
    [SerializeField] private MaxMinValue _platformSpawnChance;
    [SerializeField] private MaxMinValue _stumpSpawnChance;
    [SerializeField] private MaxMinValue _upStumpSpawnChance;
    [SerializeField] private MaxMinValue _holeSpawnChance;
    [SerializeField] private MaxMinValue _upHoleSpawnChance;

    private float distance = 0.0f;
    [SerializeField] private float spawnIntDistance = 1.0f;


    [HideInInspector] public float moveLevelINTER = 0;
    [HideInInspector] public float moveLevelSECOND = 0;
    [HideInInspector] public float moveLevelFIRST = 0;
    [HideInInspector] public float moveLevelTHIRD = 0;


    [System.Serializable]
    private struct objAtScene
    {
        public GameObject objPrefab;
        public ObjType objType;
        public LayerInGame layer;
    }

    [System.Serializable]
    private struct objInLayer
    {
        public GameObject objPrefab;
        public ObjType objType;

        public objInLayer(GameObject obj, ObjType type)
        {
            objPrefab = obj;
            objType = type;
        }
    }

    private List<objInLayer> interactiveLevelObj = new List<objInLayer>();//создаем общий список по структуре в которой указываем ссылку на ранее созданный объект, а так же его тип и слой
    [SerializeField] private float _InteractiveLSpeedValue = 1;
    [SerializeField] private float _ILSpawnValue = 1; //переменные которые влияют на то, какая будет задержка спавна для каждого уровня
    private List<objInLayer> firstLevelObj = new List<objInLayer>();
    [SerializeField] private float _FirstLSpeedValue = 0.80f;
    [SerializeField] private float _FLSpawnValue = 0.9f;
    private List<objInLayer> secondLevelObj = new List<objInLayer>();
    [SerializeField] private float _SecondLSpeedValue = 0.60f;
    [SerializeField] private float _SLSpawnValue = 0.8f;
    private List<objInLayer> thirdLevelObj = new List<objInLayer>();
    [SerializeField] private float _ThirdLSpeedValue = 0.40f;
    [SerializeField] private float _TLSpawnValue = 0.7f;

    [SerializeField] private float levelObjSpeed = 1;

    [SerializeField] private objAtScene[] assemblyObj = new objAtScene[0];
    [SerializeField] private int cloneValue = 5;
    private float OrthSize = 5;

    [HideInInspector] public float moveSpeed = 0.0f;

    public float spawnPosition = 5.0f;
    [HideInInspector] public float spawnPositionX = 12;

    private int axisValue = 0;//переменная нужна для правильного расчета скорости спавна объектов

    private void Awake()
    {
        instance = this;
        OrthSize = Camera.main.orthographicSize;
        LevelInitialization();
    }

    private void Start()//задаем скорость уровню, причем важно, 0.02f это Time.deltaTime, но почему-то при перезапуске уровня значение меняется
    {
        moveLevelINTER = (levelObjSpeed * _InteractiveLSpeedValue) * 0.02f * -2.5f;
        moveLevelSECOND = (levelObjSpeed * _SecondLSpeedValue) * 0.02f * -2.5f;
        moveLevelFIRST = (levelObjSpeed * _FirstLSpeedValue) * 0.02f * -2.5f;
        moveLevelTHIRD = (levelObjSpeed * _ThirdLSpeedValue) * 0.02f * -2.5f;
    }

    private Vector2 groundSize;
    private Vector2 upGroundSize;//переменные нужны для правильного размещения элементов, которые должны быть раставленны с учетом высоты ground upground
    private float pipeSizeY;
    private float platformSizeY;

    private void LevelInitialization()
    {
        for (int i = 0; i < assemblyObj.Length; i++)
        {
            if (assemblyObj[i].objPrefab != null)
            {
                GameObject GO = assemblyObj[i].objPrefab;
                float goSizeY = 0;
                if (GO.GetComponent<SpriteRenderer>()) goSizeY = GO.GetComponent<SpriteRenderer>().size.y / 2;


                switch (assemblyObj[i].objType)
                {
                    case ObjType.Ground:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go;

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    _go = Instantiate(GO, new Vector3(-spawnPositionX, -OrthSize), Quaternion.identity);
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    groundSize.y = goSizeY;
                                    groundSize.x = _go.GetComponent<SpriteRenderer>().size.x;
                                    break;
                                case LayerInGame.level_1:
                                    _go = Instantiate(GO, new Vector3(-spawnPositionX, -OrthSize + groundSize.y), Quaternion.identity);
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    _go = Instantiate(GO, new Vector3(-spawnPositionX, -OrthSize + groundSize.y * 2), Quaternion.identity);
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.UpGround:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go;

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    _go = Instantiate(GO, new Vector3(-spawnPositionX, OrthSize), Quaternion.identity);

                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);

                                    upGroundSize.y = goSizeY;
                                    upGroundSize.x = _go.GetComponent<SpriteRenderer>().size.x;
                                    break;
                                case LayerInGame.level_1:
                                    _go = Instantiate(GO, new Vector3(-spawnPositionX, OrthSize - upGroundSize.y - goSizeY), Quaternion.identity);
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    _go = Instantiate(GO, new Vector3(-spawnPositionX, OrthSize - upGroundSize.y * 2 - goSizeY * 2), Quaternion.identity);
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }

                        }
                        break;

                    case ObjType.Hole:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, -OrthSize), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.UpHole:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, OrthSize), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.F_Tree:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, 0), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_3:
                                    thirdLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.F_Grass:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, -OrthSize + groundSize.y + goSizeY), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_3:
                                    thirdLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.F_Bush:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, -OrthSize + groundSize.y + goSizeY), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_3:
                                    thirdLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.Stump:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, -OrthSize + groundSize.y + goSizeY), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_3:
                                    thirdLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.UpStump:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, OrthSize - upGroundSize.y - goSizeY), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_3:
                                    thirdLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.Pipe:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, 0), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_3:
                                    thirdLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }

                        pipeSizeY = goSizeY;

                        break;

                    case ObjType.Platform:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, 0), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_3:
                                    thirdLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }

                        platformSizeY = goSizeY;

                        break;

                    case ObjType.Point:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, 0), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    case ObjType.ActiveBush:
                        for (int j = 0; j <= cloneValue; j++)
                        {
                            GameObject _go = Instantiate(GO, new Vector3(-spawnPositionX, OrthSize - goSizeY), Quaternion.identity);

                            switch (assemblyObj[i].layer)
                            {
                                case LayerInGame.interactiveObj:
                                    interactiveLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_1:
                                    firstLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_2:
                                    secondLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                case LayerInGame.level_3:
                                    thirdLevelObj.Add(new objInLayer(_go, assemblyObj[i].objType));
                                    _go.SetActive(false);
                                    break;
                                default:
                                    Debug.LogError(GO.name + " слой объекта не определен");
                                    break;
                            }
                        }
                        break;

                    default:
                        Debug.LogError(GO.name + " тип объекта не определен");
                        break;
                }
            }
        }

        CreateObjInInteractiveLayer();
        CreateObjInFirstLayer();
        CreateObjInSecondLayer();
        CreateObjInThirdLayer();
        SpawnInteractiveLayerObj(ObjType.Pipe);
        System.GC.Collect();//очищаем временную память перед запуском основной игры
    }//создаем пул объектов для уровня

    private GameObject GetFreeObj(List<objInLayer> ObjList, ObjType type)//ищем свободный объект в пуле, который не активен, после чего активируем его
    {
        List<GameObject> _go = new List<GameObject>();
        for (int i = 0; i < ObjList.Count; i++)
        {
            if (!ObjList[i].objPrefab.activeInHierarchy && ObjList[i].objType == type)
            {
                _go.Add(ObjList[i].objPrefab);
            }
        }
        if (_go.Count != 0)
            return _go[Random.Range(0, _go.Count)];
        else
            return null;
    }

    private void CreateObjInInteractiveLayer()//объединить в один for все!!!
    {
        //инициализация спавна земли
        int controlValueGround = 0;
        for (int i = 0; i < interactiveLevelObj.Count; i++)
        {
            if (interactiveLevelObj[i].objType == ObjType.Ground)
            {
                if (controlValueGround >= 2)
                {
                    interactiveLevelObj[i].objPrefab.SetActive(true);
                    interactiveLevelObj[i].objPrefab.transform.position = new Vector2(-groundSize.x, interactiveLevelObj[i].objPrefab.transform.position.y);
                    controlValueGround = 0;
                    break;
                }
                else
                {
                    if (controlValueGround != 1)
                    {
                        interactiveLevelObj[i].objPrefab.SetActive(true);
                        interactiveLevelObj[i].objPrefab.transform.position = new Vector2(0, interactiveLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }
                    else
                    {
                        interactiveLevelObj[i].objPrefab.SetActive(true);
                        interactiveLevelObj[i].objPrefab.transform.position = new Vector2(groundSize.x, interactiveLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }

                }
            }
        }
        for (int i = 0; i < interactiveLevelObj.Count; i++)
        {
            if (interactiveLevelObj[i].objType == ObjType.UpGround)
            {
                if (controlValueGround >= 2)
                {
                    interactiveLevelObj[i].objPrefab.SetActive(true);
                    interactiveLevelObj[i].objPrefab.transform.position = new Vector2(-upGroundSize.x, interactiveLevelObj[i].objPrefab.transform.position.y);
                    controlValueGround = 0;
                    break;
                }
                else
                {
                    if (controlValueGround != 1)
                    {
                        interactiveLevelObj[i].objPrefab.SetActive(true);
                        interactiveLevelObj[i].objPrefab.transform.position = new Vector2(0, interactiveLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }
                    else
                    {
                        interactiveLevelObj[i].objPrefab.SetActive(true);
                        interactiveLevelObj[i].objPrefab.transform.position = new Vector2(upGroundSize.x, interactiveLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }

                }
            }
        }

        float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);//трава на интерактивном уровне
        if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
        for (int i = 0; i < interactiveLevelObj.Count; i++)
        {
            if (interactiveLevelObj[i].objType == ObjType.F_Grass)
            {
                StartCoroutine(CreateGrassInteractive(_spawnTime * _ILSpawnValue, GetFreeObj(interactiveLevelObj, ObjType.F_Grass)));
                break;
            }
        }
    }

    private void SpawnInteractiveLayerObj(ObjType lastType)
    {
        { /*  float _spawnTime = Random.Range(_stumpSpawnTime.MinValue, _stumpSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _stumpSpawnTime.MinValue;
        for (int i = 0; i < interactiveLevelObj.Count; i++)
        {
            if (interactiveLevelObj[i].objType == ObjType.Stump)
            {
                StartCoroutine(CreateStumpInteractive(_spawnTime * _ILSpawnValue, GetFreeObj(interactiveLevelObj, ObjType.Stump)));
                break;
            }
        }

        _spawnTime = Random.Range(_upStumpSpawnTime.MinValue, _upStumpSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _upStumpSpawnTime.MinValue;
        for (int i = 0; i < interactiveLevelObj.Count; i++)
        {
            if (interactiveLevelObj[i].objType == ObjType.UpStump)
            {
                StartCoroutine(CreateUpStumpInteractive(_spawnTime * _ILSpawnValue, GetFreeObj(interactiveLevelObj, ObjType.UpStump)));
                break;
            }
        }

        _spawnTime = Random.Range(_pipeSpawnTime.MinValue, _pipeSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _pipeSpawnTime.MinValue;
        for (int i = 0; i < interactiveLevelObj.Count; i++)
        {
            if (interactiveLevelObj[i].objType == ObjType.Pipe)
            {
                StartCoroutine(CreatePipeInteractive(_spawnTime * _ILSpawnValue, GetFreeObj(interactiveLevelObj, ObjType.Pipe)));
                break;
            }
        }

        _spawnTime = Random.Range(_platformSpawnTime.MinValue, _platformSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _platformSpawnTime.MinValue;
        for (int i = 0; i < interactiveLevelObj.Count; i++)
        {
            if (interactiveLevelObj[i].objType == ObjType.Platform)
            {
                StartCoroutine(CreatePlatformInteractive(_spawnTime * _ILSpawnValue, GetFreeObj(interactiveLevelObj, ObjType.Platform)));
                break;
            }
        }*/
        }//предыдущая версия спавна интерактивных объектов

        //реализация шанса выдачи конкретного объекта
        float _chanceSpawn = Random.Range(1, 100);
        if (lastType != ObjType.Pipe && (_chanceSpawn >= _pipeSpawnChance.MinValue && _chanceSpawn <= _pipeSpawnChance.MaxValue))
        {
            GameObject _go = GetFreeObj(interactiveLevelObj, ObjType.Pipe);

            if (_go == null)
            {
                SpawnInteractiveLayerObj(lastType);
            }
            else
                StartCoroutine(CreatePipeInteractive(_go));
        }
        else
        if (lastType != ObjType.Hole && (_chanceSpawn >= _holeSpawnChance.MinValue && _chanceSpawn <= _holeSpawnChance.MaxValue))
        {
            GameObject _go = GetFreeObj(interactiveLevelObj, ObjType.Hole);

            if (_go == null)
            {
                SpawnInteractiveLayerObj(lastType);
            }
            else
                StartCoroutine(CreateHoleInteractive(_go));
        }
        else
        if (lastType != ObjType.UpHole && (_chanceSpawn >= _upHoleSpawnChance.MinValue && _chanceSpawn <= _upHoleSpawnChance.MaxValue))
        {
            GameObject _go = GetFreeObj(interactiveLevelObj, ObjType.UpHole);

            if (_go == null)
            {
                SpawnInteractiveLayerObj(lastType);
            }
            else
                StartCoroutine(CreateUpHoleInteractive(_go));
        }
        else
        if (_chanceSpawn >= _platformSpawnChance.MinValue && _chanceSpawn <= _platformSpawnChance.MaxValue)
        {
            GameObject _go = GetFreeObj(interactiveLevelObj, ObjType.Platform);

            if (_go == null)
            {
                SpawnInteractiveLayerObj(lastType);
            }
            else
                StartCoroutine(CreatePlatformInteractive(_go));
        }
        else
        if (lastType != ObjType.Stump && (_chanceSpawn >= _stumpSpawnChance.MinValue && _chanceSpawn <= _stumpSpawnChance.MaxValue))
        {
            GameObject _go = GetFreeObj(interactiveLevelObj, ObjType.Stump);

            if (_go == null)
            {
                SpawnInteractiveLayerObj(lastType);
            }
            else
                StartCoroutine(CreateStumpInteractive(_go));
        }
        else
        if (lastType != ObjType.UpStump && (_chanceSpawn >= _upStumpSpawnChance.MinValue && _chanceSpawn <= _upStumpSpawnChance.MaxValue))
        {
            GameObject _go = GetFreeObj(interactiveLevelObj, ObjType.UpStump);

            if (_go == null)
            {
                SpawnInteractiveLayerObj(lastType);
            }
            else
                StartCoroutine(CreateUpStumpInteractive(_go));
        }
        else//если кубик не попал ни в один из интервалов указанных пользователем, то функция запускает создание еще одной платформы
        {
            GameObject _go = GetFreeObj(interactiveLevelObj, ObjType.Platform);

            if (_go == null)
            {
                SpawnInteractiveLayerObj(lastType);
            }
            else
                StartCoroutine(CreatePlatformInteractive(_go));
        }
    }

    private void CreateObjInFirstLayer()
    {
        float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _treeSpawnTime.MinValue;
        for (int i = 0; i < firstLevelObj.Count; i++)
        {
            if (firstLevelObj[i].objType == ObjType.F_Tree)
            {
                StartCoroutine(CreateTreeFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Tree)));
                break;
            }
        }


        _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
        for (int i = 0; i < firstLevelObj.Count; i++)
        {
            if (firstLevelObj[i].objType == ObjType.F_Grass)
            {
                StartCoroutine(CreateGrassFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Grass)));
                break;
            }
        }

        _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
        for (int i = 0; i < firstLevelObj.Count; i++)
        {
            if (firstLevelObj[i].objType == ObjType.F_Bush)
            {
                StartCoroutine(CreateBushFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Bush)));
                break;
            }
        }

        int controlValueGround = 0;
        for (int i = 0; i < firstLevelObj.Count; i++)
        {
            if (firstLevelObj[i].objType == ObjType.Ground)
            {
                if (controlValueGround >= 2)
                {
                    firstLevelObj[i].objPrefab.SetActive(true);
                    firstLevelObj[i].objPrefab.transform.position = new Vector2(-groundSize.x, firstLevelObj[i].objPrefab.transform.position.y);
                    controlValueGround = 0;
                    break;
                }
                else
                {
                    if (controlValueGround != 1)
                    {
                        firstLevelObj[i].objPrefab.SetActive(true);
                        firstLevelObj[i].objPrefab.transform.position = new Vector2(0, firstLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }
                    else
                    {
                        firstLevelObj[i].objPrefab.SetActive(true);
                        firstLevelObj[i].objPrefab.transform.position = new Vector2(groundSize.x, firstLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }

                }
            }
        }
        for (int i = 0; i < firstLevelObj.Count; i++)
        {
            if (firstLevelObj[i].objType == ObjType.UpGround)
            {
                if (controlValueGround >= 2)
                {
                    firstLevelObj[i].objPrefab.SetActive(true);
                    firstLevelObj[i].objPrefab.transform.position = new Vector2(-upGroundSize.x, firstLevelObj[i].objPrefab.transform.position.y);
                    controlValueGround = 0;
                    break;
                }
                else
                {
                    if (controlValueGround != 1)
                    {
                        firstLevelObj[i].objPrefab.SetActive(true);
                        firstLevelObj[i].objPrefab.transform.position = new Vector2(0, firstLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }
                    else
                    {
                        firstLevelObj[i].objPrefab.SetActive(true);
                        firstLevelObj[i].objPrefab.transform.position = new Vector2(upGroundSize.x, firstLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }

                }
            }
        }





    }
    private void CreateObjInSecondLayer()
    {
        float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = 2;

        for (int i = 0; i < secondLevelObj.Count; i++)
        {
            if (secondLevelObj[i].objType == ObjType.F_Tree)
            {
                StartCoroutine(CreateTreeSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Tree)));
                break;
            }
        }
        _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = 0.5f;
        for (int i = 0; i < secondLevelObj.Count; i++)
        {
            if (secondLevelObj[i].objType == ObjType.F_Grass)
            {
                StartCoroutine(CreateGrassSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Grass)));
                break;
            }
        }
        _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
        for (int i = 0; i < secondLevelObj.Count; i++)
        {
            if (secondLevelObj[i].objType == ObjType.F_Bush)
            {
                StartCoroutine(CreateBushSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Bush)));
                break;
            }
        }

        int controlValueGround = 0;
        for (int i = 0; i < secondLevelObj.Count; i++)
        {
            if (secondLevelObj[i].objType == ObjType.Ground)
            {
                if (controlValueGround >= 2)
                {
                    secondLevelObj[i].objPrefab.SetActive(true);
                    secondLevelObj[i].objPrefab.transform.position = new Vector2(-groundSize.x, secondLevelObj[i].objPrefab.transform.position.y);
                    controlValueGround = 0;
                    break;
                }
                else
                {
                    if (controlValueGround != 1)
                    {
                        secondLevelObj[i].objPrefab.SetActive(true);
                        secondLevelObj[i].objPrefab.transform.position = new Vector2(0, secondLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }
                    else
                    {
                        secondLevelObj[i].objPrefab.SetActive(true);
                        secondLevelObj[i].objPrefab.transform.position = new Vector2(groundSize.x, secondLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }
                }
            }
        }
        for (int i = 0; i < secondLevelObj.Count; i++)
        {
            if (secondLevelObj[i].objType == ObjType.UpGround)
            {
                if (controlValueGround >= 2)
                {
                    secondLevelObj[i].objPrefab.SetActive(true);
                    secondLevelObj[i].objPrefab.transform.position = new Vector2(-upGroundSize.x, secondLevelObj[i].objPrefab.transform.position.y);
                    controlValueGround += 1;
                    break;
                }
                else
                {
                    if (controlValueGround != 1)
                    {
                        secondLevelObj[i].objPrefab.SetActive(true);
                        secondLevelObj[i].objPrefab.transform.position = new Vector2(0, secondLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }
                    else
                    {
                        secondLevelObj[i].objPrefab.SetActive(true);
                        secondLevelObj[i].objPrefab.transform.position = new Vector2(upGroundSize.x, secondLevelObj[i].objPrefab.transform.position.y);
                        controlValueGround += 1;
                    }
                }
            }
        }

    }
    private void CreateObjInThirdLayer()
    {
        float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = 2;
        for (int i = 0; i < thirdLevelObj.Count; i++)
        {
            if (thirdLevelObj[i].objType == ObjType.F_Tree)
            {
                StartCoroutine(CreateTreeThird(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Tree)));
                break;
            }
        }
        _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = 0.5f;
        for (int i = 0; i < thirdLevelObj.Count; i++)
        {
            if (thirdLevelObj[i].objType == ObjType.F_Grass)
            {
                StartCoroutine(CreateGrassSecond(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Grass)));
                break;
            }
        }
        _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
        if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
        for (int i = 0; i < thirdLevelObj.Count; i++)
        {
            if (thirdLevelObj[i].objType == ObjType.F_Bush)
            {
                StartCoroutine(CreateBushThird(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Bush)));
                break;
            }
        }


    }

    //Корутины по генерации уровня
    private IEnumerator CreateTreeFirst(float time, GameObject _go)
    {
        //Формула расчета увеличения времени респавна объекта: 
        //Направление респа (от -1 до 1 равен противоположному направлению бега) + направление бега (от -1 до 1)
        //Все это умножаем на заданную скорость бега, далее результат складываем с базовым значением скорости спавна. 
        //(вычисляем на сколько медленнее будет респ, если игрок будет бежать медленее)
        //Получаем скорость спавна, которая увеличивается, если игрок замедляется

        float lastTime = time;
        for (int i = 0; i <= (((axisValue + moveSpeed) * lastTime) + lastTime); i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
                //Debug.Log("ждем " + i);
            }
            else
            {
                lastTime = time;
               // Debug.Log("идет тайминг " + i);
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _treeSpawnTime.MaxValue;
            StartCoroutine(CreateTreeFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Tree)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _treeSpawnTime.MaxValue;
            StartCoroutine(CreateTreeFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Tree)));
        }
    }
    private IEnumerator CreateGrassFirst(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= ((axisValue + moveSpeed) * lastTime) + lastTime; i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
            StartCoroutine(CreateGrassFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Grass)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
            StartCoroutine(CreateGrassFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Grass)));
        }
    }
    private IEnumerator CreateBushFirst(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= (((axisValue + moveSpeed) * lastTime) + lastTime); i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
            StartCoroutine(CreateBushFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Bush)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
            StartCoroutine(CreateBushFirst(_spawnTime * _FLSpawnValue, GetFreeObj(firstLevelObj, ObjType.F_Bush)));
        }
    }

    private IEnumerator CreateTreeSecond(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= (((axisValue + moveSpeed) * lastTime) + lastTime); i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _treeSpawnTime.MaxValue;
            StartCoroutine(CreateTreeSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Tree)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _treeSpawnTime.MaxValue;
            StartCoroutine(CreateTreeSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Tree)));
        }
    }
    private IEnumerator CreateGrassSecond(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= (((axisValue + moveSpeed) * lastTime) + lastTime); i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;


            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
            StartCoroutine(CreateGrassSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Grass)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
            StartCoroutine(CreateGrassSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Grass)));
        }
    }
    private IEnumerator CreateBushSecond(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= (((axisValue + moveSpeed) * lastTime) + lastTime); i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
            StartCoroutine(CreateBushSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Bush)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
            StartCoroutine(CreateBushSecond(_spawnTime * _SLSpawnValue, GetFreeObj(secondLevelObj, ObjType.F_Bush)));
        }
    }

    private IEnumerator CreateTreeThird(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= (((axisValue + moveSpeed) * lastTime) + lastTime); i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _treeSpawnTime.MaxValue;
            StartCoroutine(CreateTreeThird(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Tree)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_treeSpawnTime.MinValue, _treeSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _treeSpawnTime.MaxValue;
            StartCoroutine(CreateTreeThird(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Tree)));
        }
    }
    private IEnumerator CreateGrassThird(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= (((axisValue + moveSpeed) * lastTime) + lastTime); i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
            StartCoroutine(CreateGrassThird(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Grass)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
            StartCoroutine(CreateGrassThird(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Grass)));
        }
    }
    private IEnumerator CreateBushThird(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= (((axisValue + moveSpeed) * lastTime) + lastTime); i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
            StartCoroutine(CreateBushThird(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Bush)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_bushSpawnTime.MinValue, _bushSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _bushSpawnTime.MinValue;
            StartCoroutine(CreateBushThird(_spawnTime * _TLSpawnValue, GetFreeObj(thirdLevelObj, ObjType.F_Bush)));
        }
    }

    private IEnumerator CreateGrassInteractive(float time, GameObject _go)
    {
        float lastTime = time;
        for (int i = 0; i <= ((axisValue + moveSpeed) * lastTime) + lastTime; i++)
        {
            if (moveSpeed == 0.0f)
            {
                lastTime = 5.0f;
                i = 0;
            }
            else
                lastTime = time;

            yield return new WaitForSeconds(0.1f);
        }

        if (_go == null)
        {
            yield return new WaitForSeconds(0.5f);
            float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
            StartCoroutine(CreateGrassFirst(_spawnTime * _ILSpawnValue, GetFreeObj(interactiveLevelObj, ObjType.F_Grass)));
        }
        else
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

            //test
            float _spawnTime = Random.Range(_grassSpawnTime.MinValue, _grassSpawnTime.MaxValue);
            if (_spawnTime == 0) _spawnTime = _grassSpawnTime.MinValue;
            StartCoroutine(CreateGrassFirst(_spawnTime * _ILSpawnValue, GetFreeObj(interactiveLevelObj, ObjType.F_Grass)));
        }
    }

    private IEnumerator CreateStumpInteractive(GameObject _go)
    {
        distance = 0.0f;
        while (distance < spawnIntDistance)
        {
            distance += moveSpeed * 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        _go.SetActive(true);
        _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

        SpawnInteractiveLayerObj(ObjType.Stump);//после завершения запускаем void, где определяем сл. объект для спавна
    }

    private IEnumerator CreateHoleInteractive(GameObject _go)
    {
        distance = 0.0f;
        while (distance < spawnIntDistance)
        {
            distance += moveSpeed * 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        _go.SetActive(true);
        _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

        SpawnInteractiveLayerObj(ObjType.Hole);//после завершения запускаем void, где определяем сл. объект для спавна
    }

    
    private IEnumerator CreateUpHoleInteractive(GameObject _go)
    {
        distance = 0.0f;
        while (distance < spawnIntDistance)
        {
            distance += moveSpeed * 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        _go.SetActive(true);
        _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

        SpawnInteractiveLayerObj(ObjType.UpHole);//после завершения запускаем void, где определяем сл. объект для спавна
    }

    private IEnumerator CreateUpStumpInteractive(GameObject _go)
    {
        distance = 0.0f;
        while (distance < spawnIntDistance)
        {
            distance += moveSpeed * 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        _go.SetActive(true);
        _go.transform.position = new Vector2(spawnPositionX, _go.transform.position.y);//axisValue должна быть отрицательной, если мы бежим вперед.

        SpawnInteractiveLayerObj(ObjType.UpStump);
    }

    private IEnumerator CreatePipeInteractive(GameObject _go)
    {
        distance = 0.0f;
        while (distance < spawnIntDistance)
        {
            distance += moveSpeed * 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        _go.SetActive(true);

        float _dP = Random.Range(1, 100);//кидаем кубик, так сказать
        if (_dP < 80)
        {
            float yPos = Random.Range(-OrthSize + pipeSizeY * 1.8f + groundSize.y, OrthSize - pipeSizeY * 1.8f - upGroundSize.y);
            if (spawnPositionX > 0)
                _go.transform.position = new Vector2(spawnPositionX, yPos);//axisValue должна быть отрицательной, если мы бежим вперед.
            else
                _go.transform.position = new Vector2(spawnPositionX, yPos);//axisValue должна быть отрицательной, если мы бежим вперед.
        }

        SpawnInteractiveLayerObj(ObjType.Pipe);
    }

    private IEnumerator CreatePlatformInteractive(GameObject _go)
    {
        distance = 0.0f;
        while (distance < spawnIntDistance)
        {
            distance += moveSpeed * 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        _go.SetActive(true);

        float _dP = Random.Range(1, 100);//кидаем кубик, так сказать
        if (_dP < 80)
        {
            float yPos = Random.Range(-OrthSize + platformSizeY * 3 + groundSize.y, OrthSize - platformSizeY * 3 - upGroundSize.y);//позиция платформы
            _go.transform.position = new Vector2(spawnPositionX, yPos);//axisValue должна быть отрицательной, если мы бежим вперед.
        }

        GameObject point = GetFreeObj(interactiveLevelObj, ObjType.Point);
        if (point != null)
        {
            point.SetActive(true);
            if (_go.transform.position.y > 0)
                point.transform.position = new Vector2(spawnPositionX, _go.transform.position.y - platformSizeY * 2);
            else
            if (_go.transform.position.y <= 0)
                point.transform.position = new Vector2(spawnPositionX, _go.transform.position.y + platformSizeY * 2);
        }

        SpawnInteractiveLayerObj(ObjType.Ground);//внимание, надо заменить на null ObjType
    }


    private void CreateGroundInteractive(float pos, List<objInLayer> layer, ObjType type)
    {
        GameObject _go = GetFreeObj(layer, type);

        if (_go != null)
        {
            _go.SetActive(true);
            _go.transform.position = new Vector2(pos, _go.transform.position.y);
        }
        else
            Debug.LogError("Ground создается некорректно, не хватает объектов в пуле");

    }




    private void FixedUpdate()
    {

        //axisValue должна быть отрицательной, если мы бежим вперед.
        if (moveSpeed < 0)
        {
            axisValue = 1;
            spawnPositionX = -spawnPosition;
        } 
        else if (moveSpeed > 0)
        {
            axisValue = -1;
            spawnPositionX = spawnPosition;
        } 
        else if (moveSpeed == 0) axisValue = 0;

       //test
        for (int i = 0; i < interactiveLevelObj.Count; i++)
       {
           if (interactiveLevelObj[i].objPrefab.activeInHierarchy)
                interactiveLevelObj[i].objPrefab.transform.position = 
               new Vector2((interactiveLevelObj[i].objPrefab.transform.position.x + moveSpeed * moveLevelINTER), interactiveLevelObj[i].objPrefab.transform.position.y);

            if (interactiveLevelObj[i].objType == ObjType.Ground && interactiveLevelObj[i].objPrefab.activeInHierarchy)
            {
                if ((spawnPositionX * (-1)) < 0 && interactiveLevelObj[i].objPrefab.transform.position.x < -groundSize.x)
                {
                    interactiveLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(interactiveLevelObj[i].objPrefab.transform.position.x + groundSize.x * 2, interactiveLevelObj, ObjType.Ground);
                    
                }
                else
                if ((spawnPositionX * (-1)) > 0 && interactiveLevelObj[i].objPrefab.transform.position.x > groundSize.x)
                {
                    interactiveLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(interactiveLevelObj[i].objPrefab.transform.position.x - groundSize.x * 2, interactiveLevelObj, ObjType.Ground);
                }
            }
            if (interactiveLevelObj[i].objType == ObjType.UpGround && interactiveLevelObj[i].objPrefab.activeInHierarchy)
            {
                if ((spawnPositionX * (-1)) < 0 && interactiveLevelObj[i].objPrefab.transform.position.x < -upGroundSize.x)
                {
                    interactiveLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(interactiveLevelObj[i].objPrefab.transform.position.x + upGroundSize.x * 2, interactiveLevelObj, ObjType.UpGround);

                }
                else
                if ((spawnPositionX * (-1)) > 0 && interactiveLevelObj[i].objPrefab.transform.position.x > upGroundSize.x)
                {
                    interactiveLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(interactiveLevelObj[i].objPrefab.transform.position.x - groundSize.x * 2, interactiveLevelObj, ObjType.UpGround);
                }
            }
        }

        for (int i = 0; i < firstLevelObj.Count; i++)
        {
            if (firstLevelObj[i].objPrefab.activeInHierarchy)
                firstLevelObj[i].objPrefab.transform.position =
               new Vector2((firstLevelObj[i].objPrefab.transform.position.x + moveSpeed * moveLevelFIRST), firstLevelObj[i].objPrefab.transform.position.y);

            if (firstLevelObj[i].objType == ObjType.Ground && firstLevelObj[i].objPrefab.activeInHierarchy)
            {
                if ((spawnPositionX * (-1)) < 0 && firstLevelObj[i].objPrefab.transform.position.x < -groundSize.x)
                {
                    firstLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(firstLevelObj[i].objPrefab.transform.position.x + groundSize.x * 2, firstLevelObj, ObjType.Ground);

                }
                else
                if ((spawnPositionX * (-1)) > 0 && firstLevelObj[i].objPrefab.transform.position.x > groundSize.x)
                {
                    firstLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(firstLevelObj[i].objPrefab.transform.position.x - groundSize.x * 2, firstLevelObj, ObjType.Ground);
                }
            }
            if (firstLevelObj[i].objType == ObjType.UpGround && firstLevelObj[i].objPrefab.activeInHierarchy)
            {
                if ((spawnPositionX * (-1)) < 0 && firstLevelObj[i].objPrefab.transform.position.x < -upGroundSize.x)
                {
                    firstLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(firstLevelObj[i].objPrefab.transform.position.x + upGroundSize.x * 2, firstLevelObj, ObjType.UpGround);

                }
                else
                if ((spawnPositionX * (-1)) > 0 && firstLevelObj[i].objPrefab.transform.position.x > upGroundSize.x)
                {
                    firstLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(firstLevelObj[i].objPrefab.transform.position.x - groundSize.x * 2, firstLevelObj, ObjType.UpGround);
                }
            }
        }

        for (int i = 0; i < secondLevelObj.Count; i++)
        {
            if (secondLevelObj[i].objPrefab.activeInHierarchy)
                secondLevelObj[i].objPrefab.transform.position =
               new Vector2((secondLevelObj[i].objPrefab.transform.position.x + moveSpeed * moveLevelSECOND), secondLevelObj[i].objPrefab.transform.position.y);

            if (secondLevelObj[i].objType == ObjType.Ground && secondLevelObj[i].objPrefab.activeInHierarchy)
            {
                if ((spawnPositionX * (-1)) < 0 && secondLevelObj[i].objPrefab.transform.position.x < -groundSize.x)
                {
                    secondLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(secondLevelObj[i].objPrefab.transform.position.x + groundSize.x * 2, secondLevelObj, ObjType.Ground);

                }
                else
                if ((spawnPositionX * (-1)) > 0 && secondLevelObj[i].objPrefab.transform.position.x > groundSize.x)
                {
                    secondLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(secondLevelObj[i].objPrefab.transform.position.x - groundSize.x * 2, secondLevelObj, ObjType.Ground);
                }
            }
            if (secondLevelObj[i].objType == ObjType.UpGround && secondLevelObj[i].objPrefab.activeInHierarchy)
            {
                if ((spawnPositionX * (-1)) < 0 && secondLevelObj[i].objPrefab.transform.position.x < -upGroundSize.x)
                {
                    secondLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(secondLevelObj[i].objPrefab.transform.position.x + upGroundSize.x * 2, secondLevelObj, ObjType.UpGround);

                }
                else
                if ((spawnPositionX * (-1)) > 0 && secondLevelObj[i].objPrefab.transform.position.x > upGroundSize.x)
                {
                    secondLevelObj[i].objPrefab.SetActive(false);
                    CreateGroundInteractive(secondLevelObj[i].objPrefab.transform.position.x - groundSize.x * 2, secondLevelObj, ObjType.UpGround);
                }
            }
        }

        for (int i = 0; i < thirdLevelObj.Count; i++)
        {
            if (thirdLevelObj[i].objPrefab.activeInHierarchy)
                thirdLevelObj[i].objPrefab.transform.position =
               new Vector2((thirdLevelObj[i].objPrefab.transform.position.x + moveSpeed * moveLevelTHIRD), thirdLevelObj[i].objPrefab.transform.position.y);
        }


    }



















    private void Update()
    {

    }

    public void StartGame()
    {
        moveSpeed = 1;
    }

    public void StopGame()
    {
        moveSpeed = 0;
    }

    public void ResetLevel()
    {
        for (int i = 0; i < interactiveLevelObj.Count; i++)
        {
            if (interactiveLevelObj[i].objPrefab.activeInHierarchy && (interactiveLevelObj[i].objType != ObjType.Ground || interactiveLevelObj[i].objType != ObjType.UpGround))
                interactiveLevelObj[i].objPrefab.SetActive(false);
        }
        for (int i = 0; i < firstLevelObj.Count; i++)
        {
            if (firstLevelObj[i].objPrefab.activeInHierarchy && (firstLevelObj[i].objType != ObjType.Ground || firstLevelObj[i].objType != ObjType.UpGround))
                firstLevelObj[i].objPrefab.SetActive(false);
        }
        for (int i = 0; i < secondLevelObj.Count; i++)
        {
            if (secondLevelObj[i].objPrefab.activeInHierarchy && (firstLevelObj[i].objType != ObjType.Ground || secondLevelObj[i].objType != ObjType.UpGround))
                secondLevelObj[i].objPrefab.SetActive(false);
        }
        for (int i = 0; i < thirdLevelObj.Count; i++)
        {
            if (thirdLevelObj[i].objPrefab.activeInHierarchy && (firstLevelObj[i].objType != ObjType.Ground || thirdLevelObj[i].objType != ObjType.UpGround))
                thirdLevelObj[i].objPrefab.SetActive(false);
        }



    }
}
