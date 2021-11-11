using UnityEngine;

public abstract class CardData : MonoBehaviour
{
    [System.NonSerialized]
    public Card Card;

    abstract public bool IsDataValid();
}
