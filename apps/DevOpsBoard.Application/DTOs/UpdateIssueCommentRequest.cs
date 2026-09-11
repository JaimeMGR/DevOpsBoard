namespace DevOpsBoard.Application.DTOs;

public class UpdateIssueCommentRequest
{
    public string Content { get; set; } = string.Empty;

    public UpdateIssueCommentRequest()
    {
    }

    public UpdateIssueCommentRequest(
        string content)
    {
        Content = content;
    }
}