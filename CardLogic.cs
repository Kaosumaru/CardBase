using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Targetable
{

}

public class CardLogicBase
{
    public string id;
    public HashSet<string> Tags = null;

    public bool HasTag(string tag)
    {
        if (Tags == null) return false;
        return Tags.Contains(tag);
    }

    virtual public IEnumerable<Targetable> AvailableTargets(Card card)
    {
        return new List<Targetable> { };
    }

    virtual public object[] TextVariables(Card card)
    {
        return new object[] { };
    }

    // -----------------------------------------
    // Utils
    // -----------------------------------------


    virtual public void AddedToDeck(Card card, Card deck, bool addedNow)
    {
        AddListenersAfterCardInPlay(card, deck);
    }


    // helpers for adding listeners for active effects
    virtual public void AddListenersAfterCardInPlay(Card card, Card deck)
    {

    }

    virtual public void RemoveListenersAfterCardRemovedFromPlay(Card card)
    {
        CleanupForCard(card);
    }


    static protected void AddListener<T>(Card c, UnityEvent<T> e, UnityAction<T> call)
    {
        e.AddListener(call);
        AddCleanup(c, () => e.RemoveListener(call));
    }

    static protected void AddListener(Card c, UnityEvent e, UnityAction call)
    {
        e.AddListener(call);
        AddCleanup(c, () => e.RemoveListener(call));
    }


    static Dictionary<Card, List<Action>> _cardCleanupQueue = new Dictionary<Card, List<Action>>();
    static void AddCleanup(Card c, Action a)
    {
        List<Action> actions = null;
        if (!_cardCleanupQueue.TryGetValue(c, out actions))
        {
            actions = new List<Action>();
            _cardCleanupQueue[c] = actions;
        }
        actions.Add(a);
    }

    static protected void CleanupForCard(Card c)
    {
        List<Action> actions = null;
        if (!_cardCleanupQueue.TryGetValue(c, out actions)) return;
        foreach (var a in actions)
        {
            a.Invoke();
        }

        _cardCleanupQueue.Remove(c);
    }
}
