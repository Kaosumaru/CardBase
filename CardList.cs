﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class CardBaseList
{
    static protected Dictionary<string, CardLogicBase> _baseCards = new Dictionary<string, CardLogicBase>();
    public static CardLogicBase GetBaseLogic(string id)
    {
        _baseCards.TryGetValue(id, out var ret);
        if (ret == null)
        {
            Debug.Log($"Logic for '{id}' not found");
        }
        return ret;
    }
}
public class GenericCardList<CardLogicChild> : CardBaseList where CardLogicChild : CardLogicBase 
{
    static Dictionary<string, CardLogicChild> _cards = new Dictionary<string, CardLogicChild>();

    public static CardLogicChild GetLogic(string id)
    {
        _cards.TryGetValue(id, out var ret);
        return ret;
    }

    static public Dictionary<string, CardLogicChild> GetAllCards()
    {
        return _cards;
    }

    static protected void LoadCardsByReflection(Assembly assembly)
    {
        if (assembly == null) return;
        Type parentType = typeof(CardLogicChild);
        Type[] types = assembly.GetTypes();
        IEnumerable<Type> subclasses = types.Where(t => t.IsSubclassOf(parentType));

        foreach (Type type in subclasses)
        {
            var logic = (CardLogicChild)Activator.CreateInstance(type);
            if (string.IsNullOrEmpty(logic.id)) continue;
            AddCard(logic, true);
        }
    }

    static protected void AddCard(CardLogicChild logic, bool ignoreDuplicate = false)
    {
        Debug.Assert(logic.id != null, $"Card id is not set");

        if (!ignoreDuplicate && Application.isPlaying && _cards.ContainsKey(logic.id))
        {
            Debug.LogWarning($"{logic.id} already added to list!");
        }

        AddCard(logic.id, logic);
    }

    static protected void AddCards(IEnumerable<CardLogicChild> logic)
    {
        foreach (var card in logic)
        {
            AddCard(card);
        }
    }

    static protected void AddCard(string name, CardLogicChild logic)
    {
        _cards[name] = logic;
        _baseCards[name] = logic;
    }

    static protected void Deinitialize()
    {
        _cards.Clear();
        _baseCards.Clear();
    }
}
