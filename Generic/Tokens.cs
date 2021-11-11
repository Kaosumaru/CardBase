﻿using System;
using System.Collections.Generic;


[System.Serializable]
public class Tokens
{
    protected Dictionary<Guid, Token> tokens = new Dictionary<Guid, Token>();

    public Token Token(Guid tokenType)
    {
        tokens.TryGetValue(tokenType, out var token);
        if (token == null)
        {
            token = new Token();
            tokens[tokenType] = token;
        }
        return token;
    }

    public int TokenValue(Guid tokenType)
    {
        tokens.TryGetValue(tokenType, out var token);
        if (token == null) return 0;
        return token.Value;
    }

    public bool HasToken(Guid tokenType)
    {
        return tokens.ContainsKey(tokenType);
    }

    public Tokens Clone()
    {
        var res = new Tokens();
        foreach(var entry in tokens)
        {
            res.tokens[entry.Key] = new Token(entry.Value);
        }
        return res;
    }

    public void Clear()
    {
        foreach (var entry in tokens)
        {
            entry.Value.Clear();
        }
    }
}