using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CsvHelper;
public interface IRowReader
{
    bool Has(string header);

    bool TryGetString(string header, out string value);
    string GetString(string header, string @default = null);

    int GetInt(string header, int @default = 0);
    int? GetNullableInt(string header);

    float GetFloat(string header, float @default = 0f);
    float? GetNullableFloat(string header);
    string GetFirstString(params string[] headers);
}
public sealed class CsvRowReader : IRowReader
{
    private readonly CsvReader csv;
    private readonly Dictionary<string, int> indexMap;
    private readonly CultureInfo culture;

    public CsvRowReader(CsvReader csv, string[] headers, CultureInfo culture = null)
    {
        this.csv = csv;
        this.culture = culture ?? CultureInfo.InvariantCulture;
        indexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headers.Length; i++)
        {
            var raw = headers[i]?.Trim();
            if (string.IsNullOrEmpty(raw)) continue;

            if (!indexMap.ContainsKey(raw)) indexMap[raw] = i;

            var norm = NormalizeKey(raw);
            if (!indexMap.ContainsKey(norm)) indexMap[norm] = i;
        }
    }

    public bool Has(string header) => indexMap.ContainsKey(header) || indexMap.ContainsKey(NormalizeKey(header));

    private string Field(string header)
    {
        if (indexMap.TryGetValue(header, out var i) || indexMap.TryGetValue(NormalizeKey(header), out i))
            return csv.TryGetField(i, out string s) ? s?.Trim() : null;
        return null;
    }

    public bool TryGetString(string header, out string value)
    {
        value = Field(header);
        return !string.IsNullOrWhiteSpace(value);
    }

    public string GetString(string header, string @default = null)
    {
        var s = Field(header);
        return string.IsNullOrWhiteSpace(s) ? @default : s;
    }

    public int GetInt(string header, int @default = 0)
    {
        var s = Field(header);
        return int.TryParse(s, NumberStyles.Integer, culture, out var v) ? v : @default;
    }

    public int? GetNullableInt(string header)
    {
        var s = Field(header);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return int.TryParse(s, NumberStyles.Integer, culture, out var v) ? v : (int?)null;
    }

    public float GetFloat(string header, float @default = 0f)
    {
        var s = Field(header);
        return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var v) ? v : @default;
    }

    public float? GetNullableFloat(string header)
    {
        var s = Field(header);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var v) ? v : (float?)null;
    }

    public string GetFirstString(params string[] headers)
    {
        foreach (var h in headers)
        {
            var s = Field(h);
            if (!string.IsNullOrWhiteSpace(s)) return s;
        }
        return null;
    }

    private static string NormalizeKey(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            else sb.Append('_'); 
        }
        var norm = sb.ToString();
        while (norm.Contains("__")) norm = norm.Replace("__", "_");
        return norm.Trim('_');
    }
}

public static class CsvReaderRowExtensions
{
    public static IRowReader AsRowReader(this CsvReader csv, CultureInfo culture = null)
        => new CsvRowReader(csv, csv.HeaderRecord, culture ?? CultureInfo.InvariantCulture);
}