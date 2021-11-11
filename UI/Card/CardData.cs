using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class CardData : MonoBehaviour
{
    [System.NonSerialized]
    public Card Card;

    abstract public bool IsDataValid();
}
