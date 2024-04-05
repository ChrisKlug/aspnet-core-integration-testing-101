using Demo.AspNet.IntegrationTesting.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Demo.AspNet.IntegrationTesting.Api.Services;

public interface IToDoService
{
    Task<ToDoItem?> GetToDo(int id, bool includeCompleted = false);
    Task<ToDoItem[]> GetToDos(bool includeCompleted = false);
    Task<ToDoItem?> UpdateToDo(int id, string? title, string? description, bool? isCompleted);
}

public class DefaultToDoService : IToDoService
{
    private readonly ToDosDbContext ctx;

    public DefaultToDoService(ToDosDbContext ctx)
    {
        this.ctx = ctx;
    }

    public Task<ToDoItem?> GetToDo(int id, bool includeCompleted = false)
    {
        IQueryable<ToDoItem> query = ctx.Set<ToDoItem>().Where(x => x.Id == id);

        if (!includeCompleted)
            query = query.Where(x => x.Completed == null);

        return query.AsNoTracking().FirstOrDefaultAsync();
    }

    public Task<ToDoItem[]> GetToDos(bool includeCompleted = false)
    {
        IQueryable<ToDoItem> query = ctx.Set<ToDoItem>();

        if (!includeCompleted)
            query = query.Where(x => x.Completed == null);

        return query.AsNoTracking().ToArrayAsync();
    }

    public async Task<ToDoItem?> UpdateToDo(int id, string? title, string? description, bool? isCompleted)
    {
        var item = await ctx.Set<ToDoItem>().Where(x => x.Id == id).FirstOrDefaultAsync();

        if (item == null)
        {
            return null;
        }

        item.Title = title ?? item.Title;
        item.Description = description ?? item.Description;
        if (isCompleted == true && !item.IsCompleted)
        {
            item.Completed = DateTime.Now;
        }

        await ctx.SaveChangesAsync();

        return item;
    }
}
