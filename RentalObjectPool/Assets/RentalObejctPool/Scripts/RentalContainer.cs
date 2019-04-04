using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RentalContainer : MonoBehaviour, IRentable
{
    [SerializeField]
    private string objectKey;
    public string ObjectKey { get { return objectKey; } private set { objectKey = value; } }

    [SerializeField]
    private int initCreateCount = 0;

    [SerializeField]
    private GameObject poolingPrefab = null;
    private Queue<RentableObject> poolingObject = new Queue<RentableObject>();

    [SerializeField]
    private bool isNoSupplement;
    public bool IsNoSupplement { get { return isNoSupplement; } set { isNoSupplement = value; } }

    public IEnumerator Initialize()
    {
        if (poolingPrefab == null)
        {
            Debug.LogWarning(string.Format("RentalContainer initialze failed! :: invalid prefab - key({0})", ObjectKey));
            yield break;
        }
        if (poolingPrefab.GetComponent<RentableObject>() == null)
        {
            Debug.LogWarning(string.Format("RentalContainer initialze failed! :: prefab is not RentableObject - key({0})", ObjectKey));
            yield break;
        }

        poolingPrefab.SetActive(false);

        for (int i = 0; i < initCreateCount; i++)
        {
            var newObject = CreateObject();
            poolingObject.Enqueue(newObject);

            if (initCreateCount % 10 == 0)
            {
                yield return null;
            }
        }
    }

    private RentableObject CreateObject()
    {
        var newObject = Instantiate(poolingPrefab).GetComponent<RentableObject>();
        newObject.SetObjectPoolKey(gameObject, ObjectKey);
        newObject.transform.SetParent(transform);
        return newObject;
    }

    public RentableObject Rent()
    {
        if (poolingObject.Count != 0)
        {
            var obj = poolingObject.Dequeue();
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
        poolingObject.Enqueue(returnObject);
    }

    public void Clear()
    {
        while(poolingObject.Count != 0)
        {
            var obj = poolingObject.Dequeue();
            Destroy(obj.gameObject);
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
