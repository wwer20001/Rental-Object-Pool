using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRentable { }

public class RentalObjectPool : MonoBehaviour, IRentable
{
    public static RentalObjectPool Instance;

    public bool IsInitPool { get; private set; }

    private Dictionary<string, RentalContainer> rentalContainers = new Dictionary<string, RentalContainer>();

    private void Awake()
    {
        if(Instance == null)
        {
            IsInitPool = false;
            Instance = this;
            
            DontDestroyOnLoad(this);
        }
        else
        {
            if(Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    public IEnumerator Initialize()
    {
        var containers = GetComponentsInChildren<RentalContainer>();
        foreach(var container in containers)
        {
            if(!rentalContainers.ContainsKey(container.ObjectKey))
            {
                rentalContainers.Add(container.ObjectKey, container);
            }
            else
            {
                Debug.LogWarning(string.Format("RentalObjectPool Initialize Error! :: key({0}) is already contains.", container.ObjectKey));
            }
        }

        foreach (var container in rentalContainers)
        {
            yield return StartCoroutine(container.Value.Initialize());
        }

        IsInitPool = true;
    }

    public RentableObject Rent(string key)
    {
        if (rentalContainers.ContainsKey(key))
        {
            return rentalContainers[key].Rent();
        }
        else
        {
            Debug.LogWarning(string.Format("Object rent fail. key({0]) is not contains pool.", key));
            return null;
        }
    }

    public void Return(RentableObject returnObject)
    {
        returnObject.Return(this);
        if(rentalContainers.ContainsKey(returnObject.ObjectPoolKey))
        {
            rentalContainers[returnObject.ObjectPoolKey].Return(returnObject);
        }
        else
        {
            Debug.LogWarning(string.Format("Destroy {0} object. container({1}) is not exist or destroyed.", returnObject.name, returnObject.ObjectPoolKey));
            Destroy(returnObject.gameObject);
        }
    }

    public void ClearContainer(string key)
    {
        if (rentalContainers.ContainsKey(key))
        {
            rentalContainers[key].Clear();
        }
        else
        {
            Debug.LogWarning(string.Format("container({0}) clear failed. This container is not exist.", key));
        }
    }

    public void DestroyContainer(string key)
    {
        if (rentalContainers.ContainsKey(key))
        {
            rentalContainers[key].Destroy();
            rentalContainers.Remove(key);
        }
        else
        {
            Debug.LogWarning(string.Format("container({0}) destroy failed. This container is already not exist.", key));
        }
    }
}
