using Microsoft.AspNetCore.Mvc;

namespace MailApp.Filters;

public class DateFilter
{
    [BindProperty(Name = "start_date")] public DateTime? Start { get; init; }

    [BindProperty(Name = "end_date")] public DateTime? End { get; init; }

    public DateFilter()
    { }

    public DateFilter(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }
}
