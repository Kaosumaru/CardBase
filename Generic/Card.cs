using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Linq;
using UnityEngine.Events;
using Linq.Extensions;

public class CardAddedEventArgs : EventArgs
{
    public Card Card { get; set; }
    public Card Creator { get; set; }
    public bool Created { get; set; }
}

public class CardRemovedEventArgs : EventArgs
{
    public Card Card { get; set; }
}

public class CardMovedEventArgs : EventArgs
{
    public Card Parent { get; set; }
    public Card PreviousParent { get; set; }
}

public class CardAddedEvent : UnityEvent<CardAddedEventArgs>
{
}

public class CardRemovedEvent : UnityEvent<CardRemovedEventArgs>
{
}

public class CardMovedEvent : UnityEvent<CardMovedEventArgs>
{
}

[System.Serializable]
public class Card
{
    public string id; // unique instance of card Logic
    protected Tokens tokens = new Tokens();
    public List<Card> Children = new List<Card>();

    [NonSerialized]
    protected WeakReference<Card> parent;

    [NonSerialized]
    public UnityEvent OnDestroyed = new UnityEvent();
    [NonSerialized]
    public CardAddedEvent OnChildCardAdded = new CardAddedEvent();
    [NonSerialized]
    public CardRemovedEvent OnChildCardRemoved = new CardRemovedEvent();
    [NonSerialized]
    public CardMovedEvent OnMoved = new CardMovedEvent();

    public Token Token(System.Guid tokenType)
    {
        return tokens.Token(tokenType);
    }

    public int TokenValue(System.Guid tokenType)
    {
        return tokens.TokenValue(tokenType);
    }

    public bool HasToken(System.Guid tokenType)
    {
        return tokens.HasToken(tokenType);
    }

    public void ClearTokens()
    {
        tokens.Clear();
    }

    public Card CreateChild(string id, Card creator = null)
    {
        var cardData = new Card();
        cardData.id = id;
        cardData.parent = new WeakReference<Card>(this);
        Children.Add(cardData);
        OnChildCardAdded?.Invoke(new CardAddedEventArgs { Card = cardData, Creator = creator, Created = true });
        return cardData;
    }

    public void Add(Card cardData, Card creator = null)
    {
        Card oldParent = null;
        if (cardData.parent != null)
        {
            bool success = cardData.parent.TryGetTarget(out oldParent);
            if (!success)
            {
                Debug.Assert(false);
                return;
            }
            if (oldParent == this) return;

            oldParent.RemoveChild(cardData);
            oldParent.OnChildCardRemoved?.Invoke(new CardRemovedEventArgs { Card = cardData });
        }
        cardData.parent = new WeakReference<Card>(this);
        Children.Add(cardData);

        if (oldParent != null)
        {
            cardData.OnMoved?.Invoke(new CardMovedEventArgs { Parent = this, PreviousParent = oldParent });
        }
        OnChildCardAdded?.Invoke(new CardAddedEventArgs { Card = cardData, Created = false, Creator = creator });
    }

    public Card Clone()
    {
        var data = new Card();
        data.id = id;
        data.tokens = tokens.Clone();
        data.Children = (from child in Children select child.Clone()).ToList();
        foreach (var child in data.Children)
        {
            child.parent = new WeakReference<Card>(data);
        }
        return data;
    }

    public void Destroy()
    {
        Card parentCard = null;
        bool? success = parent?.TryGetTarget(out parentCard);
        if (!success.GetValueOrDefault(false))
        {
            OnDestroyed?.Invoke();
            return;
        }
        parentCard.RemoveChild(this);
        OnDestroyed?.Invoke();
        parentCard.OnChildCardRemoved?.Invoke(new CardRemovedEventArgs { Card = this });
        parent = null;
    }

    public void DestroyAllChildren()
    {
        while (Children.Count > 0) Children.First().Destroy();
    }

    protected void RemoveChild(Card card)
    {
        Children.Remove(card);
    }

    public void AddAllCardsTo(Card card)
    {
        while (Children.Count > 0) card.Add(Children[0]);
    }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
        foreach (var child in Children)
        {
            child.parent = new WeakReference<Card>(this);
        }

        OnDestroyed = new UnityEvent();
        OnChildCardAdded = new CardAddedEvent();
        OnChildCardRemoved = new CardRemovedEvent();
        OnMoved = new CardMovedEvent();
    }

    public void Shuffle()
    {
        Children = Children.Shuffle().ToList();
    }

    public int Count
    {
        get { return Children.Count; }
    }
}
