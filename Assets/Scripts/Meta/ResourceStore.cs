using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceStore : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public ResourceDefinition def;
        public int amount;
    }

    [Header("Config")]
    [Tooltip("Initial resources & references; order defines UI default order.")]
    public List<Entry> resources = new();

    public event Action<ResourceDefinition, int> OnChanged;

    public int Get(ResourceDefinition def)
    {
        var e = resources.Find(x => x.def == def);
        return e != null ? e.amount : 0;
    }

    public void Set(ResourceDefinition def, int value)
    {
        var e = resources.Find(x => x.def == def);
        if (e == null)
        {
            e = new Entry { def = def, amount = 0 };
            resources.Add(e);
        }
        if (e.amount == value) return;
        e.amount = value;
        OnChanged?.Invoke(def, e.amount);
    }

    public void Add(ResourceDefinition def, int delta) => Set(def, Get(def) + delta);

    public bool TrySpend(ResourceDefinition def, int cost)
    {
        int cur = Get(def);
        if (cur < cost) return false;
        Set(def, cur - cost);
        return true;
    }
}
