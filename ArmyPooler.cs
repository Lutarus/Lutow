using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Character.Core;

public class ArmyPooler : MonoBehaviour
{
    #region SingleTon

    public static ArmyPooler Instance;
    

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    int tagName;
    public Dictionary<int, Queue<GameObject>> poolDictionary;
    PrefabCollector prefabCollector;

    public Dictionary<int, Queue<GameObject>> nodeDictionary;

    private void Start()
    {
        prefabCollector = PrefabCollector.Instance;
        poolDictionary = new Dictionary<int, Queue<GameObject>>();

        nodeDictionary = new Dictionary<int, Queue<GameObject>>();

        foreach (PrefabCollector.ArmyPool pool in prefabCollector.armyPools)
        {
            
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < 5; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                tagName = pool.characterType;
                obj.SetActive(false);
                objectPool.Enqueue(obj);
                //Debug.Log("Pool ilk oluşturma :" + tagName);
                //Debug.Log("prefab collector ilk oluşturma :" + pool.characterType);
            }
            if (!nodeDictionary.ContainsKey(pool.characterType))
            {
                Queue<GameObject> nodePool = new Queue<GameObject>();
                nodeDictionary.Add(pool.characterType, nodePool);
            }

            poolDictionary.Add(tagName, objectPool);
        }
    }

    public GameObject SpawnFromPool(int characterType, Vector3 position, Quaternion rotation)
    {
        //Debug.Log("Pool spawn çağırımı :" + characterType);
        if (!poolDictionary.ContainsKey(characterType))
        {
            //Debug.LogWarning("Çağırılan " + characterType + " pool içerisinde mevcut değil");
            return null;
        }
        if (poolDictionary[characterType].Count == 0)
            GrowPool(characterType);

        GameObject objectToSpawn = poolDictionary[characterType].Dequeue();

        if (objectToSpawn.GetComponent<Health>().IsDead())
        {
            objectToSpawn.GetComponent<Health>().CharacterReborn();
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.GetComponent<NavMeshAgent>().Warp(position);
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnToPool(int characterType, GameObject instance)
    {
        instance.SetActive(false);
        poolDictionary[characterType].Enqueue(instance);
    }

    public GameObject SpawnFromNodePool(int characterType, Vector3 position, Quaternion rotation)
    {
        if (!nodeDictionary.ContainsKey(characterType))
        {
            //Debug.LogWarning("Çağırılan " + characterType + " pool içerisinde mevcut değil");
            return null;
        }

        GameObject objectToSpawn = nodeDictionary[characterType].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.GetComponent<NavMeshAgent>().Warp(position);
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }
    public void ReturnToNodePool(int characterType, GameObject instance)
    {
        instance.SetActive(false);
        nodeDictionary[characterType].Enqueue(instance);
    }

    private void GrowPool(int characterType)
    {
         foreach (PrefabCollector.ArmyPool pool in prefabCollector.armyPools)
        {
            if (pool.characterType == characterType)
            {
                for (int i = 0; i < 5; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    poolDictionary[characterType].Enqueue(obj);
                }
                return;
            }
        }
    }
}
