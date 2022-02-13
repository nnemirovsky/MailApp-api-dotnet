using MailApp.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace MailApp.Services;

public class UriService : IUriService
{
    private readonly string _baseUri;

    public UriService(IHttpContextAccessor contextAccessor)
    {
        var request = contextAccessor.HttpContext?.Request;
        _baseUri = string.Concat(request?.Scheme, "://", request?.Host.ToUriComponent());
    }

    public Uri GetPageUri(PaginationFilter filter, string route)
    {
        var displayNames = PaginationFilter.GetPropertiesDisplayNames();
        var endpointUri = new Uri(string.Concat(_baseUri, route));
        var modifiedUri =
            QueryHelpers.AddQueryString(endpointUri.ToString(), displayNames.pageNumber, filter.PageNumber.ToString());
        modifiedUri = QueryHelpers.AddQueryString(modifiedUri, displayNames.pageSize, filter.PageSize.ToString());
        return new Uri(modifiedUri);
    }
}
