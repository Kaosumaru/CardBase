using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Token
{
    [field: NonSerialized]
    public UnityEvent OnValueChanged = new UnityEvent();
    [field: NonSerialized]
    public UnityEvent OnEmptied = new UnityEvent();
    [field: NonSerialized]
    public UnityEvent OnSet = new UnityEvent();

    int _Value = 0;
    public int Value
    {
        get { return _Value; }
        set { Set(value); }
    }

    public Token()
    {

    }

    public Token(Token other)
    {
        _Value = other._Value;
    }

    public void Set(int value)
    {
        if (value < 0) value = 0;
        if (_Value == value) return;
        var previousValue = _Value;
        _Value = value;
        OnValueChanged?.Invoke();
        if (_Value == 0)
        {
            OnEmptied?.Invoke();
        }
        else if (previousValue == 0)
        {
            OnSet?.Invoke();
        }
    }

    public void Clear()
    {
        Set(0);
    }

    public void Increment(int value = 1)
    {
        Set(_Value + value);
    }

    public void IncrementMax(int value = 1, int max = 1)
    {
        var v = Mathf.Min(_Value + value, max);
        Set(v);
    }

    public bool SetIfEmpty(int value)
    {
        if (_Value != 0) return false;
        Set(value);
        return true;
    }

    public int Decrement(int value = 1)
    {
        var amount = Mathf.Min(_Value, value);
        Set(_Value - value);
        return amount;
    }

    public bool TryToDecrement(int value = 1)
    {
        if (Value < value) return false;
        Decrement(value);
        return true;
    }

    public bool DecrementIfNotEmpty(int value = 1)
    {
        if (_Value == 0) return false;
        Set(_Value - value);
        return _Value == 0;
    }

    public bool Empty()
    {
        return _Value == 0;
    }

    public bool NotEmpty()
    {
        return _Value != 0;
    }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
        OnValueChanged = new UnityEvent();
        OnEmptied = new UnityEvent();
        OnSet = new UnityEvent();
    }
}
