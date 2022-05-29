using UnityEngine;

public class CardData : MonoBehaviour
{
    [System.NonSerialized]
    public Card Card;

    virtual public bool IsDataValid()
    {
        return true;
    }

    virtual public void RequestDestroy()
    {
        Destroy(gameObject);
    }
}
