using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TopBarUI : MonoBehaviour
{
    [System.Serializable] public class ResourceSlot
    {
        public ResourceDefinition def;       // drag the SO here
        public Image icon;
        public TextMeshProUGUI valueText;
        public string format = "n0";         // e.g., "n0" or custom
    }

    [Header("Refs")]
    public ResourceStore store;
    public GameCalendar calendar;

    [Header("Resource Slots (3 items or more)")]
    public List<ResourceSlot> resourceSlots = new(); // size 3, assign defs/icons/labels

    [Header("Calendar UI")]
    public Image seasonDot;              // colored dot (optional)
    public TextMeshProUGUI seasonLabel;  // "Spring"
    public TextMeshProUGUI dayLabel;     // "Day 7 / 28 (Y1)"
    public Image dayProgressFill;        // Image with FillMethod=Horizontal
    public Slider dayProgressSlider;     // alternative if you prefer Slider

    [Header("Text Formats")]
    public string dayFormat = "Day {0} / {1}";
    public string yearSuffix = " (Y{0})";
    public string seasonUppercase = "none"; // "upper","lower","none"

    [Header("Auto Build (optional)")]
    public bool autoBuildRows = true;
    public Transform resourcesGroupParent;
    public ResourceEntryUI resourceRowPrefab;
    public bool buildFromStoreList = false;

    readonly Dictionary<ResourceDefinition, ResourceEntryUI> rows = new Dictionary<ResourceDefinition, ResourceEntryUI>();

    System.Action dayChangedHandler;
    System.Action seasonChangedHandler;
    System.Action yearChangedHandler;

    void OnEnable()
    {
        if (autoBuildRows) BuildRows();
        if (store != null) store.OnChanged += OnResourceChanged;
        RefreshAllResources();
        HookCalendar(true);
        RefreshCalendarUI(forceAll:true);
    }

    void OnDisable()
    {
        if (store != null) store.OnChanged -= OnResourceChanged;
        HookCalendar(false);
    }

    void Update()
    {
        if (calendar != null)
        {
            float p = Mathf.Clamp01(calendar.dayProgress01);
            if (dayProgressFill)  dayProgressFill.fillAmount = p;
            if (dayProgressSlider) dayProgressSlider.value   = p;
        }
    }

    void HookCalendar(bool on)
    {
        if (calendar == null) return;
        if (on)
        {
            dayChangedHandler ??= () => RefreshCalendarUI(false, true, false);
            seasonChangedHandler ??= () => RefreshCalendarUI(true, true, false);
            yearChangedHandler ??= () => RefreshCalendarUI(false, false, true);

            calendar.OnDayChanged += dayChangedHandler;
            calendar.OnSeasonChanged += seasonChangedHandler;
            calendar.OnYearChanged += yearChangedHandler;
        }
        else
        {
            if (dayChangedHandler != null) calendar.OnDayChanged -= dayChangedHandler;
            if (seasonChangedHandler != null) calendar.OnSeasonChanged -= seasonChangedHandler;
            if (yearChangedHandler != null) calendar.OnYearChanged -= yearChangedHandler;
        }
    }

    public void BuildRows()
    {
        rows.Clear();
        if (!resourcesGroupParent || !resourceRowPrefab) return;

        for (int i = resourcesGroupParent.childCount - 1; i >= 0; i--)
            Destroy(resourcesGroupParent.GetChild(i).gameObject);

        var list = new List<ResourceDefinition>();
        if (buildFromStoreList && store != null)
        {
            foreach (var e in store.resources)
            {
                if (e.def) list.Add(e.def);
            }
        }
        else
        {
            foreach (var slot in resourceSlots)
            {
                if (slot.def) list.Add(slot.def);
            }
        }

        foreach (var def in list)
        {
            var row = Instantiate(resourceRowPrefab, resourcesGroupParent);
            int v = store ? store.Get(def) : 0;
            row.Bind(def, v.ToString("n0"));
            rows[def] = row;
        }
    }

    void OnResourceChanged(ResourceDefinition def, int newValue)
    {
        if (autoBuildRows && rows.TryGetValue(def, out var row))
        {
            row.SetValue(newValue);
            return;
        }

        foreach (var slot in resourceSlots)
        {
            if (slot.def == def && slot.valueText)
                slot.valueText.text = newValue.ToString(slot.format);
        }
    }

    public void RefreshAllResources()
    {
        if (store == null) return;

        if (autoBuildRows && rows.Count > 0)
        {
            foreach (var kv in rows)
                kv.Value.SetValue(store.Get(kv.Key));
            return;
        }

        foreach (var slot in resourceSlots)
        {
            if (!slot.def) continue;
            if (slot.icon) slot.icon.sprite = slot.def.icon;
            if (slot.valueText) slot.valueText.text = store.Get(slot.def).ToString(slot.format);
        }
    }

    public void RefreshCalendarUI(bool seasonChanged = true, bool dayChanged = true, bool yearChanged = true, bool forceAll=false)
    {
        if (calendar == null) return;

        if (seasonChanged || forceAll)
        {
            string sName = calendar.CurrentSeasonName;
            if (seasonUppercase == "upper") sName = sName.ToUpperInvariant();
            else if (seasonUppercase == "lower") sName = sName.ToLowerInvariant();

            if (seasonLabel) seasonLabel.text = sName;
            if (seasonDot)   seasonDot.color  = calendar.CurrentSeasonColor;
        }

        if (dayChanged || yearChanged || forceAll)
        {
            string day = string.Format(dayFormat, calendar.DayInSeason, calendar.daysPerSeason);
            string year = string.Format(yearSuffix, calendar.Year);
            if (dayLabel) dayLabel.text = day + year;
        }

        // progress handled per-frame in Update() for smoothness
    }

    // Simple debug actions (hook to buttons if you want)
    public void AddGold(int n)    => TryAdd("gold", n);
    public void AddSpirit(int n)  => TryAdd("spirit", n);
    public void AddPopulation(int n) => TryAdd("population", n);

    void TryAdd(string id, int n)
    {
        if (store == null) return;
        var def = resourceSlots.Find(r => r.def && r.def.id == id)?.def;
        if (def) store.Add(def, n);
    }
}
