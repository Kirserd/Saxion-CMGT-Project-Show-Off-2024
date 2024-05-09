﻿using System.Collections.Generic;

public static class DataBridge
{
    private static Dictionary<string, object> _data = new();

    public static void MarkDataClean<T>(string key) where T : new()
    {
        if (!_data.ContainsKey(key))
            return;

        _data.TryGetValue(key, out var value);
        var unboxedData = (Data<T>)value;
        unboxedData.IsDirty = false;
        _data[key] = unboxedData;
    }
    public static void UpdateData<T>(string key, T data) where T : new()
    {
        if (_data.ContainsKey(key))
            _data[key] = new Data<T>(data) { IsDirty = true };
        else
            _data.Add(key, new Data<T>(data));
    }

    public static Data<T> TryGetData<T>(string key) where T : new()
    {
        if (_data.ContainsKey(key))
            return (Data<T>)_data[key];
        else
            return Data<T>.Empty;
    }
}