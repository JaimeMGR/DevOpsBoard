namespace DevOpsBoard.Application.DTOs;

public class IssueQueryParameters
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public string? AssigneeId { get; set; }

    public string? Search { get; set; }

    public string SortBy { get; set; } = "CreatedAt";

    public string SortDirection { get; set; } = "desc";

    public IssueQueryParameters()
    {
    }

    public IssueQueryParameters(
        int Page = 1,
        int PageSize = 20,
        string? Status = null,
        string? Priority = null,
        string? AssigneeId = null,
        string? Search = null,
        string SortBy = "CreatedAt",
        string SortDirection = "desc")
    {
        this.Page = Page;
        this.PageSize = PageSize;
        this.Status = Status;
        this.Priority = Priority;
        this.AssigneeId = AssigneeId;
        this.Search = Search;
        this.SortBy = SortBy;
        this.SortDirection = SortDirection;
    }
    
}