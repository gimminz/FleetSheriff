using System;

public static class AddressIconUtil
{
    /// <summary>
    /// CSV의 ItemAddress에서 마지막 토큰을 아이콘키로 추출.
    /// 예) "Sci-Fi Flat Skills / Tex_skill_77" -> "Tex_skill_77"
    /// 슬래시가 없으면 전체 트림 문자열 반환.
    /// </summary>
    public static string ExtractIconKey(string itemAddress)
    {
        if (string.IsNullOrWhiteSpace(itemAddress))
            return null;

        // 슬래시/백슬래시 모두 분리자 취급
        char[] sep = new[] { '/', '\\' };

        var trimmed = itemAddress.Trim();
        var parts = trimmed.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        // 마지막 토큰
        var key = parts[parts.Length - 1].Trim();

        // 확장자가 들어올 경우 제거 (안전)
        int dot = key.LastIndexOf('.');
        if (dot >= 0) key = key.Substring(0, dot);

        return string.IsNullOrWhiteSpace(key) ? null : key;
    }
}
