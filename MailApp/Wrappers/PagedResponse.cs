namespace MailApp.Wrappers;

public class PagedResponse : ApiResponse
{
    public long PageNumber { get; set; }
    public long PageSize { get; set; }
    public Uri? FirstPage { get; set; }
    public Uri? LastPage { get; set; }
    public long TotalPages { get; set; }
    public long TotalRecords { get; set; }
    public Uri? PreviousPage { get; set; }
    public Uri? NextPage { get; set; }

    public PagedResponse(object data, int pageNumber, int pageSize) : base(data)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
