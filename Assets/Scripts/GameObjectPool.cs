using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameObjectPool : MonoBehaviour
{
	static GameObjectPool Pool;
	public List<ObjectCache> caches;

	public Hashtable activeCachedObjects;

    [Serializable]

	public class ObjectCache
	{
        [SerializeField]
		public GameObject prefab;       
		[SerializeField]
		public int cacheSize = 10;
		public TypeAction typeAction;      //тип действия
		public float square;              //площадь занимаемая персонажем
		public float scaledPower;          //кратность увеличения/уменьшения
		public float minSpedTarget;       //скорость персонажа min
		public float maxSpedTarget;       //скорость персонажа max


		private int cacheIndex = 0;
		private GameObject[] objects;
		private GameObject ob;

		[HideInInspector]
		public void Initialize()
		{
			objects = new GameObject[cacheSize];
			for (var i = 0; i < cacheSize; i++)
			{
				objects[i] = MonoBehaviour.Instantiate (prefab) as GameObject;
				objects[i].SetActive (false);
				objects[i].name = objects[i].name + i;
			}
		}

		public GameObject GetNextObjectInCache()
		{
			GameObject obj = null;
			for (var i = 0; i < cacheSize; i++) 
			{
				obj = objects[cacheIndex];
                if (!obj.activeSelf)
                {
                    break;
                }
				// If not, increment index and make it loop around
				// if it exceeds the size of the cache
				cacheIndex = (cacheIndex + 1) % cacheSize;
			}
			if (obj.activeSelf) 
			{
				Debug.LogWarning (
					"Spawn of " + prefab.name +
					" exceeds cache size of " + cacheSize +
					"! Reusing already active object.", obj);
				GameObjectPool.Unspawn(obj);

			}
			cacheIndex = (cacheIndex + 1) % cacheSize;
			return obj;
		}               
	}

	//void Awake()
	//{
 //       Pool = this;
	//	int amount = 0;
	//	for (var i = 0; i < caches.Length; i++) 
	//	{
	//		caches[i].Initialize ();
	//		amount += caches[i].cacheSize;
	//	}
	//	activeCachedObjects = new Hashtable(amount);
	//}

    public void OnInitialized()
    {
		Pool = this;
		int amount = 0;
		for (var i = 0; i < caches.Count; i++)
		{
			caches[i].Initialize();
			amount += caches[i].cacheSize;
		}
		activeCachedObjects = new Hashtable(amount);
	}

    public static GameObject Spawn ( GameObject prefab, Vector3 position, Quaternion rotation )
	{
		ObjectCache cache = null;
		if ( Pool != null )
		{
			for ( var i = 0; i < Pool.caches.Count; i++)
			{
				if ( Pool.caches[i].prefab == prefab )
				{
                    cache = Pool.caches[i];
				}
			}
		}
		if ( cache == null ) 
		{
			return GameObject.Instantiate ( prefab, position, rotation ) as GameObject;
		}
		GameObject obj = cache.GetNextObjectInCache();
		Target target = obj.GetComponent<Target>();
		target.typeAction = cache.typeAction;
		target.square = cache.square;
		target.scaledPower = cache.scaledPower;
		target.minSpedTarget = cache.minSpedTarget;
		target.maxSpedTarget = cache.maxSpedTarget;
		obj.transform.position = position;
		obj.transform.rotation = rotation;
		obj.SetActive(true);
		Pool.activeCachedObjects[obj] = true;
		return obj;
	}

	public static void Unspawn( GameObject objectToDestroy )
	{
		if ( Pool != null && Pool.activeCachedObjects.ContainsKey(objectToDestroy) ) 
		{
			//Debug.Log (Pool.activeCachedObjects.ContainsKey(objectToDestroy).ToString ());
			objectToDestroy.SetActive(false);
			Pool.activeCachedObjects[objectToDestroy] = false;
		}
		else
		{ 
			Unspawn ( objectToDestroy );
		}
	}
}