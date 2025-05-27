using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckView : MonoBehaviour
{
    // object used to instantiate visualization of card in deck
    public CardData CardObject;

    // container to which add cards
    public GameObject Container;

    // scale of children (TODO - remove?)
    public float Scale = 1.0f;

    // card representing deck that we are visualizing
    private Card DeckCard;

    Dictionary<Card, CardData> _cardToObj = new Dictionary<Card, CardData>();

    public void SetDeckCard(Card card)
    {
        DeckCard = card;
    }

    protected virtual Card Deck()
    {
        return DeckCard;
    }

    // Start is called before the first frame update
    protected void Start()
    {
        RemoveAllChildren();
        AddCards();
        Deck().OnChildCardRemoved.AddListener(OnCardRemoved);
        Deck().OnChildCardAdded.AddListener(OnCardAdded);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Transform CurrentTransform()
    {
        return Container ? Container.transform : transform;
    }

    void RemoveAllChildren()
    {
        foreach (Transform child in CurrentTransform())
        {
            Destroy(child.gameObject);
        }
    }

    void AddCards()
    {
        var hand = Deck();
        foreach (var card in hand.Children)
        {
            AddCard(card);
        }
    }

    void AddCard(Card card)
    {
        var cardObj = Instantiate(CardObject, CurrentTransform());
        cardObj.Card = card;
        cardObj.transform.localScale = new Vector3(Scale, Scale, 1.0F);

        // check if we have all visual data to display it
        if (!cardObj.IsDataValid())
        {
            Debug.LogWarning($"Data not found for card {card.id}");
            return;
        }

        _cardToObj.Add(card, cardObj);
    }

    void OnCardRemoved(CardRemovedEventArgs args)
    {
        var cardObj = _cardToObj[args.Card];
        cardObj.RequestDestroy();
        _cardToObj.Remove(args.Card);
    }

    void OnCardAdded(CardAddedEventArgs args)
    {
        AddCard(args.Card);
    }
}
