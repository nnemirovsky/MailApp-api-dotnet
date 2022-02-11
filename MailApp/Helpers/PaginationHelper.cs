using MailApp.Filters;
using MailApp.Services;
using MailApp.Wrappers;

namespace MailApp.Helpers;

public static class PaginationHelper
{
    public static PaginatedApiResponse CreatePagedResponse(object pagedData, PaginationFilter filter, long totalRecords,
        IUriService uriService, string route)
    {
        var response = new PaginatedApiResponse(pagedData, filter.PageNumber, filter.PageSize);
        var totalPages = (double) totalRecords / filter.PageSize;
        int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

        if (filter.PageNumber >= 1 && filter.PageNumber < roundedTotalPages)
            response.NextPage = uriService.GetPageUri(
                new PaginationFilter(filter.PageNumber + 1, filter.PageSize), route);
        if (filter.PageNumber - 1 >= 1 && filter.PageNumber <= roundedTotalPages)
            response.PreviousPage = uriService.GetPageUri(
                new PaginationFilter(filter.PageNumber - 1, filter.PageSize), route);

        response.FirstPage = uriService.GetPageUri(new PaginationFilter(1, filter.PageSize), route);
        response.LastPage = uriService.GetPageUri(
            new PaginationFilter(roundedTotalPages, filter.PageSize), route);
        response.TotalPages = roundedTotalPages;
        response.TotalRecords = totalRecords;
        return response;
    }
}
