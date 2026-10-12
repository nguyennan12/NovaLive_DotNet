using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NovaLive.Application.Common.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // 1. Chuyển chữ thường
        var normalizedString = text.ToLowerInvariant().Trim();

        // 2. Chuyển chữ Đ / đ sang d
        normalizedString = normalizedString.Replace("đ", "d").Replace("Đ", "d");

        // 3. Chuẩn hóa bỏ dấu Unicode FormD
        var stringBuilder = new StringBuilder();
        var normalizedD = normalizedString.Normalize(NormalizationForm.FormD);

        foreach (var c in normalizedD)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var cleanText = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

        // 4. Thay thế ký tự không hợp lệ bằng dấu gạch ngang
        cleanText = Regex.Replace(cleanText, @"[^a-z0-9\s-]", "");
        cleanText = Regex.Replace(cleanText, @"\s+", "-");
        cleanText = Regex.Replace(cleanText, @"-+", "-");

        return cleanText.Trim('-');
    }
}
