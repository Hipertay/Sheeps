﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelHelper : MonoBehaviour
{
    public int levelID;
    public int taskID;
    public bool randomTask;
    public int coralSize;
    public Task[] tasks;

    [Serializable]
    public class Task
    {
        public int taskID;
        public int corralID;
        public Object[] targets;

        [Serializable]
        public class Object
        {
            public GameObject prefab;
            public int cacheSize;
            public TypeAction typeAction;      //тип действия
            public float square;              //площадь занимаемая персонажем
            public float scaledPower;          //кратность увеличения/уменьшения
            public float minSpedTarget;       //скорость персонажа min
            public float maxSpedTarget;       //скорость персонажа max
        }
    }


}
