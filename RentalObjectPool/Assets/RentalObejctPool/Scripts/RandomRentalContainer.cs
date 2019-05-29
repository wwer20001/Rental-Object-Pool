using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRentalContainer : MonoBehaviour, IRentable
{
    [SerializeField]
    private string objectKey;
    public string ObjectKey { get { return objectKey; } private set { objectKey = value; } }

    [SerializeField]
    private int initCreateCount = 0;

    [SerializeField]
    private List<GameObject> poolingPrefabs = null;
    private List<RentableObject> poolingObjects = new List<RentableObject>();

    [SerializeField]
    private bool isNoSupplement;
    public bool IsNoSupplement { get { return isNoSupplement; } set { isNoSupplement = value; } }

    public IEnumerator Initialize()
    {
        if (poolingPrefabs == null)
        {
            Debug.LogWarning(string.Format("RentalContainer initialze failed! :: prefab list is null - key({0})", ObjectKey));
            yield break;
        }
        if (poolingPrefabs.Count == 0)
        {
            Debug.LogWarning(string.Format("RentalContainer initialze failed! :: prefab list is empty  - key({0})", ObjectKey));
            yield break;
        }

        foreach(var prefab in poolingPrefabs)
        {
            var rentableObject = prefab.GetComponent<RentableObject>();
            if(rentableObject == null)
            {
                Debug.LogWarning(string.Format("RentalContainer initialze failed! :: prefab is not RentableObject - key({0})", ObjectKey));
                yield break;
            }
            prefab.SetActive(false);
        }

        for (int i = 0; i < initCreateCount; i++)
        {
            var newObject = CreateObject();
            poolingObjects.Add(newObject);

            if (initCreateCount % 10 == 0)
            {
                yield return null;
            }
        }
    }

    public bool Set(string key, GameObject prefab, int createCount)
    {
        if (prefab == null)
        {
            Debug.LogWarning(string.Format("RentalContainer initialze failed! :: invalid prefab - key({0})", ObjectKey));
            return false;
        }
        if (prefab.GetComponent<RentableObject>() == null)
        {
            Debug.LogWarning(string.Format("RentalContainer initialze failed! :: prefab is not RentableObject - key({0})", ObjectKey));
            return false;
        }

        ObjectKey = key;
        poolingPrefabs.Add(prefab);
        initCreateCount = createCount;
        return true;
    }

    private RentableObject CreateObject()
    {
        var newObject = Instantiate(poolingPrefabs[Random.Range(0, poolingPrefabs.Count-1)]).GetComponent<RentableObject>();
        newObject.SetObjectPoolKey(gameObject, ObjectKey);
        newObject.transform.SetParent(transform);
        return newObject;
    }

    public RentableObject Rent()
    {
        if (poolingObjects.Count != 0)
        {
            var obj = poolingObjects[Random.Range(0, poolingObjects.Count)];
            poolingObjects.Remove(obj);
            obj.Rent(this);
            return obj;
        }
        else
        {
            if (!IsNoSupplement)
            {
                var obj = CreateObject();
                obj.Rent(this);
                return obj;
            }
            else
            {
                Debug.LogWarning(string.Format("object rent fail. key({0}) is no more supplement create.", ObjectKey));
                return null;
            }
        }
    }

    public void Return(RentableObject returnObject)
    {
        returnObject.transform.SetParent(transform);
        poolingObjects.Add(returnObject);
    }

    public void Clear()
    {
        foreach(var obj in poolingObjects)
        {
            Destroy(obj.gameObject);
        }
        poolingObjects.Clear();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}