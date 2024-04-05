namespace Demo.AspNet.IntegrationTesting.Api.Data;

public class ToDoItem
{
    public int Id { get; private set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Completed { get; set; }
    public bool IsCompleted => Completed != null;
}

