using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
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
    // DECK
    // -----------------------------------------

    // this is called when card is added to deck.
    // addedNow means that card was just added, if not it started in this deck from beginning
    // (after game start or deserialization)
    public void AddedToDeck(Card card, Card deck, bool addedNow)
    {
        foreach (var child in card.Children)
        {
            BaseLogic(child).AddedToDeck(child, card, addedNow);
        }

        AddListener(card, card.OnChildCardAdded, (args) => { OnCardAdded(args, card); });
        AddListener(card, card.OnChildCardRemoved, OnCardRemoved);
        OnAddedToDeck(card, deck, addedNow);
    }

    protected virtual void OnAddedToDeck(Card card, Card deck, bool addedNow)
    {

    }

    void OnCardAdded(CardAddedEventArgs args, Card deck)
    {
        BaseLogic(args.Card).AddedToDeck(args.Card, deck, true);
    }

    void OnCardRemoved(CardRemovedEventArgs args)
    {
        BaseLogic(args.Card).RemoveListenersAfterCardRemovedFromPlay(args.Card);
    }

    CardLogicBase BaseLogic(Card card)
    {
        return CardBaseList.GetBaseLogic(card.id);
    }


    // -----------------------------------------
    // Utils
    // -----------------------------------------

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

#if UNITY_EDITOR
    static CardLogicBase()
    {
        EditorApplication.playModeStateChanged += PlayModeState;

    }

    private static void PlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            _cardCleanupQueue.Clear();
    }
#endif
}
