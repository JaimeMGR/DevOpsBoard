namespace DevOpsBoard.Application.DTOs;

public class CreateIssueCommentRequest
{
    public string Content { get; set; } = string.Empty;

    public CreateIssueCommentRequest()
    {
    }

    public CreateIssueCommentRequest(
        string content)
    {
        Content = content;
    }
}