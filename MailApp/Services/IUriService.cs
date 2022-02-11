using MailApp.Filters;

namespace MailApp.Services;

public interface IUriService
{
    public Uri GetPageUri(PaginationFilter filter, string route);
}
