using System.Collections.Generic;
using UnityEngine;

public static class IconSpriteCache
{
    private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

    /// <summary>
    /// prefix + key 경로로 Resources에서 Sprite 로드 (확장자 제외), 캐시 사용.
    /// key가 null/empty면 null 반환.
    /// </summary>
    public static Sprite LoadSprite(string prefix, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        // 대소문자 민감 경로: CSV/파일명과 완전히 동일하게 맞추세요.
        string path = string.IsNullOrWhiteSpace(prefix) ? key : (prefix + key);

        if (_cache.TryGetValue(path, out var s) && s) return s;

        var loaded = Resources.Load<Sprite>(path);
        if (loaded)
        {
            _cache[path] = loaded;
            return loaded;
        }

        // 실패 캐시(원치 않으면 주석처리)
        _cache[path] = null;
        return null;
    }

    public static void Clear() => _cache.Clear();
}
