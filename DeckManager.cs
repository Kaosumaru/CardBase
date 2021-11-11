using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager
{
    Card _deck;
    public DeckManager(Card card)
    {
        _deck = card;
        _deck.OnChildCardAdded.AddListener(OnCardAdded);
        _deck.OnChildCardRemoved.AddListener(OnCardRemoved);

        foreach (var child in _deck.Children)
        {
            Logic(child).AddedToDeck(child, _deck, false);
        }

    }

    void OnCardAdded(CardAddedEventArgs args)
    {
        Logic(args.Card).AddedToDeck(args.Card, _deck, true);
    }

    void OnCardRemoved(CardRemovedEventArgs args)
    {
        Logic(args.Card).RemoveListenersAfterCardRemovedFromPlay(args.Card);
    }

    CardLogicBase Logic(Card card)
    {
        return CardBaseList.GetBaseLogic(card.id);
    }
}
