using MailApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace MailApp.Filters;

public class PaginationFilter
{
    private readonly int _pageNumber;
    private readonly int _pageSize;

    [BindProperty(Name = "page_number")]
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    [BindProperty(Name = "page_size")]
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > 10 ? 10 : value;
    }

    public PaginationFilter()
    {
        PageNumber = 1;
        PageSize = 10;
    }

    public PaginationFilter(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static (string pageNumber, string pageSize) GetPropertiesDisplayNames()
    {
        var objType = typeof(PaginationFilter);
        var attrType = typeof(BindPropertyAttribute);

        var propName = nameof(PageNumber);

        var pageNumber =
            ((BindPropertyAttribute) AttributesHelper.GetPropertyAttribute(objType, propName, attrType)).Name!;

        propName = nameof(PageSize);

        var pageSize =
            ((BindPropertyAttribute) AttributesHelper.GetPropertyAttribute(objType, propName, attrType)).Name!;

        return (pageNumber, pageSize);
    }
}
