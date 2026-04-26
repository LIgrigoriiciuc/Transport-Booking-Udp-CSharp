namespace Shared.Util;

public static class DateTimeUtils
{
    public const string Format = "yyyy-MM-dd HH:mm";
    public static string FormatDateTime(DateTime? dt) => dt.HasValue ? dt.Value.ToString(Format) : "";
    public static DateTime Parse(string? dt)
    {
        if (dt == null) return default;
        if (DateTime.TryParseExact(dt, Format, null, System.Globalization.DateTimeStyles.None, out var result))
            return result;
        throw new ArgumentException("Invalid date format. Please use dd-MM-yyyy HH:mm");
    }
}