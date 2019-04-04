using UnityEngine;

public abstract class RentableObject : MonoBehaviour
{
    public bool IsRented { get; private set; }
    public string ObjectPoolKey { get; private set; }

    public void SetObjectPoolKey(GameObject callObject, string key)
    {
        if(callObject.GetComponent<RentalContainer>() != null)
        {
            ObjectPoolKey = key;
        }
        else
        {
            Debug.LogWarning(string.Format("RentableObject.SetObjectPoolKey({0}, {1}), invalid call. {0} is not RentalContainer.", callObject.name, key));
        }
    }

    public virtual void Rent(IRentable rentable)
    {
        if (rentable == null) return;

        gameObject.SetActive(true);
        IsRented = false;
    }

    public virtual void Return(IRentable rentable)
    {
        if (rentable == null) return;

        gameObject.SetActive(false);
        IsRented = false;
    }
}
