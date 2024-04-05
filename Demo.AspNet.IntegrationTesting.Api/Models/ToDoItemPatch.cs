namespace Demo.AspNet.IntegrationTesting.Api.Models;

public class ToDoItemPatch
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsComplete { get; set; }
}
