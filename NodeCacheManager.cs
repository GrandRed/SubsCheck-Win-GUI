using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public class NodeCacheManager<T>
{
    private readonly ConcurrentDictionary<string, T> _cache;
    private readonly string _cacheFile;

    public NodeCacheManager(string cacheFile)
    {
        _cache = new ConcurrentDictionary<string, T>();
        _cacheFile = cacheFile;
        LoadCache();
    }

    private void LoadCache()
    {
        if (File.Exists(_cacheFile))
        {
            var json = File.ReadAllText(_cacheFile);
            var items = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
            foreach (var item in items)
            {
                _cache[item.Key] = item.Value;
            }
        }
    }

    public void Add(string key, T value)
    {
        _cache[key] = value;
        SaveCache();
    }

    public T Get(string key)
    {
        return _cache.TryGetValue(key, out var value) ? value : default;
    }

    public void Remove(string key)
    {
        if (_cache.TryRemove(key, out _))
        {
            SaveCache();
        }
    }

    private void SaveCache()
    {
        var json = JsonConvert.SerializeObject(_cache,
            Formatting.Indented);
        File.WriteAllText(_cacheFile, json);
    }

    public string ComputeHash(string input)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}