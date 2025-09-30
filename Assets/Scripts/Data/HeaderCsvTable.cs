using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using UnityEngine;

public abstract class HeaderCsvTable<T> : DataTable where T : class
{
    protected readonly Dictionary<int, T> byId = new();
    protected readonly List<T> cache = new();

    public IReadOnlyList<T> All => cache;
    public bool TryGetById(int id, out T data) => byId.TryGetValue(id, out data);
    public T GetById(int id) => byId[id];

    protected virtual string Locale => "ko-KR";
    protected virtual CultureInfo Culture => CultureInfo.InvariantCulture;

    protected abstract int GetId(T row);

    protected abstract T MapRow(IRowReader row, string locale);

    protected virtual void OnRowAdded(T item) { }

    protected virtual void OnAfterLoad() { }

    public override void Load(string filename)
    {
        byId.Clear();
        cache.Clear();

        var path = string.Format(FormatPath, filename);
        var ta = Resources.Load<TextAsset>(path);
        if (ta == null)
        {
            Debug.LogError($"{GetType().Name}: CSV not found at Resources/{path}.csv");
            return;
        }

        using var reader = new StringReader(ta.text);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var rr = csv.AsRowReader(Culture);
            var item = MapRow(rr, Locale);
            if (item == null) continue;

            var id = GetId(item);
            if (byId.ContainsKey(id))
            {
                Debug.LogError($"{GetType().Name}: duplicated ID {id}");
                continue;
            }

            byId[id] = item;
            cache.Add(item);
            OnRowAdded(item);
        }
        OnAfterLoad();
    }
}