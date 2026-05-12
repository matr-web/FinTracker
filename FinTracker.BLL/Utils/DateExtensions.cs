namespace FinTracker.BLL.Utils;

/// <summary>
/// Provides extension methods for the DateOnly and DateTime types
/// </summary>
public static class DateExtensions
{
    public static DateOnly ToFirstDayOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1);
    }

    public static DateOnly? ToFirstDayOfMonth(this DateOnly? date)
    {
        if (date == null) return null;
        return new DateOnly(date.Value.Year, date.Value.Month, 1);
    }

    public static DateOnly ToLastDayOfMonth(this DateOnly date)
    {
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        return new DateOnly(date.Year, date.Month, daysInMonth);
    }

    public static DateTime ToFirstDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime ToLastDayOfMonth(this DateTime date)
    {
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        return new DateTime(date.Year, date.Month, daysInMonth, date.Hour, date.Minute, date.Second);
    }
}
